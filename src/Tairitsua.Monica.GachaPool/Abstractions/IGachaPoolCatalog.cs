using Tairitsua.Monica.GachaPool.Models;

namespace Tairitsua.Monica.GachaPool.Abstractions;

/// <summary>
/// Owns the typed gacha pools registered for one Monica host.
/// </summary>
/// <remarks>
/// The default implementation is singleton and thread-safe. Pool identifiers are compared with ordinal
/// case-insensitive semantics. Replacing or removing pools affects only the current host.
/// </remarks>
public interface IGachaPoolCatalog
{
    /// <summary>
    /// Adds a pool or atomically replaces the existing pool with the same identifier.
    /// </summary>
    /// <typeparam name="TPrize">The publisher-owned prize value type.</typeparam>
    /// <param name="definition">The immutable pool definition.</param>
    void AddOrReplace<TPrize>(GachaPoolDefinition<TPrize> definition) where TPrize : notnull;

    /// <summary>
    /// Removes a pool.
    /// </summary>
    /// <param name="poolId">The pool identifier.</param>
    /// <returns><see langword="true"/> when a pool was removed; otherwise <see langword="false"/>.</returns>
    bool Remove(string poolId);

    /// <summary>
    /// Gets consistent snapshots of every registered pool.
    /// </summary>
    /// <returns>Pool snapshots ordered by display name and identifier.</returns>
    IReadOnlyList<GachaPoolSnapshot> GetPools();

    /// <summary>
    /// Gets a consistent snapshot of one pool.
    /// </summary>
    /// <param name="poolId">The pool identifier.</param>
    /// <returns>The current pool snapshot.</returns>
    /// <exception cref="KeyNotFoundException">No pool has the supplied identifier.</exception>
    GachaPoolSnapshot GetPool(string poolId);

    /// <summary>
    /// Draws from a pool and returns presentation-safe prize identity.
    /// </summary>
    /// <param name="poolId">The pool identifier.</param>
    /// <param name="filter">Optional draw restriction; unrestricted draws preserve configured no-prize probability.</param>
    /// <returns>The completed draw outcome.</returns>
    /// <exception cref="KeyNotFoundException">No pool has the supplied identifier.</exception>
    GachaDrawResult Draw(string poolId, GachaDrawFilter? filter = null);

    /// <summary>
    /// Draws from a pool and returns the publisher-owned typed value.
    /// </summary>
    /// <typeparam name="TPrize">The expected prize value type.</typeparam>
    /// <param name="poolId">The pool identifier.</param>
    /// <param name="filter">Optional draw restriction; unrestricted draws preserve configured no-prize probability.</param>
    /// <returns>The completed typed draw outcome.</returns>
    /// <exception cref="KeyNotFoundException">No pool has the supplied identifier.</exception>
    /// <exception cref="InvalidOperationException">The registered pool uses a different prize value type.</exception>
    GachaDrawResult<TPrize> Draw<TPrize>(string poolId, GachaDrawFilter? filter = null) where TPrize : notnull;

    /// <summary>
    /// Performs a bounded batch of presentation-safe draws.
    /// </summary>
    /// <param name="poolId">The pool identifier.</param>
    /// <param name="count">Number of draws, from one through the configured batch limit.</param>
    /// <param name="filter">Optional draw restriction applied to every draw.</param>
    /// <returns>Draw outcomes in completion order.</returns>
    /// <remarks>
    /// A batch executes under one pool lock: other draws, snapshots, and resets wait until the complete batch finishes.
    /// </remarks>
    /// <exception cref="KeyNotFoundException">No pool has the supplied identifier.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The count is outside the configured batch range.</exception>
    IReadOnlyList<GachaDrawResult> DrawMany(string poolId, int count, GachaDrawFilter? filter = null);

    /// <summary>
    /// Restores limited inventory and clears draw statistics for one pool.
    /// </summary>
    /// <param name="poolId">The pool identifier.</param>
    /// <exception cref="KeyNotFoundException">No pool has the supplied identifier.</exception>
    void Reset(string poolId);
}
