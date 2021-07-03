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
                _buildPoolLockSlim.EnterWriteLock();
                try
                {
                    _needBuildPool = true;
                    _remainedCard = value ?? new NothingCard();
                    _notingCardBeTheFirstValidCard = false;
                }
                finally
                {
                    _buildPoolLockSlim.ExitWriteLock();
                }
                
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
            _needBuildPool = true;
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
        
        /// <summary>
        /// Set the given rarity total probability, and the probability will arrange to corresponding card according to
        /// each set. (Default is dividing equally)
        /// </summary>
        /// <param name="rarity"></param>
        /// <param name="totalProbability"></param>
        /// <returns></returns>
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
        /// Only when all the card in pool has the real probability.
        /// </summary>
        /// <param name="removedProbability">Total probability of cards which have been removed from pool</param>
        private void RemoveCardProbability(double removedProbability)
        {
            _buildPoolLockSlim.EnterWriteLock();
            try
            {
                
                var perCardGet = removedProbability / _cards.Count(c => !c.IsFixedRealProbability && !c.IsRemoved);
                foreach (var card in _cards)
                {
                    if(card.IsFixedRealProbability || card.IsRemoved) continue;
                    card.RealProbability += perCardGet;
                }
                var remainingProbability = 1 - _cards.Sum(c => c.RealProbability);
                CreateBinarySearchLine(remainingProbability);
            }
            finally
            {
                _buildPoolLockSlim.ExitWriteLock();
            }
        }
        
        private void CreateBinarySearchLine(double remainingProbability)
        {
            var searchLine = new KeyValuePair<double, Card>[_cards.Count];
            double probabilityIndex = 0;
            probabilityIndex += remainingProbability;

            var curRarity = _cards.First().Rarity;
            var curRarityStartIndex = 0;
            foreach (var (card, index) in _cards.Select((v, i) => (v, i)))
            {
                searchLine[index] = new KeyValuePair<double, Card>(probabilityIndex, card);
                probabilityIndex += card.RealProbability;
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
                    _remainedCard = _cards.First();
                    _remainedCard.RealProbability += remainingProbability;
                }
                else if (_remainedCard is NothingCard nothingCard)
                {
                    nothingCard.RealProbability = remainingProbability;
                }
            }

            SearchLine = new BinarySearchLine
            {
                LeftMostCard = _remainedCard,
                CardsBinarySearchLine = searchLine
            };
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
                // fulfill all cards with global probability.
                foreach (var (rarity, wholeProbability) in RarityProbabilitySetting)
                {
                    if (wholeProbability == 0) continue;
                    var cardsWithSameRarity = _cards.Where(c => c.Rarity == rarity && c.SetProbability == null).ToList();
                    var count = cardsWithSameRarity.Count;
                    var perProbability = (wholeProbability - cardsWithSameRarity
                        .Where(c => c.ProbabilityOfSameRarityPool != null)
                        .Sum(c => c.ProbabilityOfSameRarityPool).Value) / count;
                    foreach (var card in cardsWithSameRarity)
                    {
                        if (card.ProbabilityOfSameRarityPool != null)
                        {
                            card.RealProbability = wholeProbability * card.ProbabilityOfSameRarityPool.Value;
                        }
                        else
                        {
                            card.RealProbability = perProbability;
                        }
                    }
                }

                var remainingProbability = 1 - _cards.Sum(c => c.RealProbability);
                if (remainingProbability < 0)
                    throw new InvalidOperationException("The cards pool total probability is out of 100%");
                var cardsHaveNotSetProbability = _cards.Where(c => c.SetProbability == null).ToList();
                if (cardsHaveNotSetProbability.Count > 0)
                {
                    var perCardProbability = remainingProbability / cardsHaveNotSetProbability.Count;
                    foreach (var card in cardsHaveNotSetProbability)
                    {
                        card.RealProbability = perCardProbability;
                    }

                    remainingProbability = 0;
                }
                CreateBinarySearchLine(remainingProbability);
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

            string append = $"sum of all probability: {Cards.Sum(c => c.RealProbability)}\n";
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
                            _buildPoolLockSlim.EnterWriteLock();
                            try
                            {
                                var tmp = card.RealProbability;
                                card.IsRemoved = true;
                                RemoveCardProbability(tmp);
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