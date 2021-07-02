using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;

namespace CardPool.Core
{
    public class CardsPool
    {
        private Card _remainedCard;
        private static readonly Random RandomSeed = new();
        /// <summary>
        /// The whole probability setting of cards which have the same specific rarity.
        /// </summary>
        public Dictionary<Card.CardRarity, double> RarityProbabilitySetting { get; } = new();

        /// <summary>
        /// The specific rarity cards index interval for rarity drawing. 
        /// </summary>
        public Dictionary<Card.CardRarity, KeyValuePair<int, int>> RarityInterval { get; } = new();

        /// <summary>
        /// The all cards in pool. 
        /// </summary>
        public IReadOnlyList<Card> Cards => _cards;

        private bool _needBuildPool;
        
        private readonly ReaderWriterLockSlim _buildPoolLockSlim = new(LockRecursionPolicy.SupportsRecursion);
        
        /// <summary>
        /// The remaining probability card, which usually means worst lucky and gets nothing, but you can set it as a
        /// specific card. If not set, the default remained card will be the first least probability card (leftmost card).
        /// You can set the card as null, which means drawer may draws out null (return NothingCard).
        /// This card's global probability will be auto assigned when initialize the cards pool.
        /// </summary>
        public Card RemainedCard
        {
            get => _remainedCard;
            set
            {
                _remainedCard = value ?? new NothingCard();
                _notingCardBeTheFirstValidCard = false;
            }
        }

        /// <summary>
        /// Because of the precision of double, there always a remaining probability value.
        /// </summary>
        private bool _notingCardBeTheFirstValidCard = true;
        /// <summary>
        /// Limited card will make draw method be serial.
        /// </summary>
        private bool _containLimitedCard;
        private List<Card> _cards;
        
        private BinarySearchLine SearchLine { get; set; }

        public CardsPool()
        {
            
        }
        public CardsPool(IEnumerable<Card> cards, params IEnumerable<Card>[] appendCards)
        {
            _cards = cards.ToList();
            if (appendCards == null) return;
            foreach (var cardList in appendCards)
            {
                _cards.AddRange(cardList.ToList());
            }
        }

        #region AlterPoolCards

        public void RemoveCards(Card card, params Card[] moreCards)
        {
            _buildPoolLockSlim.EnterWriteLock();
            try
            {
                _needBuildPool = true;
                if(card == null) return;
                _cards ??= new List<Card>();
                _cards.Remove(card);
                if (moreCards != null)
                {
                    foreach (var appendedCard in moreCards)
                    {
                        if (appendedCard == null) continue;
                        _cards.Remove(appendedCard);
                    }
                }
            }
            finally
            {
                _buildPoolLockSlim.ExitWriteLock();
            }
        }
        
        public void AddCards(Card card, params Card[] moreCards)
        {
            _buildPoolLockSlim.EnterWriteLock();
            try
            {
                _needBuildPool = true;
                if(card == null) return;
                _cards ??= new List<Card>();
                _cards.Add(card);
                if (moreCards != null)
                {
                    foreach (var appendedCard in moreCards)
                    {
                        if (appendedCard == null) continue;
                        _cards.Add(appendedCard);
                    }
                }
            }
            finally
            {
                _buildPoolLockSlim.ExitWriteLock();
            }
        }
        public void AddCards(IEnumerable<Card> cards)
        {
            _buildPoolLockSlim.EnterWriteLock();
            try
            {
                _needBuildPool = true;
                if(cards == null) return;
                _cards ??= new List<Card>();
                _cards.AddRange(cards);
            }
            finally
            {
                _buildPoolLockSlim.ExitWriteLock();
            }
        }

        #endregion
        
        public CardsPool SetPoolRarityProbability(Card.CardRarity rarity, double totalProbability)
        {
            _buildPoolLockSlim.EnterWriteLock();
            try
            {
                _needBuildPool = true;
                RarityProbabilitySetting[rarity] = totalProbability;
            }
            finally
            {
                _buildPoolLockSlim.ExitWriteLock();
            }
            return this;
        }
        
        /// <summary>
        /// Sort out the added cards, adjust their probability and build the proper pool. The build is auto but also can
        /// invoke actively.
        /// </summary>
        /// <exception cref="InvalidOperationException">The invalid cards in pool may curse some problems.</exception>
        [SuppressMessage("ReSharper", "PossibleInvalidOperationException")]
        public void BuildPool()
        {
            _buildPoolLockSlim.EnterWriteLock();
            try
            {
                _needBuildPool = false;
                if (_cards == null || _cards.Count == 0) throw new InvalidOperationException("Cards pool is empty");
                _cards = _cards.OrderBy(card => card.Rarity).ToList();
                _containLimitedCard = _cards.Any(c => c.IsLimitedCard);
                var cards = _cards;
                // fulfill all cards with global probability.
                foreach (var (rarity, wholeProbability) in RarityProbabilitySetting)
                {
                    if (wholeProbability == 0) continue;
                    var cardsWithSameRarity = cards.Where(c => c.Rarity == rarity && c.Probability == null).ToList();
                    var count = cardsWithSameRarity.Count;
                    var perProbability = (wholeProbability - cardsWithSameRarity
                        .Where(c => c.ProbabilityOfSameRarityPool != null)
                        .Sum(c => c.ProbabilityOfSameRarityPool).Value) / count;
                    foreach (var card in cardsWithSameRarity)
                    {
                        if (card.ProbabilityOfSameRarityPool != null)
                        {
                            card.Probability = wholeProbability * card.ProbabilityOfSameRarityPool;
                        }
                        else
                        {
                            card.Probability = perProbability;
                        }
                    }
                }

                var remainingProbability = 1 - cards.Sum(c => c.Probability).Value;
                if (remainingProbability < 0)
                    throw new InvalidOperationException("The cards pool total probability is out of 100%");
                var cardsHaveNotSetProbability = cards.Where(c => c.Probability == null).ToList();
                if (cardsHaveNotSetProbability.Count > 0)
                {
                    var perCardProbability = remainingProbability / cardsHaveNotSetProbability.Count;
                    foreach (var card in cardsHaveNotSetProbability)
                    {
                        card.Probability = perCardProbability;
                    }

                    remainingProbability = 0;
                }



                var searchLine = new Dictionary<double, Card>();
                double probabilityIndex = 0;
                probabilityIndex += remainingProbability;

                var curRarity = cards.First().Rarity;
                var curRarityStartIndex = 0;
                foreach (var (card, index) in cards.Select((v, i) => (v, i)))
                {
                    searchLine.Add(probabilityIndex, card);
                    probabilityIndex += card.Probability.Value;
                    //record rarity info
                    if (card.Rarity != curRarity)
                    {
                        RarityInterval.Add(curRarity, new KeyValuePair<int, int>(curRarityStartIndex, index - 1));
                        curRarityStartIndex = index;
                        curRarity = card.Rarity;
                    }

                }

                if (remainingProbability != 0)
                {
                    if (_notingCardBeTheFirstValidCard)
                    {
                        _remainedCard = cards.First();
                        _remainedCard.Probability += remainingProbability;
                    }
                    else if (_remainedCard is NothingCard nothingCard)
                    {
                        nothingCard.Probability = remainingProbability;
                    }
                }

                SearchLine = new BinarySearchLine
                {
                    LeftMostCard = _remainedCard,
                    CardsBinarySearchLine = searchLine
                };
            }
            finally
            {
                _buildPoolLockSlim.ExitWriteLock();
            }
        }

        public string GetPoolProbabilityInfo()
        {
            if (_needBuildPool)
            {
                _buildPoolLockSlim.EnterWriteLock();
                try
                {
                    if(_needBuildPool) BuildPool();
                }
                finally
                {
                    _buildPoolLockSlim.ExitWriteLock();
                }
            }
            string append = null;
            if (_remainedCard != null) append = $"[RemainedCard] {_remainedCard}\n";
            return append + string.Join('\n', Cards);
        }
        
        internal Card InternalDrawCard(int? startIndex = null, int? endIndex = null)
        {
            if (_needBuildPool)
            {
                _buildPoolLockSlim.EnterWriteLock();
                try
                {
                    if(_needBuildPool) BuildPool();
                }
                finally
                {
                    _buildPoolLockSlim.ExitWriteLock();
                }
            }

            if (_containLimitedCard)
            {
                _buildPoolLockSlim.EnterUpgradeableReadLock();
            }
            else
            {
                _buildPoolLockSlim.EnterReadLock();
            }

            try
            {
                var card = startIndex == null || endIndex == null
                    ? SearchLine.Search(RandomSeed.NextDouble())
                    : SearchLine.Search(RandomSeed.NextDouble(), startIndex.Value, endIndex.Value);
                if (card.IsLimitedCard)
                {
                    if (card.SuccessGetCard())
                    {
                        if (card.RemainCount == 0)
                        {
                            card.Probability = 0;
                            _buildPoolLockSlim.EnterWriteLock();
                            try
                            {
                                BuildPool();
                            }
                            finally
                            {
                                _buildPoolLockSlim.ExitWriteLock();
                            }
                        }
                        return card;
                    }

                    throw new Exception("Need redesign the multi thread frame");//theoretically impossible.
                }
                return card;
            }
            finally
            {
                if (_containLimitedCard)
                {
                    _buildPoolLockSlim.ExitUpgradeableReadLock();
                }
                else
                {
                    _buildPoolLockSlim.ExitReadLock();
                }
            }
        }
    }
}