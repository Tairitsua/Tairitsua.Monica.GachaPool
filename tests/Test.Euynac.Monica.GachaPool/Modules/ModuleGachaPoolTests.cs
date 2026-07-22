using System.Reflection;
using System.Text.Json;
using AwesomeAssertions;
using Euynac.Monica.GachaPool.Abstractions;
using Euynac.Monica.GachaPool.Facades;
using Euynac.Monica.GachaPool.Models;
using Euynac.Monica.GachaPool.Modules;
using Euynac.Monica.GachaPool.Pages;
using Test.Euynac.Monica.GachaPool.Support;
using Microsoft.AspNetCore.Components;
using Monica.Core.Modularity.Annotations;
using Monica.Core.Results;

namespace Test.Euynac.Monica.GachaPool.Modules;

public sealed class ModuleGachaPoolTests
{
    [Fact]
    public async Task HostComposesBothPackageModulesWithCanonicalKeys()
    {
        var factory = new GachaPoolTestApplicationFactory(includeUi: true);
        var cancellationToken = TestContext.Current.CancellationToken;

        await using var application = await factory.CreateAsync(cancellationToken: cancellationToken);
        await using var scope = application.CreateScope(cancellationToken);

        var keys = application.ModuleSnapshots
            .Where(snapshot => snapshot.ModuleType == typeof(ModuleGachaPool)
                               || snapshot.ModuleType == typeof(ModuleGachaPoolUI))
            .Select(snapshot => snapshot.ModuleKey.Value)
            .ToArray();

        keys.Should().BeEquivalentTo(
            "Euynac.Monica.GachaPool",
            "Euynac.Monica.GachaPool.UI");
        scope.Resolve<IGachaPoolCatalog>().GetPools().Should().HaveCount(3);
        scope.Resolve<GachaPoolFacade>().GetPools().Status.Should().Be(ResStatus.Ok);
    }

    [Fact]
    public void ModuleTypesDeclareCanonicalPackageKeys()
    {
        GetDeclaredModuleKey<ModuleGachaPool>().Should().Be("Euynac.Monica.GachaPool");
        GetDeclaredModuleKey<ModuleGachaPoolUI>().Should().Be("Euynac.Monica.GachaPool.UI");
    }

    [Fact]
    public void DashboardPageUsesCollisionResistantPublicRoute()
    {
        var routes = typeof(UIGachaPoolPage)
            .GetCustomAttributes<RouteAttribute>()
            .Select(static attribute => attribute.Template);

        routes.Should().ContainSingle().Which.Should().Be(UIGachaPoolPage.PAGE_URL);
        UIGachaPoolPage.PAGE_URL.Should().Be("/euynac-gacha-pool");
    }

    [Fact]
    public async Task DrawRedistributesWithinRarityWhenLimitedPrizeIsExhausted()
    {
        var factory = new GachaPoolTestApplicationFactory();
        var cancellationToken = TestContext.Current.CancellationToken;

        await using var application = await factory.CreateAsync(cancellationToken: cancellationToken);
        await using var scope = application.CreateScope(cancellationToken);
        var catalog = scope.Resolve<IGachaPoolCatalog>();

        var first = catalog.Draw<TestReward>("limited");
        var second = catalog.Draw<TestReward>("limited");
        var snapshot = catalog.GetPool("limited");

        first.Value.Should().Be(new TestReward("limited"));
        first.RemainingStock.Should().Be(0);
        second.Value.Should().Be(new TestReward("fallback"));
        snapshot.Entries.Single(entry => entry.Prize.Id == "limited-prize").Probability.Should().Be(0);
        snapshot.Entries.Single(entry => entry.Prize.Id == "fallback-prize").Probability.Should().Be(1);
    }

    [Fact]
    public async Task DrawReturnsNoPrizeWhenRollFallsOutsideConfiguredRarities()
    {
        var factory = new GachaPoolTestApplicationFactory();
        var cancellationToken = TestContext.Current.CancellationToken;

        await using var application = await factory.CreateAsync(cancellationToken: cancellationToken);
        await using var scope = application.CreateScope(cancellationToken);
        var catalog = scope.Resolve<IGachaPoolCatalog>();

        catalog.Draw("limited");
        catalog.Draw("limited");
        var result = catalog.Draw("no-prize");

        result.Outcome.Should().Be(GachaDrawOutcomeKind.NoPrize);
        result.HasPrize.Should().BeFalse();
        result.ProbabilityAtDraw.Should().BeApproximately(0.2, 1e-12);
    }

    [Fact]
    public async Task RestrictedDrawsHonorIncludeExcludeAndRarityFilters()
    {
        var factory = new GachaPoolTestApplicationFactory(new SequenceGachaRandomSource(0.75, 0.25, 0.5));
        var cancellationToken = TestContext.Current.CancellationToken;

        await using var application = await factory.CreateAsync(cancellationToken: cancellationToken);
        await using var scope = application.CreateScope(cancellationToken);
        var catalog = scope.Resolve<IGachaPoolCatalog>();

        var included = catalog.Draw<TestReward>("limited", GachaDrawFilter.IncludeOnly("fallback-prize"));
        var excluded = catalog.Draw<TestReward>("limited", GachaDrawFilter.Excluding("fallback-prize"));
        var rarity = catalog.Draw<TestReward>("no-prize", GachaDrawFilter.ForRarity(GachaRarity.OneStar));

        included.Value.Should().Be(new TestReward("fallback"));
        excluded.Value.Should().Be(new TestReward("limited"));
        rarity.Value.Should().Be(new TestReward("common"));
    }

    [Fact]
    public async Task PresentationDrawsAndHistoryDoNotExposeTypedPrizeValues()
    {
        var factory = new GachaPoolTestApplicationFactory();
        var cancellationToken = TestContext.Current.CancellationToken;

        await using var application = await factory.CreateAsync(cancellationToken: cancellationToken);
        await using var scope = application.CreateScope(cancellationToken);
        var catalog = scope.Resolve<IGachaPoolCatalog>();

        var result = catalog.Draw("limited");
        var historyResult = catalog.GetPool("limited").RecentDraws.Should().ContainSingle().Which;

        result.Should().BeOfType<GachaDrawResult>();
        historyResult.Should().BeOfType<GachaDrawResult>();
    }

    [Fact]
    public async Task SnapshotSerializesWithJsonSafePrizeTypeName()
    {
        var factory = new GachaPoolTestApplicationFactory();
        var cancellationToken = TestContext.Current.CancellationToken;

        await using var application = await factory.CreateAsync(cancellationToken: cancellationToken);
        await using var scope = application.CreateScope(cancellationToken);
        var snapshot = scope.Resolve<IGachaPoolCatalog>().GetPool("limited");

        using var document = JsonDocument.Parse(JsonSerializer.Serialize(snapshot));

        document.RootElement
            .GetProperty(nameof(GachaPoolSnapshot.PrizeTypeName))
            .GetString()
            .Should()
            .Be(typeof(TestReward).FullName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(21)]
    public async Task FacadeRejectsBatchSizesOutsideConfiguredBounds(int count)
    {
        var factory = new GachaPoolTestApplicationFactory();
        var cancellationToken = TestContext.Current.CancellationToken;

        await using var application = await factory.CreateAsync(cancellationToken: cancellationToken);
        await using var scope = application.CreateScope(cancellationToken);

        var response = scope.Resolve<GachaPoolFacade>().DrawMany("limited", count);

        response.Status.Should().Be(ResStatus.BadRequest);
        response.Data.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task FacadeTreatsBlankPoolIdentifiersAsBadRequests(string poolId)
    {
        var factory = new GachaPoolTestApplicationFactory();
        var cancellationToken = TestContext.Current.CancellationToken;

        await using var application = await factory.CreateAsync(cancellationToken: cancellationToken);
        await using var scope = application.CreateScope(cancellationToken);
        var facade = scope.Resolve<GachaPoolFacade>();

        var getResponse = facade.GetPool(poolId);
        var drawResponse = facade.Draw(poolId);
        var batchResponse = facade.DrawMany(poolId, 1);
        var resetResponse = facade.Reset(poolId);

        getResponse.Status.Should().Be(ResStatus.BadRequest);
        drawResponse.Status.Should().Be(ResStatus.BadRequest);
        batchResponse.Status.Should().Be(ResStatus.BadRequest);
        resetResponse.Status.Should().Be(ResStatus.BadRequest);
        getResponse.Message.Should().NotBeNullOrWhiteSpace();
        getResponse.Message.Should().NotBe("ServiceMessages:PoolIdInvalid");
        drawResponse.Message.Should().Be(getResponse.Message);
        batchResponse.Message.Should().Be(getResponse.Message);
        resetResponse.Message.Should().Be(getResponse.Message);
    }

    [Fact]
    public async Task ConcurrentDrawsDepleteFiniteInventoryExactlyOnce()
    {
        var factory = new GachaPoolTestApplicationFactory();
        var cancellationToken = TestContext.Current.CancellationToken;

        await using var application = await factory.CreateAsync(cancellationToken: cancellationToken);
        await using var scope = application.CreateScope(cancellationToken);
        var catalog = scope.Resolve<IGachaPoolCatalog>();
        var pool = GachaPoolBuilder
            .Create<TestReward>("concurrent", "Concurrent")
            .SetRarityProbability(GachaRarity.FiveStar, 1)
            .AddPrize(
                "single",
                "Single",
                new TestReward("single"),
                GachaRarity.FiveStar,
                initialStock: 1)
            .Build();
        catalog.AddOrReplace(pool);

        var draws = await Task.WhenAll(Enumerable.Range(0, 64).Select(_ =>
            Task.Run(() => catalog.Draw<TestReward>(pool.Id), cancellationToken)));
        var snapshot = catalog.GetPool(pool.Id);

        draws.Count(static draw => draw.HasPrize).Should().Be(1);
        draws.Count(static draw => draw.Outcome == GachaDrawOutcomeKind.Exhausted).Should().Be(63);
        draws.Select(static draw => draw.Sequence).Should().OnlyHaveUniqueItems();
        snapshot.TotalDraws.Should().Be(64);
        snapshot.NoPrizeDraws.Should().Be(63);
        snapshot.Entries.Should().ContainSingle().Which.RemainingStock.Should().Be(0);
    }

    [Fact]
    public async Task DuplicatePoolRegistrationFailsWhenCatalogIsMaterialized()
    {
        var factory = new GachaPoolTestApplicationFactory(registerDuplicatePool: true);
        var cancellationToken = TestContext.Current.CancellationToken;

        Func<Task> action = async () =>
        {
            await using var application = await factory.CreateAsync(cancellationToken: cancellationToken);
            await using var scope = application.CreateScope(cancellationToken);
            scope.Resolve<IGachaPoolCatalog>();
        };

        await action.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("*registered more than once*");
    }

    [Fact]
    public async Task FacadeDoesNotExposeInternalExceptionDetails()
    {
        var factory = new GachaPoolTestApplicationFactory(new ThrowingGachaRandomSource());
        var cancellationToken = TestContext.Current.CancellationToken;

        await using var application = await factory.CreateAsync(cancellationToken: cancellationToken);
        await using var scope = application.CreateScope(cancellationToken);

        var response = scope.Resolve<GachaPoolFacade>().Draw("limited");

        response.Status.Should().Be(ResStatus.InternalError);
        response.Message.Should().NotContain(ThrowingGachaRandomSource.DiagnosticToken);
        response.Data.Should().BeNull();
    }

    [Fact]
    public async Task TypedNoPrizeResultSafelyRepresentsValueTypeAbsence()
    {
        var factory = new GachaPoolTestApplicationFactory(new SequenceGachaRandomSource(0.95));
        var cancellationToken = TestContext.Current.CancellationToken;

        await using var application = await factory.CreateAsync(cancellationToken: cancellationToken);
        await using var scope = application.CreateScope(cancellationToken);

        var result = scope.Resolve<IGachaPoolCatalog>().Draw<int>("numeric-no-prize");
        var hasValue = result.TryGetValue(out var value);
        Func<int> readValue = () => result.Value;

        result.Outcome.Should().Be(GachaDrawOutcomeKind.NoPrize);
        hasValue.Should().BeFalse();
        value.Should().Be(default);
        readValue.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public async Task CreateAsyncKeepsPoolStateHostOwnedAcrossApplications()
    {
        var factory = new GachaPoolTestApplicationFactory();
        var cancellationToken = TestContext.Current.CancellationToken;

        await using var firstApplication = await factory.CreateAsync(cancellationToken: cancellationToken);
        await using var firstScope = firstApplication.CreateScope(cancellationToken);
        firstScope.Resolve<IGachaPoolCatalog>().Draw("limited");

        await using var secondApplication = await factory.CreateAsync(cancellationToken: cancellationToken);
        await using var secondScope = secondApplication.CreateScope(cancellationToken);

        firstScope.Resolve<IGachaPoolCatalog>().GetPool("limited").TotalDraws.Should().Be(1);
        secondScope.Resolve<IGachaPoolCatalog>().GetPool("limited").TotalDraws.Should().Be(0);
    }

    private static string GetDeclaredModuleKey<TModule>()
    {
        return typeof(TModule).GetCustomAttribute<ModuleKeyAttribute>()?.Key.Value
               ?? throw new InvalidOperationException($"{typeof(TModule).Name} does not declare a module key.");
    }
}
