using System;
using System.Linq;
using CardPool.Conventions;
using CardPool.Implements;
using CardPool.Interfaces;
using NUnit.Framework;

namespace CardPool.Tests;

[TestFixture]
public class CardsPoolTests
{
    private ICardsPool _pool;

    [SetUp]
    public void Setup()
    {
        _pool = new CardsPool();
    }

    [Test]
    public void AddCards_WhenCardsAdded_ShouldBeInPool()
    {
        // Arrange
        var card1 = Card<int>.CreateCard(CardRarity.OneStar, 1);
        var card2 = Card<int>.CreateCard(CardRarity.TwoStar, 2);
        
        // Act
        _pool.AddCards(card1, card2);
        
        // Assert
        Assert.That(_pool.Cards, Has.Count.EqualTo(2));
        Assert.That(_pool.Cards, Contains.Item(card1));
        Assert.That(_pool.Cards, Contains.Item(card2));
    }

    [Test]
    public void RemoveCards_WhenCardsRemoved_ShouldNotBeInPool()
    {
        // Arrange
        var card1 = Card<int>.CreateCard(CardRarity.OneStar, 1);
        var card2 = Card<int>.CreateCard(CardRarity.TwoStar, 2);
        _pool.AddCards(card1, card2);
        
        // Act
        _pool.RemoveCards(card1);
        
        // Assert
        Assert.That(_pool.Cards, Has.Count.EqualTo(1));
        Assert.That(_pool.Cards, Does.Not.Contain(card1));
        Assert.That(_pool.Cards, Contains.Item(card2));
    }

    [Test]
    public void SetPoolRarityProbability_WhenProbabilitySet_ShouldBeReflectedInSettings()
    {
        // Arrange
        const double probability = 0.7;
        
        // Act
        _pool.SetPoolRarityProbability(CardRarity.OneStar, probability);
        
        // Assert
        Assert.That(_pool.RarityProbabilitySetting[CardRarity.OneStar], Is.EqualTo(probability));
    }

    [Test]
    public void BuildPool_WhenCardsAdded_ShouldCalculateCorrectProbabilities()
    {
        // Arrange
        _pool.SetPoolRarityProbability(CardRarity.OneStar, 0.7)
             .SetPoolRarityProbability(CardRarity.TwoStar, 0.3);
        
        var oneStarCards = Card<int>.CreateMultiCards(CardRarity.OneStar, 1, 2);
        var twoStarCards = Card<int>.CreateMultiCards(CardRarity.TwoStar, 3);
        _pool.AddCards(oneStarCards);
        _pool.AddCards(twoStarCards);
        
        // Act
        _pool.BuildPool();
        
        // Assert
        var oneStarTotalProb = oneStarCards.Sum(c => c.RealProbability);
        var twoStarTotalProb = twoStarCards.Sum(c => c.RealProbability);
        
        Assert.That(oneStarTotalProb, Is.EqualTo(0.7).Within(0.0001));
        Assert.That(twoStarTotalProb, Is.EqualTo(0.3).Within(0.0001));
    }

    [Test]
    public void RemainedCard_WhenSet_ShouldBeRetrievable()
    {
        // Arrange
        var remainedCard = Card<int>.CreateCard(CardRarity.OneStar, 999);
        
        // Act
        _pool.RemainedCard = remainedCard;
        
        // Assert
        Assert.That(_pool.RemainedCard, Is.SameAs(remainedCard));
    }

    [Test]
    public void InternalDrawCard_ShouldRespectProbabilities()
    {
        // Arrange
        const int drawCount = 10000;
        const double oneStarProb = 0.7;
        const double twoStarProb = 0.3;
        
        _pool.SetPoolRarityProbability(CardRarity.OneStar, oneStarProb)
             .SetPoolRarityProbability(CardRarity.TwoStar, twoStarProb);
        
        var oneStarCards = Card<int>.CreateMultiCards(CardRarity.OneStar, 1, 2);
        var twoStarCards = Card<int>.CreateMultiCards(CardRarity.TwoStar, 3);
        _pool.AddCards(oneStarCards);
        _pool.AddCards(twoStarCards);
        _pool.BuildPool();
        
        // Act
        var drawnCards = Enumerable.Range(0, drawCount)
            .Select(_ => _pool.InternalDrawCard())
            .ToList();
        
        // Assert
        var oneStarDraws = drawnCards.Count(c => c.Rarity == CardRarity.OneStar);
        var twoStarDraws = drawnCards.Count(c => c.Rarity == CardRarity.TwoStar);
        
        var actualOneStarProb = (double)oneStarDraws / drawCount;
        var actualTwoStarProb = (double)twoStarDraws / drawCount;
        
        // Allow for some statistical variance
        Assert.That(actualOneStarProb, Is.EqualTo(oneStarProb).Within(0.02));
        Assert.That(actualTwoStarProb, Is.EqualTo(twoStarProb).Within(0.02));
    }

    [Test]
    public void GetPoolProbabilityInfo_ShouldReturnFormattedString()
    {
        // Arrange
        var card = Card<int>.CreateCard(CardRarity.OneStar, 1);
        _pool.AddCards(card);
        _pool.SetPoolRarityProbability(CardRarity.OneStar, 1.0);
        _pool.BuildPool();
        
        // Act
        var info = _pool.GetPoolProbabilityInfo();
        
        // Assert
        Assert.That(info, Is.Not.Empty);
        Assert.That(info, Does.Contain(card.GetCardName()));
        Assert.That(info, Does.Contain("100.00%"));
    }
} 