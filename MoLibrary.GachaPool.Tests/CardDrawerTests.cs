using System.Linq;
using MoLibrary.GachaPool.Conventions;
using MoLibrary.GachaPool.Implements;
using MoLibrary.GachaPool.Interfaces;
using NUnit.Framework;

namespace MoLibrary.GachaPool.Tests;

[TestFixture]
public class CardDrawerTests
{
    private ICardsPool _pool;
    private ICardDrawer _drawer;
    private Card<int>[] _oneStarCards;
    private Card<int>[] _twoStarCards;

    [SetUp]
    public void Setup()
    {
        _pool = new CardsPool();
        _drawer = new CardDrawer(_pool);
        
        // Create test cards
        _oneStarCards = Card<int>.CreateMultiCards(CardRarity.OneStar, 1, 2).ToArray();
        _twoStarCards = Card<int>.CreateMultiCards(CardRarity.TwoStar, 3, 4).ToArray();
        
        // Configure pool
        _pool.SetPoolRarityProbability(CardRarity.OneStar, 0.6)
             .SetPoolRarityProbability(CardRarity.TwoStar, 0.4);
        
        _pool.AddCards(_oneStarCards);
        _pool.AddCards(_twoStarCards);
        _pool.BuildPool();
    }

    [Test]
    public void DrawCard_ShouldReturnCardFromPool()
    {
        // Act
        var card = _drawer.DrawCard();
        
        // Assert
        Assert.That(card, Is.Not.Null);
        Assert.That(_pool.Cards, Does.Contain(card));
    }

    [Test]
    public void DrawCard_WithRarity_ShouldReturnCardOfSpecifiedRarity()
    {
        // Act
        var card = _drawer.DrawCard(CardRarity.TwoStar);
        
        // Assert
        Assert.That(card, Is.Not.Null);
        Assert.That(card.Rarity, Is.EqualTo(CardRarity.TwoStar));
    }

    [Test]
    public void DrawCardInclude_ShouldOnlyDrawSpecifiedCards()
    {
        // Arrange
        var includedCards = new[] { _oneStarCards[0] };
        
        // Act
        var card = _drawer.DrawCardInclude(includedCards);
        
        // Assert
        Assert.That(card, Is.Not.Null);
        Assert.That(includedCards, Does.Contain(card));
    }

    [Test]
    public void DrawCardExcept_ShouldNotDrawExcludedCards()
    {
        // Arrange
        var excludedCards = new[] { _oneStarCards[0], _twoStarCards[0] };
        
        // Act
        var card = _drawer.DrawCardExcept(excludedCards);
        
        // Assert
        Assert.That(card, Is.Not.Null);
        Assert.That(excludedCards, Does.Not.Contain(card));
    }

    [Test]
    public void Statistician_ShouldTrackDrawnCards()
    {
        // Arrange
        const int drawCount = 100;
        
        // Act
        for (var i = 0; i < drawCount; i++)
        {
            _drawer.DrawCard();
        }
        
        // Assert
        Assert.That(_drawer.Statistician.RecordedTimes, Is.EqualTo(drawCount));
        Assert.That(_drawer.Statistician.CardRecordDict.Values.Sum(v => v.Value), Is.EqualTo(drawCount));
    }

    [Test]
    public void GenericDrawer_ShouldReturnTypedCards()
    {
        // Arrange
        var genericDrawer = new CardDrawer<int>(_pool);
        
        // Act
        var card = genericDrawer.DrawCard();
        
        // Assert
        Assert.That(card, Is.Not.Null);
        Assert.That(card, Is.InstanceOf<Card<int>>());
    }

    [Test]
    public void DrawCard_WithRemovedCard_ShouldNotDrawRemovedCard()
    {
        // Arrange
        var cardToRemove = _oneStarCards[0];
        _pool.RemoveCards(cardToRemove);
        const int drawCount = 100;
        
        // Act
        var drawnCards = Enumerable.Range(0, drawCount)
            .Select(_ => _drawer.DrawCard())
            .ToList();
        
        // Assert
        Assert.That(drawnCards, Does.Not.Contain(cardToRemove), "Should not draw removed card");
        Assert.That(cardToRemove.IsRemoved, Is.True, "Card should be marked as removed");
        Assert.That(_pool.Cards, Does.Contain(cardToRemove), "Removed card should still be in pool");
    }

    [Test]
    public void DrawCard_WithLimitedCard_ShouldRespectLimitCount()
    {
        // Arrange
        const int limitCount = 5;
        var limitedCard = new Card<int>(CardRarity.OneStar, 999) { TotalCount = limitCount };
        _pool.AddCards(limitedCard);
        _pool.BuildPool();
        
        // Act
        var drawnLimitedCards = Enumerable.Range(0, limitCount + 5)
            .Select(_ => _drawer.DrawCardInclude(limitedCard))
            .Where(card => card == limitedCard)
            .ToList();
        
        // Assert
        Assert.That(drawnLimitedCards.Count, Is.EqualTo(limitCount));
        Assert.That(limitedCard.IsRemoved, Is.True);
    }
} 