using System;
using System.Collections.Generic;
using System.Linq;
using CardPool.Conventions;
using CardPool.Implements;
using NUnit.Framework;

namespace CardPool.Tests;

/// <summary>
/// Tests for CardsPool initialization and setup functionality.
/// </summary>
public class CardsPoolInitializeTests
{
    /// <summary>
    /// Tests card pool initialization with a single card with zero probability.
    /// </summary>
    [Test]
    public void TestUnexpectedCreate4()
    {
        var pool = new CardsPool();
        var card = new Card<int>(1)
        {
            PresetProbability = 0
        };
        pool.RemainedCard = null;
        pool.AddCards(card);
        pool.BuildPool();
        Console.WriteLine(pool.GetPoolProbabilityInfo());
        Console.WriteLine("sum of all probability: "+pool.Cards.Sum(c=>c.RealProbability));
    }
    /// <summary>
    /// Tests card pool initialization with a single one-star card.
    /// </summary>
    [Test]
    public void TestUnexpectedCreate2()
    {
        var oneStarCards = Card<int>.CreateMultiCards(CardRarity.OneStar, 1);
        var pool = new CardsPool(oneStarCards);
        pool.BuildPool();
        Console.WriteLine(pool.GetPoolProbabilityInfo());
        Console.WriteLine("sum of all probability: "+pool.Cards.Sum(c=>c.RealProbability));
    }
    /// <summary>
    /// Tests card pool initialization with a single generic card.
    /// </summary>
    [Test]
    public void TestUnexpectedCreate3()
    {
        var cards = new List<Card> {new Card<int>(1)};
        var pool = new CardsPool(cards);
        pool.BuildPool();
        Console.WriteLine(pool.GetPoolProbabilityInfo());
        Console.WriteLine("sum of all probability: "+pool.Cards.Sum(c=>c.RealProbability));
    }
    /// <summary>
    /// Tests card pool initialization with multiple cards of different rarities and types.
    /// </summary>
    [Test]
    public void TestRarityCreate1()
    {
        var oneStarCards = Card<int>.CreateMultiCards(CardRarity.OneStar, 1, 2, 3, 4, 5);
        var twoStarCards = Card<int>.CreateMultiCards(CardRarity.TwoStar, 11, 12, 13, 14, 15);
        var singleCard = new Card<int>(100) {PresetProbability = 0.005};
        var twoStarCardsAppend = Card<TimeSpan>.CreateMultiCards(CardRarity.TwoStar,
            new TimeSpan(1, 0, 0),
            new TimeSpan(1, 1, 0),
            new TimeSpan(1, 1, 10));
        var pool = new CardsPool(oneStarCards, twoStarCards, twoStarCardsAppend);
        pool.AddCards(singleCard);
        pool.RemainedCard = null;
        pool.SetPoolRarityProbability(CardRarity.OneStar, 0.5)
            .SetPoolRarityProbability(CardRarity.TwoStar, 0.3);
        pool.BuildPool();
        Console.WriteLine(pool.GetPoolProbabilityInfo());
        Console.WriteLine("sum of all probability: "+pool.Cards.Sum(c=>c.RealProbability));
    }
}