using System.Collections.Generic;
using System.Linq;
using CardPool.Conventions;
using CardPool.Implements;
using CardPool.Interfaces;
using NUnit.Framework;

namespace CardPool.Tests;

[TestFixture]
public class CardDrawStatisticianTests
{
    private ICardsPool _pool;
    private ICardDrawStatistician _statistician;
    private List<Card<int>> _testCards;

    [SetUp]
    public void Setup()
    {
        _pool = new CardsPool();
        _testCards = Card<int>.CreateMultiCards(CardRarity.OneStar, 1, 2, 3);
        _pool.AddCards(_testCards);
        _pool.BuildPool();
        _statistician = new CardDrawStatistician(_pool);
    }

    [Test]
    public void Constructor_ShouldInitializeWithAllPoolCards()
    {
        // Assert
        Assert.That(_statistician.CardRecordDict.Keys, Is.EquivalentTo(_pool.Cards));
        Assert.That(_statistician.CardRecordDict.Values.All(v => v.Value == 0));
    }

    [Test]
    public void RecordDrawnCard_ShouldIncrementCardCount()
    {
        // Arrange
        var card = _testCards[0];
        
        // Act
        _statistician.RecordDrawnCard(card);
        
        // Assert
        Assert.That(_statistician.CardRecordDict[card].Value, Is.EqualTo(1));
        Assert.That(_statistician.RecordedTimes, Is.EqualTo(1));
    }

    [Test]
    public void RecordDrawnCard_MultipleTimes_ShouldTrackCorrectly()
    {
        // Arrange
        var card = _testCards[0];
        const int drawCount = 5;
        
        // Act
        for (var i = 0; i < drawCount; i++)
        {
            _statistician.RecordDrawnCard(card);
        }
        
        // Assert
        Assert.That(_statistician.CardRecordDict[card].Value, Is.EqualTo(drawCount));
        Assert.That(_statistician.RecordedTimes, Is.EqualTo(drawCount));
    }

    [Test]
    public void GetReportTableString_ShouldIncludeAllCards()
    {
        // Arrange
        foreach (var card in _testCards)
        {
            _statistician.RecordDrawnCard(card);
        }
        
        // Act
        var report = _statistician.GetReportTableString();
        
        // Assert
        foreach (var card in _testCards)
        {
            Assert.That(report, Does.Contain(card.GetCardName()));
        }
    }

    [Test]
    public void GetReportTableString_ShouldShowCorrectProbabilities()
    {
        // Arrange
        const int drawCount = 100;
        var targetCard = _testCards[0];
        
        for (var i = 0; i < drawCount; i++)
        {
            _statistician.RecordDrawnCard(targetCard);
        }
        
        // Act
        var report = _statistician.GetReportTableString();
        
        // Assert
        Assert.That(report, Does.Contain("100.00%")); // Since we only drew one card
        Assert.That(report, Does.Contain(drawCount.ToString()));
    }

    [Test]
    public void RecordDrawnCard_WithRemainedCard_ShouldTrackCorrectly()
    {
        // Arrange
        var remainedCard = new Card<int>(CardRarity.OneStar, 999);
        _pool.RemainedCard = remainedCard;
        var statistician = new CardDrawStatistician(_pool); // Create new statistician after setting remained card
        
        // Act
        statistician.RecordDrawnCard(remainedCard);
        
        // Assert
        Assert.That(statistician.CardRecordDict.ContainsKey(remainedCard));
        Assert.That(statistician.CardRecordDict[remainedCard].Value, Is.EqualTo(1));
        Assert.That(statistician.RecordedTimes, Is.EqualTo(1));
    }

    [Test]
    public void RecordDrawnCard_WithMultipleThreads_ShouldBeThreadSafe()
    {
        // Arrange
        var card = _testCards[0];
        const int threadCount = 10;
        const int drawsPerThread = 1000;
        var threads = new System.Threading.Thread[threadCount];
        
        // Act
        for (var i = 0; i < threadCount; i++)
        {
            threads[i] = new System.Threading.Thread(() =>
            {
                for (var j = 0; j < drawsPerThread; j++)
                {
                    _statistician.RecordDrawnCard(card);
                }
            });
            threads[i].Start();
        }
        
        foreach (var thread in threads)
        {
            thread.Join();
        }
        
        // Assert
        Assert.That(_statistician.CardRecordDict[card].Value, Is.EqualTo(threadCount * drawsPerThread));
        Assert.That(_statistician.RecordedTimes, Is.EqualTo(threadCount * drawsPerThread));
    }
} 