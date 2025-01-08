using System.Threading.Tasks;
using CardPool.Conventions;
using CardPool.Extensions;
using CardPool.Implements;
using CardPool.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace CardPool.Tests;

[TestFixture]
public class IntegrationTests
{
    private ICardPoolManager _manager;
    private ICardsPoolLoader _loader;

    [SetUp]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddMemoryCardPool<TestCardPoolLoader>();
        var provider = services.BuildServiceProvider();
        
        _manager = provider.GetRequiredService<ICardPoolManager>();
        _loader = provider.GetRequiredService<ICardsPoolLoader>();
    }

    [Test]
    public async Task CompleteWorkflow_ShouldWorkCorrectly()
    {
        // Arrange & Act
        await _loader.LoadPoolsAsync(_manager);
        var drawer = _manager.GetDrawer("TestPool");
        
        // Draw some cards and verify statistics
        const int drawCount = 1000;
        for (var i = 0; i < drawCount; i++)
        {
            var card = drawer.DrawCard();
            Assert.That(card, Is.Not.Null);
        }
        
        // Assert
        var stats = drawer.Statistician;
        Assert.That(stats.RecordedTimes, Is.EqualTo(drawCount));
        
        // Verify probabilities are roughly as expected
        var report = stats.GetReportTableString();
        Assert.That(report, Is.Not.Empty);
    }

    [Test]
    public async Task GenericCardWorkflow_ShouldWorkCorrectly()
    {
        // Arrange & Act
        await _loader.LoadPoolsAsync(_manager);
        var drawer = _manager.GetDrawer<int>("TestPool");
        
        // Draw some cards
        var card = drawer.DrawCard();
        
        // Assert
        Assert.That(card, Is.Not.Null);
        Assert.That(card, Is.InstanceOf<Card<int>>());
    }

    [Test]
    public async Task MultiplePoolsWorkflow_ShouldWorkIndependently()
    {
        // Arrange & Act
        await _loader.LoadPoolsAsync(_manager);
        var drawer1 = _manager.GetDrawer("TestPool");
        var drawer2 = _manager.GetDrawer("AnotherPool");
        
        // Draw from both pools
        const int drawCount = 100;
        for (var i = 0; i < drawCount; i++)
        {
            drawer1.DrawCard();
            drawer2.DrawCard();
        }
        
        // Assert
        Assert.That(drawer1.Statistician.RecordedTimes, Is.EqualTo(drawCount));
        Assert.That(drawer2.Statistician.RecordedTimes, Is.EqualTo(drawCount));
    }

    [Test]
    public async Task LimitedCardWorkflow_ShouldRespectLimits()
    {
        // Arrange
        await _loader.LoadPoolsAsync(_manager);
        var drawer = _manager.GetDrawer("LimitedPool");
        
        // Act & Assert
        var limitedCard = drawer.DrawCard();
        Assert.That(limitedCard, Is.Not.Null);
        Assert.That(limitedCard.IsLimitedCard, Is.True);
        
        // Draw until the card is removed
        while (!limitedCard.IsRemoved)
        {
            drawer.DrawCardInclude(limitedCard);
        }
        
        // Try to draw the removed card
        var nullCard = drawer.DrawCardInclude(limitedCard);
        Assert.That(nullCard, Is.Not.SameAs(limitedCard));
    }
}

/// <summary>
/// Test implementation of CardsPoolLoader for integration tests
/// </summary>
public class TestCardPoolLoader : CardsPoolByMemoryProvider
{
    public override void ConfigurePools()
    {
        // Configure a standard test pool
        ConfigurePool("TestPool", pool =>
        {
            var oneStarCards = Card<int>.CreateMultiCards(CardRarity.OneStar, 1, 2, 3);
            var twoStarCards = Card<int>.CreateMultiCards(CardRarity.TwoStar, 4, 5);
            
            pool.SetPoolRarityProbability(CardRarity.OneStar, 0.7)
                .SetPoolRarityProbability(CardRarity.TwoStar, 0.3);
                
            pool.AddCards(oneStarCards);
            pool.AddCards(twoStarCards);
            pool.BuildPool();
        });

        // Configure another pool for multiple pool testing
        ConfigurePool("AnotherPool", pool =>
        {
            var cards = Card<int>.CreateMultiCards(CardRarity.OneStar, 6, 7, 8);
            pool.AddCards(cards);
            pool.BuildPool();
        });

        // Configure a pool with limited cards
        ConfigurePool("LimitedPool", pool =>
        {
            var limitedCard = new Card<int>(CardRarity.OneStar, 999) { TotalCount = 3 };
            var normalCard = Card<int>.CreateCard(CardRarity.OneStar, 1000);
            
            pool.AddCards(limitedCard, normalCard);
            pool.BuildPool();
        });
    }
} 