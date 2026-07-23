using Tairitsua.Monica.GachaPool.Abstractions;
using Tairitsua.Monica.GachaPool.Facades;
using Tairitsua.Monica.GachaPool.Models;
using Tairitsua.Monica.GachaPool.Modules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Monica.Core.Modularity.Abstractions;
using Monica.Modules;
using Monica.Testing.Hosting;

namespace Test.Tairitsua.Monica.GachaPool.Support;

internal sealed class GachaPoolTestApplicationFactory(
    IGachaRandomSource? randomSource = null,
    bool registerDuplicatePool = false,
    bool includeUi = false,
    bool disableDashboardPage = false,
    bool includeShell = false)
    : MonicaTestApplicationFactory<GachaPoolFacade>
{
    protected override void ConfigureMonica(IMonicaBuilder builder)
    {
        var limitedPool = CreateLimitedPool();
        var guide = builder.AddGachaPool(options =>
            {
                options.RecentDrawHistoryLimit = 8;
                options.MaximumBatchSize = 20;
            })
            .AddPool(limitedPool)
            .AddPool(CreateNoPrizePool())
            .AddPool(CreateNumericNoPrizePool());

        if (registerDuplicatePool)
        {
            guide.AddPool(limitedPool);
        }

        if (includeShell)
        {
            builder.AddUIShell();
        }

        if (includeUi)
        {
            builder.AddGachaPoolUI(options =>
                options.DisableDashboardPage = disableDashboardPage);
        }
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);
        services.Replace(ServiceDescriptor.Singleton<IGachaRandomSource>(
            randomSource ?? new SequenceGachaRandomSource(0, 0, 0.95, 0.25, 0.75)));
    }

    private static GachaPoolDefinition<TestReward> CreateLimitedPool()
    {
        return GachaPoolBuilder
            .Create<TestReward>("limited", "Limited")
            .SetRarityProbability(GachaRarity.FiveStar, 1)
            .AddPrize(
                "limited-prize",
                "Limited Prize",
                new TestReward("limited"),
                GachaRarity.FiveStar,
                initialStock: 1)
            .AddPrize(
                "fallback-prize",
                "Fallback Prize",
                new TestReward("fallback"),
                GachaRarity.FiveStar)
            .Build();
    }

    private static GachaPoolDefinition<TestReward> CreateNoPrizePool()
    {
        return GachaPoolBuilder
            .Create<TestReward>("no-prize", "No Prize")
            .SetRarityProbability(GachaRarity.OneStar, 0.8)
            .AddPrize("common", "Common", new TestReward("common"), GachaRarity.OneStar)
            .Build();
    }

    private static GachaPoolDefinition<int> CreateNumericNoPrizePool()
    {
        return GachaPoolBuilder
            .Create<int>("numeric-no-prize", "Numeric No Prize")
            .SetRarityProbability(GachaRarity.OneStar, 0.5)
            .AddPrize("number", "Number", 42, GachaRarity.OneStar)
            .Build();
    }
}

internal sealed record TestReward(string Id);
