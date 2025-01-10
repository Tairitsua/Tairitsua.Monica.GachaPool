using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using MoLibrary.GachaPool.Conventions;
using MoLibrary.GachaPool.Interfaces;

namespace MoLibrary.GachaPool.Implements;

/// <summary>
/// Manages multiple card pools and provides access to them by name.
/// </summary>
public class CardPoolManager : ICardPoolManager
{
    private readonly ConcurrentDictionary<string, ICardsPool> _pools = new();
    private readonly ConcurrentDictionary<string, ICardDrawer> _drawers = new();

    /// <inheritdoc />
    public ICardsPool? GetPool(string poolName)
    {
        return _pools.GetValueOrDefault(poolName);
    }

    /// <inheritdoc />
    public ICardDrawer? GetDrawer(string poolName)
    {
        if (_drawers.TryGetValue(poolName, out var drawer))
        {
            return drawer;
        }

        if (_pools.TryGetValue(poolName, out var pool))
        {
            drawer = new CardDrawer(pool);
            _drawers.TryAdd(poolName, drawer);
            return drawer;
        }

        return null;
    }

    /// <inheritdoc />
    public ICardDrawer<T>? GetDrawer<T>(string poolName) where T : notnull
    {
        if (_drawers.TryGetValue(poolName, out var drawer))
        {
            return drawer as ICardDrawer<T>;
        }

        if (_pools.TryGetValue(poolName, out var pool))
        {
            var typedDrawer = new CardDrawer<T>(pool);
            _drawers.TryAdd(poolName, typedDrawer);
            return typedDrawer;
        }

        return null;
    }

    /// <inheritdoc />
    public IEnumerable<string> GetPoolNames()
    {
        return _pools.Keys.ToList();
    }

    /// <inheritdoc />
    public void AddOrUpdatePool(string poolName, ICardsPool pool)
    {
        _pools.AddOrUpdate(poolName, pool, (_, _) => pool);
        // Remove any existing drawer for this pool as it needs to be recreated
        _drawers.TryRemove(poolName, out _);
    }

    /// <inheritdoc />
    public bool RemovePool(string poolName)
    {
        var poolRemoved = _pools.TryRemove(poolName, out _);
        _drawers.TryRemove(poolName, out _);
        return poolRemoved;
    }
} 