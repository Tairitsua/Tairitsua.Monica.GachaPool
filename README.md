# Tairitsua.Monica.GachaPool

[![NuGet](https://img.shields.io/nuget/v/Tairitsua.Monica.GachaPool.svg)](https://www.nuget.org/packages/Tairitsua.Monica.GachaPool)
[![CI](https://github.com/Tairitsua/MoLibrary.GachaPool/actions/workflows/ci.yml/badge.svg)](https://github.com/Tairitsua/MoLibrary.GachaPool/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/license-MIT-10B981.svg)](LICENSE)

[简体中文](README.zh_CN.md)

![Monica Compatibility Mark](monica-compatibility-mark.png)

**Tairitsua.Monica.GachaPool** is a complete rewrite of the original MoLibrary.GachaPool as a Monica ecosystem package. One NuGet package contains two coherent Monica modules:

- **Tairitsua.Monica.GachaPool** — typed, thread-safe infrastructure and Facade
- **Tairitsua.Monica.GachaPool.UI** — localized MudBlazor dashboard

This community package is independently maintained and is not affiliated with, endorsed by, or supported by the Monica project.

## Highlights

- Strongly typed prize values without sacrificing UI-safe snapshots
- Explicit rarity probabilities and weighted prizes within each rarity
- Intentional no-prize probability when configured rarity totals are below 100%
- Finite inventory with atomic depletion and probability redistribution
- Include, exclude, and rarity-constrained draws
- Multiple host-owned pools, bounded batch draws, recent history, and observed statistics
- English and Simplified Chinese Monica UI with responsive theme-token styling
- MIT-licensed source, symbols, Source Link, and NuGet Trusted Publishing workflow

## Requirements

- .NET 10
- Monica 1.0.0-rc.6 or later in the same prerelease line

Because the Monica dependency is prerelease, this package also remains prerelease.

## Install

    dotnet add package Tairitsua.Monica.GachaPool --prerelease

## Define and register a pool

    using Tairitsua.Monica.GachaPool.Models;
    using Tairitsua.Monica.GachaPool.Modules;

    var featuredPool = GachaPoolBuilder
        .Create<Reward>("featured", "Featured Rewards")
        .WithDescription("A permanent pool with a limited headline reward.")
        .SetRarityProbability(GachaRarity.OneStar, 0.75)
        .SetRarityProbability(GachaRarity.ThreeStar, 0.20)
        .SetRarityProbability(GachaRarity.FiveStar, 0.04)
        .AddPrize("coins", "Coin Bundle", new Reward("currency.coins", 100), GachaRarity.OneStar, weight: 3)
        .AddPrize("profile-frame", "Aurora Frame", new Reward("cosmetic.aurora-frame", 1), GachaRarity.ThreeStar)
        .AddPrize(
            "companion",
            "Aurora Companion",
            new Reward("companion.aurora", 1),
            GachaRarity.FiveStar,
            initialStock: 5)
        .Build();

    builder.AddMonica(monica =>
    {
        monica.AddGachaPool()
            .AddPool(featuredPool);

        // Optional: adds the Monica Shell dashboard and transitively registers infrastructure.
        monica.AddGachaPoolUI();
    });

    public sealed record Reward(string Sku, int Quantity);

The configured rarity probabilities total 99%, so unrestricted draws retain a 1% no-prize outcome. Within each rarity, currently available prizes share that tier according to their relative weights.

## Draw typed values

    using Tairitsua.Monica.GachaPool.Abstractions;

    public sealed class RewardService(IGachaPoolCatalog pools)
    {
        public Reward? DrawFeaturedReward()
        {
            var result = pools.Draw<Reward>("featured");
            return result.TryGetValue(out var reward) ? reward : null;
        }
    }

`TryGetValue` is safe for both reference-type and value-type prizes. The `Value` property is also available after
`HasPrize` is true and throws when the outcome is `NoPrize` or `Exhausted`.

For API or UI boundaries, inject **GachaPoolFacade**; its methods return Monica **Res**/**Res&lt;T&gt;** envelopes and presentation-safe snapshots.

## Run the bridge

The bridge includes two sample pools and starts the localized dashboard at `http://127.0.0.1:5279/tairitsua-gacha-pool`.

    dotnet run --project samples/Tairitsua.Monica.GachaPool.Bridge/Tairitsua.Monica.GachaPool.Bridge.csproj

Normal builds consume the released Monica 1.0.0-rc.6 packages. Framework contributors can explicitly opt into a
local Monica source checkout without changing project files:

    dotnet build Tairitsua.Monica.GachaPool.slnx --configuration Release -p:MonicaSourceRoot=/path/to/Monica

`MonicaSourceRoot` must point to the repository directory containing `Monica.Core`, `Monica.UI`, and
`Monica.Testing`. No source checkout is discovered implicitly.

## Build and pack

    dotnet restore Tairitsua.Monica.GachaPool.slnx
    dotnet build Tairitsua.Monica.GachaPool.slnx --configuration Release --no-restore
    dotnet test tests/Test.Tairitsua.Monica.GachaPool/Test.Tairitsua.Monica.GachaPool.csproj --configuration Release --no-build
    dotnet pack src/Tairitsua.Monica.GachaPool/Tairitsua.Monica.GachaPool.csproj --configuration Release --no-build --output artifacts

Releases are immutable SemVer tags such as `v1.0.0-preview.2`. A manual workflow run requires the same version
without the leading `v`. The publish workflow derives `PackageVersion` from that immutable input, consumes released
Monica packages, and uses NuGet Trusted Publishing through GitHub OIDC and a protected **nuget** environment.

## Support and security

Use [GitHub Issues](https://github.com/Tairitsua/MoLibrary.GachaPool/issues) for defects and feature requests. Report vulnerabilities according to [SECURITY.md](SECURITY.md).

## License

MIT. See [LICENSE](LICENSE).
