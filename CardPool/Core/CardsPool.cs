using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace CardPool.Core;

/// <summary>
/// Represents a pool of cards with probability-based drawing functionality.
/// </summary>
public class CardsPool
{
    private Card? _remainedCard;
    private static readonly Random RandomSeed = new();
    private static readonly Lock RandomLock = new();
    /// <summary>
    /// The whole probability setting of cards which have the same specific rarity.
    /// </summary>
    public Dictionary<CardRarity, double> RarityProbabilitySetting { get; } = new();

    /// <summary>
    /// The all cards in pool. 
    /// </summary>
    public IReadOnlyList<Card> Cards => _cards;

    private bool _needBuildPool;
        
    private readonly ReaderWriterLockSlim _buildPoolLockSlim = new(LockRecursionPolicy.SupportsRecursion);
        
    /// <summary>
    /// The remaining probability card, which usually means the worst lucky and gets nothing, but you can set it as a
    /// specific card. If not set(means remained card is null), the default remained card will be the first least probability card (leftmost card).
    /// You can set the card as null, which means drawer may draw out null (return NothingCard).
    /// This card's global probability will be auto assigned when initialize the cards pool.
    /// </summary>
    public Card? RemainedCard
    {
        get => _remainedCard;
        set
        {
            _buildPoolLockSlim.EnterWriteLock();
            try
            {
                _needBuildPool = true;
                _remainedCard = value ?? new NothingCard();
            }
            finally
            {
                _buildPoolLockSlim.ExitWriteLock();
            }
                
        }
    }

    /// <summary>
    /// Limited card will make draw method be serial.
    /// </summary>
    private bool _containLimitedCard;

    /// <summary>
    /// The cards in pool. the cards are ordered by rarity ascending.
    /// </summary>
    private List<Card> _cards = [];

    private BinarySearchLine SearchLine { get; set; } = null!;

    /// <summary>
    /// Initializes a new instance of the CardsPool class.
    /// </summary>
    public CardsPool()
    {
            
    }

    /// <summary>
    /// Initializes a new instance of the CardsPool class with initial cards. 
    /// </summary>
    /// <param name="cards">The initial collection of cards.</param>
    public CardsPool(params IEnumerable<Card>?[]? cards)
    {
        _needBuildPool = true;
        if (cards == null) return;
        foreach (var cardList in cards)
        {
            if(cardList == null) continue;
            _cards.AddRange(cardList.ToList());
        }
    }

    #region AlterPoolCards

    /// <summary>
    /// Removes one or more cards from the pool.
    /// </summary>
    /// <param name="cards">The cards to remove.</param>
    public void RemoveCards(params Card?[] cards)
    {
        _buildPoolLockSlim.EnterWriteLock();
        try
        {
            _needBuildPool = true;
            foreach (var appendedCard in cards)
            {
                if (appendedCard == null) continue;
                _cards.Remove(appendedCard);
            }
        }
        finally
        {
            _buildPoolLockSlim.ExitWriteLock();
        }
    }
        
    /// <summary>
    /// Adds one or more cards to the pool.
    /// </summary>
    /// <param name="cards">The cards to add.</param>
    public void AddCards(params Card?[] cards)
    {
        _buildPoolLockSlim.EnterWriteLock();
        try
        {
            _needBuildPool = true;
            foreach (var appendedCard in cards)
            {
                if (appendedCard == null) continue;
                _cards.Add(appendedCard);
            }
        }
        finally
        {
            _buildPoolLockSlim.ExitWriteLock();
        }
    }

    /// <summary>
    /// Adds a collection of cards to the pool.
    /// </summary>
    /// <param name="cards">The collection of cards to add.</param>
    public void AddCards(IEnumerable<Card>? cards)
    {
        _buildPoolLockSlim.EnterWriteLock();
        try
        {
            _needBuildPool = true;
            if(cards == null) return;
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
    public CardsPool SetPoolRarityProbability(CardRarity rarity, double totalProbability)
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
            _containLimitedCard = _cards.Any(c => c is {IsLimitedCard: true, IsRemoved: false});
            //TODO should add probability due to the ratio for each card.
            var perCardGet = removedProbability / _cards.Count(c => c is {IsFixedRealProbability: false, IsRemoved: false});
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

    /// <summary>
    /// Create the binary search line for drawing card.
    /// </summary>
    /// <param name="remainingProbability">The remaining probability to be assigned to the leftmost card.</param>
    private void CreateBinarySearchLine(double remainingProbability)
    {
        if (remainingProbability < 0)
        {
            throw new InvalidOperationException("remainingProbability can not be below zero, which means the cards pool total probability is out of 100%");
        }

        var searchLine = new(double, Card)[_cards.Count];
        double probabilityIndex = 0;
        probabilityIndex += remainingProbability;

        foreach (var (card, index) in _cards.Select((v, i) => (v, i)))
        {
            searchLine[index] = new ValueTuple<double, Card>(probabilityIndex, card);
            probabilityIndex += card.RealProbability;
        }

        if (_remainedCard is NothingCard nothingCard)
        {
            nothingCard.RealProbability = remainingProbability;
        }
        else
        {
            _remainedCard = _cards.First();
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
    public void BuildPool()
    {
        _buildPoolLockSlim.EnterWriteLock();
        try
        {
            _needBuildPool = false;
            if (_cards == null || _cards.Count == 0) throw new InvalidOperationException("Cards pool is empty");
            _cards = _cards.OrderBy(card => card.Rarity).ToList();
            _containLimitedCard = _cards.Any(c => c is {IsLimitedCard: true, IsRemoved: false});
            // fulfill all cards with global probability.
            foreach (var (rarity, wholeProbability) in RarityProbabilitySetting)
            {
                if (wholeProbability == 0) continue;
                var cardsWithSameRarity = _cards.Where(c => c.Rarity == rarity && c.SetProbability == null).ToList();


                if (cardsWithSameRarity.Count == 0) continue;
                var perProbability = (wholeProbability - (cardsWithSameRarity
                    .Where(c => c.RatioAmountSameRarity != null)
                    .Sum(c => c.RatioAmountSameRarity) ?? 0)) / cardsWithSameRarity.Count;
                foreach (var card in cardsWithSameRarity)
                {
                    if (card.RatioAmountSameRarity != null)
                    {
                        card.RealProbability = wholeProbability * card.RatioAmountSameRarity.Value;
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

    /// <summary>
    /// Gets a string representation of the pool's probability information.
    /// </summary>
    /// <returns>A string containing the pool's probability information.</returns>
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

        var append = $"sum of all probability: {Cards.Sum(c => c.RealProbability)}\n";
        if (_remainedCard != null) append = $"[RemainedCard] {_remainedCard}\n";
        return append + string.Join('\n', Cards);
    }
        
    /// <summary>
    /// Internal method for drawing a card from the pool.
    /// </summary>
    /// <param name="startIndex">Optional starting index for drawing from a specific range.</param>
    /// <param name="endIndex">Optional ending index for drawing from a specific range.</param>
    /// <returns>The drawn card.</returns>
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

        var useUpgradeable = _containLimitedCard;
        if (useUpgradeable)
        {
            _buildPoolLockSlim.EnterUpgradeableReadLock();
        }
        else
        {
            _buildPoolLockSlim.EnterReadLock();
        }

        try
        {
            double randomNum;
            lock (RandomLock)
            {
                randomNum = RandomSeed.NextDouble();
            }
            var card = startIndex == null || endIndex == null
                ? SearchLine.Search(randomNum)
                : SearchLine.Search(randomNum, startIndex.Value, endIndex.Value);
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

                throw new Exception("Tell author to redesign the multi thread frame");//theoretically impossible.
            }
            return card;
        }
        finally
        {
            if (useUpgradeable)
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