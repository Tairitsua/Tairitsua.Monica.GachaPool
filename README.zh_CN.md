# MoLibrary.GachaPool

[![NuGet](https://img.shields.io/nuget/v/MoLibrary.GachaPool.svg)](https://www.nuget.org/packages/MoLibrary.GachaPool)
[![NuGet Downloads](https://img.shields.io/nuget/dt/MoLibrary.GachaPool.svg)](https://www.nuget.org/packages/MoLibrary.GachaPool)

MoLibrary.GachaPool 是一个灵活高效的 .NET 抽卡（扭蛋）系统管理库，提供基于概率的抽取机制。它为游戏或任何需要概率选择物品的应用程序提供了强大的基础设施。

## 语言

[English](README.md) | 简体中文

## 特性

- 🎯 基于概率的抽卡系统
- 🔄 支持多个扭蛋池管理
- 🎲 可自定义稀有度和概率设置
- 📊 内置抽取统计跟踪
- 🧩 支持自定义物品类型的泛型实现
- 🔌 易于集成的依赖注入支持
- 🔒 线程安全操作
- 🚀 高性能实现

## 安装

通过 NuGet 安装包：

```bash
dotnet add package MoLibrary.GachaPool
```

## 快速开始

1. 首先，创建你的扭蛋池加载器，继承 `CardsPoolByMemoryProvider`：

```csharp
public class MyGameGachaPoolLoader : CardsPoolByMemoryProvider
{
    public override void ConfigurePools()
    {
        // 配置一个使用整数作为物品的标准池
        ConfigurePool("standardPool", pool =>
        {
            var standardItems = Card<int>.CreateMultiCards(CardRarity.OneStar, 1, 2, 3, 4, 5);
            pool.AddCards(standardItems);
            pool.BuildPool();
        });

        // 配置带有自定义概率设置的池
        ConfigurePool("customPool", pool =>
        {
            pool.SetPoolRarityProbability(CardRarity.OneStar, 0.7)
                .SetPoolRarityProbability(CardRarity.TwoStar, 0.3);
            // 添加你的物品...
            pool.BuildPool();
        });
    }
}
```

2. 在应用程序中注册服务：

```csharp
services.AddMemoryCardPool<MyGameGachaPoolLoader>();
```

3. 在代码中使用扭蛋池管理器：

```csharp
public class GameService
{
    private readonly ICardPoolManager _poolManager;

    public GameService(ICardPoolManager poolManager)
    {
        _poolManager = poolManager;
    }

    public Card DrawItem(string poolName)
    {
        var drawer = _poolManager.GetDrawer(poolName);
        return drawer?.DrawCard();
    }

    public string GetDrawStatistics(string poolName)
    {
        var drawer = _poolManager.GetDrawer(poolName);
        return drawer?.Statistician.GetReport().GetTableString();
    }
}
```

## 高级用法

### 自定义物品类型

你可以通过继承 `Card<T>` 来创建自定义物品类型：

```csharp
public class CharacterItem : Card<CharacterItem>
{
    public string Name { get; set; }
    public int Level { get; set; }

    public CharacterItem(string name, int level, CardRarity rarity) : base(rarity)
    {
        Name = name;
        Level = level;
    }
}
```

然后使用泛型抽取器：

```csharp
var drawer = _poolManager.GetDrawer<CharacterItem>("characterPool");
var character = drawer?.DrawCard();
```

### 概率配置

你可以为不同稀有度的物品配置概率：

```csharp
ConfigurePool("myPool", pool =>
{
    pool.SetPoolRarityProbability(CardRarity.OneStar, 0.6)
        .SetPoolRarityProbability(CardRarity.TwoStar, 0.3)
        .SetPoolRarityProbability(CardRarity.ThreeStar, 0.1);
    // 添加物品...
    pool.BuildPool();
});
```

### 抽取统计

库会自动跟踪抽取统计信息：

```csharp
var drawer = _poolManager.GetDrawer("myPool");
var stats = drawer?.Statistician.GetReport().GetTableString();
Console.WriteLine(stats);
```

## 工作原理

1. **扭蛋池构建**：
   - 每个物品都有一个相对于整个池的真实概率
   - 可以设置物品的稀有度或单独设置概率
   - 系统根据设置自动生成真实概率
   - 生成区间布局作为后续抽取的基础

2. **抽取机制**：
   - 使用二分查找快速定位概率区间
   - 采用线程安全的随机数生成
   - 支持条件抽取（指定稀有度、包含/排除特定物品）

## 性能考虑

- 使用线程安全的集合实现并发访问
- 扭蛋池构建后会被缓存以供后续抽取使用
- 抽取操作使用二分查找优化
- 针对大型池优化了内存使用

## 贡献

欢迎提交 Pull Request 来帮助改进这个项目！

## 许可证

本项目采用 MIT 许可证 - 查看 [LICENSE](LICENSE) 文件了解详情。
