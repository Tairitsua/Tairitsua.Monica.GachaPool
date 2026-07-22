using Euynac.Monica.GachaPool.Facades;
using Euynac.Monica.GachaPool.Localization;
using Euynac.Monica.GachaPool.Models;
using Euynac.Monica.GachaPool.Modules;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Monica.Core.Results;

namespace Euynac.Monica.GachaPool.UIGachaPool.State;

internal sealed class GachaPoolDashboardState(
    GachaPoolFacade facade,
    IStringLocalizer<GachaPoolResource> localizer,
    IOptions<ModuleGachaPoolOption> options)
{
    internal IReadOnlyList<GachaPoolSnapshot> Pools { get; private set; } = [];

    internal GachaPoolSnapshot? SelectedPool { get; private set; }

    internal string? SelectedPoolId => SelectedPool?.Id;

    internal IReadOnlyList<GachaDrawResult> LatestDraws { get; private set; } = [];

    internal int BatchSize { get; set; } = Math.Min(10, options.Value.MaximumBatchSize);

    internal int MaximumBatchSize { get; } = options.Value.MaximumBatchSize;

    internal Res Initialize()
    {
        var response = facade.GetPools();
        if (response.IsFailed(out var error, out var pools))
        {
            return error;
        }

        Pools = pools;
        if (Pools.Count == 0)
        {
            SelectedPool = null;
            LatestDraws = [];
            return Res.Ok();
        }

        var selectedId = SelectedPoolId;
        var selected = selectedId is null
            ? Pools[0]
            : Pools.FirstOrDefault(pool =>
                string.Equals(pool.Id, selectedId, StringComparison.OrdinalIgnoreCase)) ?? Pools[0];
        SelectedPool = selected;
        return Res.Ok();
    }

    internal Res SelectPool(string poolId)
    {
        var response = facade.GetPool(poolId);
        if (response.IsFailed(out var error, out var pool))
        {
            return error;
        }

        SelectedPool = pool;
        LatestDraws = [];
        return Res.Ok();
    }

    internal Res<GachaDrawResult> DrawOne()
    {
        if (SelectedPool is null)
        {
            return Res.Fail(localizer["Messages:NoPoolSelected"]);
        }

        var response = facade.Draw(SelectedPool.Id);
        if (response.IsFailed(out var error, out var draw))
        {
            return error;
        }

        LatestDraws = [draw];
        RefreshSelectedPool();
        return Res.Ok(draw);
    }

    internal Res<IReadOnlyList<GachaDrawResult>> DrawBatch()
    {
        if (SelectedPool is null)
        {
            return Res.Fail(localizer["Messages:NoPoolSelected"]);
        }

        var response = facade.DrawMany(SelectedPool.Id, BatchSize);
        if (response.IsFailed(out var error, out var draws))
        {
            return error;
        }

        LatestDraws = draws.Reverse().ToArray();
        RefreshSelectedPool();
        return Res.Ok(draws);
    }

    internal Res Reset()
    {
        if (SelectedPool is null)
        {
            return Res.Fail(localizer["Messages:NoPoolSelected"]);
        }

        var response = facade.Reset(SelectedPool.Id);
        if (response.IsFailed(out var error))
        {
            return error;
        }

        LatestDraws = [];
        RefreshSelectedPool();
        return response;
    }

    private void RefreshSelectedPool()
    {
        var selectedId = SelectedPool?.Id;
        if (selectedId is null)
        {
            return;
        }

        var selectedResponse = facade.GetPool(selectedId);
        if (!selectedResponse.IsFailed(out _, out var selected))
        {
            SelectedPool = selected;
        }

        var poolsResponse = facade.GetPools();
        if (!poolsResponse.IsFailed(out _, out var pools))
        {
            Pools = pools;
        }
    }
}
