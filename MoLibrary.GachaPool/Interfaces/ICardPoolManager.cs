using System.Collections.Generic;
using MoLibrary.GachaPool.Conventions;

namespace MoLibrary.GachaPool.Interfaces;

/// <summary>
/// Defines the contract for managing multiple card pools.
/// </summary>
public interface ICardPoolManager
{
    /// <summary>
    /// Gets a card pool by its name.
    /// </summary>
    /// <param name="poolName">The name of the card pool to retrieve.</param>
    /// <returns>The card pool instance if found, null otherwise.</returns>
    ICardsPool? GetPool(string poolName);

    /// <summary>
    /// Gets a card drawer for a specific pool by its name.
    /// </summary>
    /// <param name="poolName">The name of the card pool to get a drawer for.</param>
    /// <returns>The card drawer instance if the pool exists, null otherwise.</returns>
    ICardDrawer? GetDrawer(string poolName);

    /// <summary>
    /// Gets a card drawer for a specific pool by its name with generic type support.
    /// </summary>
    /// <typeparam name="T">The type of cards in the pool.</typeparam>
    /// <param name="poolName">The name of the card pool to get a drawer for.</param>
    /// <returns>The generic card drawer instance if the pool exists, null otherwise.</returns>
    ICardDrawer<T>? GetDrawer<T>(string poolName) where T : notnull;

    /// <summary>
    /// Gets all available pool names.
    /// </summary>
    /// <returns>A collection of pool names.</returns>
    IEnumerable<string> GetPoolNames();

    /// <summary>
    /// Adds or updates a card pool with the specified name.
    /// </summary>
    /// <param name="poolName">The name to identify the card pool.</param>
    /// <param name="pool">The card pool instance to add or update.</param>
    void AddOrUpdatePool(string poolName, ICardsPool pool);

    /// <summary>
    /// Removes a card pool by its name.
    /// </summary>
    /// <param name="poolName">The name of the card pool to remove.</param>
    /// <returns>True if the pool was removed, false if it didn't exist.</returns>
    bool RemovePool(string poolName);
} 