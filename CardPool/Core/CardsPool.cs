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
    /// Create the binary search line for drawing card.
    /// </summary>
    /// <param name="list"></param>
    public static BinarySearchLine CreateBinarySearchLine(List<Card> list)
    {
        //sum probability to normalize probability to 1
        var sum = list.Sum(c => c.RealProbability);

        if (1 - sum < 1e-15)
        {
            sum = 1;
        }

        var searchLine = new(double, Card)[list.Count];
        double probabilityIndex = 0;
        foreach (var (card, index) in list.Select((v, i) => (v, i)))
        {
            searchLine[index] = new ValueTuple<double, Card>(probabilityIndex, card);
            probabilityIndex += card.RealProbability / sum;
        }

        return new BinarySearchLine
        {
            LeftMostCard = list.First(),
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
            foreach (var card in _cards)
            {
                card.RealProbability = 0;
                if (card is { IsRemoved: false, PresetProbability: { } setProbability })
                {
                    card.RealProbability = setProbability;
                }
            }

            var validCards = _cards.Where(c => !c.IsRemoved).ToList();
            _containLimitedCard = validCards.Any(c => c is {IsLimitedCard: true, IsRemoved: false});
            // fulfill all cards with global probability.
            foreach (var (rarity, wholeProbability) in RarityProbabilitySetting)
            {
                if (wholeProbability == 0) continue;
                var cardsWithSameRarity = validCards.Where(c => c.Rarity == rarity && c.PresetProbability == null).ToList();


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
            
            var remainingProbability = 1 - validCards.Sum(c => c.RealProbability);
            if (remainingProbability < 0)
            {
                throw new InvalidOperationException("remainingProbability can not be below zero, which means the cards pool total probability is out of 100%");
            }

            if (validCards.Count(c => c.RealProbability == 0) is var count and > 0)
            {
                var averageProbability = remainingProbability / count;
                foreach (var card in validCards.Where(c => c.RealProbability == 0))
                {
                    card.RealProbability = averageProbability;
                }
            }
            else
            {
                if (_remainedCard is {} remainedCard)
                {
                    remainedCard.RealProbability += remainingProbability;
                    if (!validCards.Contains(remainedCard))
                    {
                        validCards.Add(remainedCard);
                    }
                }
            }
            
            SearchLine = CreateBinarySearchLine(validCards);
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
    /// <returns>The drawn card.</returns>
    internal Card InternalDrawCard(BinarySearchLine? customSearchLine = null)
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

            var card = (customSearchLine ?? SearchLine).Search(randomNum);
            if (card.IsLimitedCard)
            {
                if (card.SuccessGetCard())
                {
                    if (card.RemainCount == 0)
                    {
                        _buildPoolLockSlim.EnterWriteLock();
                        try
                        {
                            card.IsRemoved = true;
                            _needBuildPool = true;
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