using System.Collections.Concurrent;
using Tairitsua.Monica.GachaPool.Abstractions;
using Tairitsua.Monica.GachaPool.Abstractions.Internal;
using Tairitsua.Monica.GachaPool.Models;
using Tairitsua.Monica.GachaPool.Modules;
using Microsoft.Extensions.Options;

namespace Tairitsua.Monica.GachaPool.Services;

internal sealed class GachaPoolCatalog : IGachaPoolCatalog
{
    private readonly ConcurrentDictionary<string, IGachaPoolRuntime> _pools =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly IGachaRandomSource _randomSource;
    private readonly int _recentDrawHistoryLimit;
    private readonly int _maximumBatchSize;

    public GachaPoolCatalog(
        IEnumerable<IGachaPoolRegistration> registrations,
        IGachaRandomSource randomSource,
        IOptions<ModuleGachaPoolOption> options)
    {
        _randomSource = randomSource;
        _recentDrawHistoryLimit = options.Value.RecentDrawHistoryLimit;
        _maximumBatchSize = options.Value.MaximumBatchSize;

        foreach (var registration in registrations)
        {
            if (!_pools.TryAdd(
                    registration.PoolId,
                    registration.CreateRuntime(_recentDrawHistoryLimit, _randomSource)))
            {
                throw new InvalidOperationException(
                    $"Gacha pool '{registration.PoolId}' is registered more than once in the current host.");
            }
        }
    }

    public void AddOrReplace<TPrize>(GachaPoolDefinition<TPrize> definition) where TPrize : notnull
    {
        ArgumentNullException.ThrowIfNull(definition);
        _pools[definition.Id] = new GachaPoolRuntime<TPrize>(
            definition,
            _recentDrawHistoryLimit,
            _randomSource);
    }

    public bool Remove(string poolId)
    {
        return _pools.TryRemove(NormalizePoolId(poolId), out _);
    }

    public IReadOnlyList<GachaPoolSnapshot> GetPools()
    {
        return _pools.Values
            .Select(static pool => pool.Snapshot())
            .OrderBy(static pool => pool.DisplayName, StringComparer.CurrentCultureIgnoreCase)
            .ThenBy(static pool => pool.Id, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public GachaPoolSnapshot GetPool(string poolId)
    {
        return GetRuntime(poolId).Snapshot();
    }

    public GachaDrawResult Draw(string poolId, GachaDrawFilter? filter = null)
    {
        return GetRuntime(poolId).Draw(filter ?? GachaDrawFilter.Any);
    }

    public GachaDrawResult<TPrize> Draw<TPrize>(
        string poolId,
        GachaDrawFilter? filter = null) where TPrize : notnull
    {
        var runtime = GetRuntime(poolId);
        if (runtime is not GachaPoolRuntime<TPrize> typedRuntime)
        {
            throw new InvalidOperationException(
                $"Gacha pool '{runtime.Id}' contains '{runtime.PrizeType.FullName}', not '{typeof(TPrize).FullName}'.");
        }

        return typedRuntime.DrawTyped(filter ?? GachaDrawFilter.Any);
    }

    public IReadOnlyList<GachaDrawResult> DrawMany(
        string poolId,
        int count,
        GachaDrawFilter? filter = null)
    {
        if (count <= 0 || count > _maximumBatchSize)
        {
            throw new ArgumentOutOfRangeException(
                nameof(count),
                count,
                $"Batch draw count must be between 1 and {_maximumBatchSize}.");
        }

        return GetRuntime(poolId).DrawMany(count, filter ?? GachaDrawFilter.Any);
    }

    public void Reset(string poolId)
    {
        GetRuntime(poolId).Reset();
    }

    private IGachaPoolRuntime GetRuntime(string poolId)
    {
        poolId = NormalizePoolId(poolId);
        return _pools.TryGetValue(poolId, out var pool)
            ? pool
            : throw new KeyNotFoundException($"Gacha pool '{poolId}' was not found.");
    }

    private static string NormalizePoolId(string poolId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(poolId);
        return poolId.Trim();
    }
}
