# Euynac.Monica.GachaPool

[![NuGet](https://img.shields.io/nuget/v/Euynac.Monica.GachaPool.svg)](https://www.nuget.org/packages/Euynac.Monica.GachaPool)
[![CI](https://github.com/Tairitsua/MoLibrary.GachaPool/actions/workflows/ci.yml/badge.svg)](https://github.com/Tairitsua/MoLibrary.GachaPool/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/license-MIT-10B981.svg)](LICENSE)

[English](README.md)

![Monica Compatibility Mark](monica-compatibility-mark.png)

**Euynac.Monica.GachaPool** 是对原 MoLibrary.GachaPool 的完整重写，并按 Monica 第三方生态规范重新设计。一个 NuGet 包内包含两个紧密关联的 Monica 模块：

- **Euynac.Monica.GachaPool**：强类型、线程安全的基础设施和 Facade
- **Euynac.Monica.GachaPool.UI**：支持中英文的 MudBlazor 仪表板

This community package is independently maintained and is not affiliated with, endorsed by, or supported by the Monica project.

以上声明表示：本社区包由独立开发者维护，与 Monica 项目不存在隶属、背书或官方支持关系。

## 主要特性

- 奖品值保持强类型，同时提供适合 UI 和 API 的安全快照
- 明确配置各稀有度的绝对概率，并在同稀有度内使用相对权重
- 当稀有度概率总和低于 100% 时，剩余部分作为明确的未中奖概率
- 有限库存原子扣减，库存耗尽后自动重算同稀有度内的概率
- 支持指定稀有度、仅包含或排除指定奖品的条件抽取
- 多卡池、宿主隔离、批量抽取、最近记录及实际概率统计
- 响应式、主题令牌驱动的中英文 Monica UI
- MIT 开源、符号包、Source Link 和 NuGet OIDC 发布流程

## 环境要求

- .NET 10
- Monica 1.0.0-rc.6 或同一预发布线的更高版本

由于依赖的 Monica 仍处于预发布阶段，本包也必须保持预发布版本。

## 安装

    dotnet add package Euynac.Monica.GachaPool --prerelease

## 定义并注册卡池

    using Euynac.Monica.GachaPool.Models;
    using Euynac.Monica.GachaPool.Modules;

    var featuredPool = GachaPoolBuilder
        .Create<Reward>("featured", "精选奖励")
        .WithDescription("包含限量头奖的常驻卡池。")
        .SetRarityProbability(GachaRarity.OneStar, 0.75)
        .SetRarityProbability(GachaRarity.ThreeStar, 0.20)
        .SetRarityProbability(GachaRarity.FiveStar, 0.04)
        .AddPrize("coins", "金币包", new Reward("currency.coins", 100), GachaRarity.OneStar, weight: 3)
        .AddPrize("profile-frame", "极光头像框", new Reward("cosmetic.aurora-frame", 1), GachaRarity.ThreeStar)
        .AddPrize(
            "companion",
            "极光伙伴",
            new Reward("companion.aurora", 1),
            GachaRarity.FiveStar,
            initialStock: 5)
        .Build();

    builder.AddMonica(monica =>
    {
        monica.AddGachaPool()
            .AddPool(featuredPool);

        // 可选：加入 Monica Shell 仪表板，并自动依赖基础设施模块。
        monica.AddGachaPoolUI();
    });

    public sealed record Reward(string Sku, int Quantity);

示例中的稀有度概率合计为 99%，因此非条件抽取会保留 1% 的未中奖概率。同一稀有度内，当前仍有库存的奖品按相对权重分配该档概率。

## 执行强类型抽取

    using Euynac.Monica.GachaPool.Abstractions;

    public sealed class RewardService(IGachaPoolCatalog pools)
    {
        public Reward? DrawFeaturedReward()
        {
            var result = pools.Draw<Reward>("featured");
            return result.TryGetValue(out var reward) ? reward : null;
        }
    }

`TryGetValue` 对引用类型和值类型奖品都安全。也可以在 `HasPrize` 为 `true` 后读取 `Value`；当结果为
`NoPrize` 或 `Exhausted` 时直接读取 `Value` 会抛出异常。

API 或 UI 边界可直接注入 **GachaPoolFacade**；其方法返回 Monica 的 **Res**/**Res&lt;T&gt;**，并只暴露适合展示的快照。

## 启动桥接 Demo

桥接项目内置两个示例卡池，启动后访问 **http://127.0.0.1:5279/euynac-gacha-pool**。

    dotnet run --project samples/Euynac.Monica.GachaPool.Bridge/Euynac.Monica.GachaPool.Bridge.csproj

正常构建直接使用已发布的 Monica 1.0.0-rc.6 包。参与框架开发时，可以显式切换为本地 Monica 源码，
无需修改项目文件：

    dotnet build Euynac.Monica.GachaPool.slnx --configuration Release -p:MonicaSourceRoot=/path/to/Monica

`MonicaSourceRoot` 必须指向包含 `Monica.Core`、`Monica.UI` 和 `Monica.Testing` 的仓库目录；项目不会自动
探测任何同级或固定路径。

## 构建与打包

    dotnet restore Euynac.Monica.GachaPool.slnx
    dotnet build Euynac.Monica.GachaPool.slnx --configuration Release --no-restore
    dotnet test tests/Test.Euynac.Monica.GachaPool/Test.Euynac.Monica.GachaPool.csproj --configuration Release --no-build
    dotnet pack src/Euynac.Monica.GachaPool/Euynac.Monica.GachaPool.csproj --configuration Release --no-build --output artifacts

发布使用不可变 SemVer 标签（例如 `v1.0.0-preview.2`）；手动触发工作流时输入不带 `v` 的同一版本号。
工作流据此生成 `PackageVersion`，使用已发布的 Monica 包，并通过 GitHub OIDC 与受保护的 `nuget`
环境执行 NuGet Trusted Publishing。

## 支持与安全

缺陷和功能建议请提交到 [GitHub Issues](https://github.com/Tairitsua/MoLibrary.GachaPool/issues)。安全问题请按 [SECURITY.md](SECURITY.md) 私下报告。

## 许可证

本项目采用 MIT 许可证，详见 [LICENSE](LICENSE)。
