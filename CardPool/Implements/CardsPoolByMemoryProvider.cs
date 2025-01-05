using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CardPool.Conventions;
using CardPool.Interfaces;

namespace CardPool.Implements;

/// <summary>
/// Provides functionality to load card pools from memory.
/// </summary>
public class CardsPoolByMemoryProvider : ICardsPoolLoader
{
    private readonly Dictionary<string, Action<CardsPool>> _poolConfigurations = new();

    /// <summary>
    /// Adds a pool configuration that will be used to create a card pool with the specified name.
    /// </summary>
    /// <param name="poolName">The name of the pool to create.</param>
    /// <param name="configure">The action to configure the pool.</param>
    public void AddPoolConfiguration(string poolName, Action<CardsPool> configure)
    {
        _poolConfigurations[poolName] = configure;
    }

    /// <summary>
    /// Adds a pool configuration for a specific card type.
    /// </summary>
    /// <typeparam name="T">The type of cards in the pool.</typeparam>
    /// <param name="poolName">The name of the pool to create.</param>
    /// <param name="cards">The cards to add to the pool.</param>
    /// <param name="configure">Optional additional configuration for the pool.</param>
    public void AddPoolConfiguration<T>(string poolName, IEnumerable<Card<T>> cards, Action<CardsPool>? configure = null) where T : Card<T>
    {
        _poolConfigurations[poolName] = pool =>
        {
            pool.AddCards(cards.ToArray<Card?>());
            configure?.Invoke(pool);
            pool.BuildPool();
        };
    }

    /// <inheritdoc />
    public Task LoadPoolsAsync(ICardPoolManager manager)
    {
        foreach (var (poolName, configure) in _poolConfigurations)
        {
            var pool = new CardsPool();
            configure(pool);
            manager.AddOrUpdatePool(poolName, pool);
        }

        return Task.CompletedTask;
    }
} 