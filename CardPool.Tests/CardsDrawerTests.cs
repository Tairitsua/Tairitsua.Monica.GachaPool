using System;
using System.Linq;
using CardPool.Conventions;
using CardPool.Implements;
using NUnit.Framework;

namespace CardPool.Tests;

/// <summary>
/// Tests for the CardDrawer functionality.
/// </summary>
public class CardsDrawerTests
{
    private CardsPool _pool;

    /// <summary>
    /// Sets up the test environment with a basic card pool containing one-star cards.
    /// </summary>
    [SetUp]
    public void Setup()
    {
        var oneStarCards = Card<int>.CreateMultiCards(CardRarity.OneStar,
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
        _pool = new CardsPool(oneStarCards);
        _pool.BuildPool();
        Console.WriteLine(_pool.GetPoolProbabilityInfo());
        Console.WriteLine("sum of all probability: "+_pool.Cards.Sum(c=>c.RealProbability));
    }

    /// <summary>
    /// Tests the probability distribution of card draws.
    /// </summary>
    [Test]
    public void ProbabilityTest()
    {
           
    }
}