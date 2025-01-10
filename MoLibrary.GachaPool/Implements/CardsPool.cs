using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MoLibrary.GachaPool.Conventions;
using MoLibrary.GachaPool.Interfaces;

namespace MoLibrary.GachaPool.Implements;

/// <summary>
/// Represents a pool of cards with probability-based drawing functionality.
/// </summary>
public class CardsPool : ICardsPool
{
    private static readonly Random RandomSeed = new();
    private static readonly Lock RandomLock = new();
    /// <summary>
    /// The whole probability setting of cards which have the same specific rarity.
    /// </summary>
    public Dictionary<CardRarity, double> RarityProbabilitySetting { get; } = new();

    /// <summary>
    /// The all cards in pool. 
    /// </summary>
    public IReadOnlyList<Card> Cards => _cards.ToList();

    private bool _needBuildPool;

    private readonly ReaderWriterLockSlim _buildPoolLockSlim = new(LockRecursionPolicy.SupportsRecursion);

    /// <summary>
    /// Limited card will make draw method be serial.
    /// </summary>
    private bool _containLimitedCard;

    /// <summary>
    /// The cards in pool. the cards are ordered by rarity ascending.
    /// </summary>
    private readonly HashSet<Card> _cards = [];

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
            if (cardList == null) continue;
            foreach (var card in cardList)
            {
                _cards.Add(card);
            }
        }
    }

    #region AlterPoolCards

    /// <summary>
    /// Marks one or more cards as removed from the pool.
    /// </summary>
    /// <param name="cards">The cards to mark as removed.</param>
    public void RemoveCards(params IEnumerable<Card> cards)
    {
        _buildPoolLockSlim.EnterWriteLock();
        try
        {
            _needBuildPool = true;
            foreach (var card in cards)
            {
                if (_cards.Contains(card))
                {
                    card.IsRemoved = true;
                }
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
    public void AddCards(params IEnumerable<Card> cards)
    {
        _buildPoolLockSlim.EnterWriteLock();
        try
        {
            _needBuildPool = true;
            foreach (var card in cards)
            {
                _cards.Add(card);
            }
        }
        finally
        {
            _buildPoolLockSlim.ExitWriteLock();
        }
    }

    #endregion

    /// <summary>
    /// Sets the probability for a specific card rarity in the pool.
    /// </summary>
    /// <param name="rarity">The rarity to set probability for.</param>
    /// <param name="probability">The probability value to set.</param>
    /// <returns>The current instance for method chaining.</returns>
    public ICardsPool SetPoolRarityProbability(CardRarity rarity, double probability)
    {
        _buildPoolLockSlim.EnterWriteLock();
        try
        {
            _needBuildPool = true;
            RarityProbabilitySetting[rarity] = probability;
            return this;
        }
        finally
        {
            _buildPoolLockSlim.ExitWriteLock();
        }
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

        var searchLine = new (double, Card)[list.Count];
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
            var remainedCard = new NothingCard();
            if (_cards.TryGetValue(remainedCard, out var existedCard) && existedCard is NothingCard existedNotingCard)
            {
                remainedCard = existedNotingCard;
            }
            else
            {
                _cards.Add(remainedCard);
            }
            
            var orderedCards = _cards.OrderBy(card => card.Rarity).ToList();
            foreach (var card in orderedCards)
            {
                card.RealProbability = 0;
                if (card is { IsRemoved: false, PresetProbability: { } setProbability })
                {
                    card.RealProbability = setProbability;
                }
            }

            var validCards = orderedCards.Where(p => !p.IsNotingCard).Where(c => !c.IsRemoved).ToList();
            _containLimitedCard = validCards.Any(c => c is { IsLimitedCard: true, IsRemoved: false });
            
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
            else if (remainingProbability > 0)
            {
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
                    remainedCard.RealProbability = remainingProbability;
                    validCards.Add(remainedCard);
                }
            }

          
            
            // Create binary search line with valid cards
            SearchLine = CreateBinarySearchLine(validCards);
        }
        finally
        {
            _buildPoolLockSlim.ExitWriteLock();
        }
    }

    private void EnsureBuildPool()
    {
        if (!_needBuildPool) return;
        _buildPoolLockSlim.EnterWriteLock();
        try
        {
            if (_needBuildPool) BuildPool();
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
        EnsureBuildPool();
        var header = $"{"CardName",-20}{"ExpectProb",-15}{"Rarity",-10}\n";
        var separator = new string('-', 45) + "\n";
        var rows = Cards.Select(c => $"{c.GetCardName(),-20}{c.RealProbability,-15:P4}{c.Rarity,-10}");
        return header + separator + string.Join("\n", rows) + "\n" +
               separator + $"sum of all probability: {Cards.Sum(c => c.RealProbability)}";
    }


    /// <summary>
    /// Internal method for drawing a card from the pool.
    /// </summary>
    /// <param name="customSearchLine">Optional custom search line for drawing.</param>
    /// <returns>The drawn card.</returns>
    public Card InternalDrawCard(BinarySearchLine? customSearchLine = null)
    {
        EnsureBuildPool();

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