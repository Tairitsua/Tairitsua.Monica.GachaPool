using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CardPool.Conventions;
using CardPool.Interfaces;

namespace CardPool.Implements;

/// <summary>
/// Base class for memory-based card pool loaders.
/// </summary>
public abstract class CardsPoolByMemoryProvider : ICardsPoolLoader
{
    private readonly Dictionary<string, Action<CardsPool>> _poolConfigurations = new();

    /// <summary>
    /// Configures the card pools that will be loaded.
    /// This method should be implemented by derived classes to set up their specific pool configurations.
    /// </summary>
    public abstract void ConfigurePools();

    /// <summary>
    /// Protected method for derived classes to configure a pool with custom configuration.
    /// </summary>
    /// <param name="poolName">The name of the pool to create.</param>
    /// <param name="configure">The action to configure the pool.</param>
    protected void ConfigurePool(string poolName, Action<CardsPool> configure)
    {
        _poolConfigurations[poolName] = configure;
    }

    /// <summary>
    /// Protected method for derived classes to configure a pool with specific card type.
    /// </summary>
    /// <typeparam name="T">The type of cards in the pool.</typeparam>
    /// <param name="poolName">The name of the pool to create.</param>
    /// <param name="cards">The cards to add to the pool.</param>
    /// <param name="configure">Optional additional configuration for the pool.</param>
    protected void ConfigurePool<T>(string poolName, IEnumerable<Card<T>> cards, Action<CardsPool>? configure = null) where T : notnull
    {
        _poolConfigurations[poolName] = pool =>
        {
            pool.AddCards(cards.ToArray<Card>());
            configure?.Invoke(pool);
            pool.BuildPool();
        };
    }

    /// <inheritdoc />
    public Task LoadPoolsAsync(ICardPoolManager manager)
    {
        // Ensure pools are configured
        if (_poolConfigurations.Count == 0)
        {
            ConfigurePools();
        }

        foreach (var (poolName, configure) in _poolConfigurations)
        {
            var pool = new CardsPool();
            configure(pool);
            manager.AddOrUpdatePool(poolName, pool);
        }

        return Task.CompletedTask;
    }
} 