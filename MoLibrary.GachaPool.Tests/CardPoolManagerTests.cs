using System.Linq;
using MoLibrary.GachaPool.Conventions;
using MoLibrary.GachaPool.Implements;
using MoLibrary.GachaPool.Interfaces;
using NUnit.Framework;

namespace MoLibrary.GachaPool.Tests;

[TestFixture]
public class CardPoolManagerTests
{
    private ICardPoolManager _manager;
    private const string TestPoolName = "TestPool";

    [SetUp]
    public void Setup()
    {
        _manager = new CardPoolManager();
    }

    [Test]
    public void AddOrUpdatePool_WhenPoolAdded_ShouldBeRetrievable()
    {
        // Arrange
        var pool = new CardsPool();
        
        // Act
        _manager.AddOrUpdatePool(TestPoolName, pool);
        var retrievedPool = _manager.GetPool(TestPoolName);
        
        // Assert
        Assert.That(retrievedPool, Is.Not.Null);
        Assert.That(retrievedPool, Is.SameAs(pool));
    }

    [Test]
    public void GetPool_WhenPoolDoesNotExist_ShouldReturnNull()
    {
        // Act
        var pool = _manager.GetPool("NonExistentPool");
        
        // Assert
        Assert.That(pool, Is.Null);
    }

    [Test]
    public void RemovePool_WhenPoolExists_ShouldRemoveAndReturnTrue()
    {
        // Arrange
        var pool = new CardsPool();
        _manager.AddOrUpdatePool(TestPoolName, pool);
        
        // Act
        var result = _manager.RemovePool(TestPoolName);
        var retrievedPool = _manager.GetPool(TestPoolName);
        
        // Assert
        Assert.That(result, Is.True);
        Assert.That(retrievedPool, Is.Null);
    }

    [Test]
    public void GetDrawer_WhenPoolExists_ShouldReturnDrawer()
    {
        // Arrange
        var pool = new CardsPool();
        _manager.AddOrUpdatePool(TestPoolName, pool);
        
        // Act
        var drawer = _manager.GetDrawer(TestPoolName);
        
        // Assert
        Assert.That(drawer, Is.Not.Null);
        Assert.That(drawer.Pool, Is.SameAs(pool));
    }

    [Test]
    public void GetGenericDrawer_WhenPoolExists_ShouldReturnTypedDrawer()
    {
        // Arrange
        var pool = new CardsPool();
        var card = Card<int>.CreateCard(CardRarity.OneStar, 1);
        pool.AddCards(card);
        _manager.AddOrUpdatePool(TestPoolName, pool);
        
        // Act
        var drawer = _manager.GetDrawer<int>(TestPoolName);
        
        // Assert
        Assert.That(drawer, Is.Not.Null);
        Assert.That(drawer.Pool, Is.SameAs(pool));
    }

    [Test]
    public void GetPoolNames_WhenPoolsExist_ShouldReturnAllNames()
    {
        // Arrange
        var poolNames = new[] { "Pool1", "Pool2", "Pool3" };
        foreach (var name in poolNames)
        {
            _manager.AddOrUpdatePool(name, new CardsPool());
        }
        
        // Act
        var retrievedNames = _manager.GetPoolNames().ToArray();
        
        // Assert
        Assert.That(retrievedNames, Is.EquivalentTo(poolNames));
    }
} 