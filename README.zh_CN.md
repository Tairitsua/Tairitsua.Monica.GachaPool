# CardPool

CardPool 是一个灵活高效的 .NET 卡池管理库，提供基于概率的抽卡机制。它为游戏或任何需要概率选择物品的应用程序提供了强大的基础设施。

## 特性

- 🎯 基于概率的抽卡系统
- 🔄 支持多个卡池管理
- 🎲 可自定义稀有度和概率设置
- 📊 内置抽卡统计跟踪
- 🧩 支持自定义卡牌类型的泛型实现
- 🔌 易于集成的依赖注入支持
- 🔒 线程安全操作
- 🚀 高性能实现

## 安装

通过 NuGet 安装包：

```bash
dotnet add package CardPool
```

## 快速开始

1. 首先，创建你的卡池加载器，继承 `CardsPoolByMemoryProvider`：

```csharp
public class MyGameCardPoolLoader : CardsPoolByMemoryProvider
{
    public override void ConfigurePools()
    {
        // 配置一个使用整数作为卡牌的标准卡池
        ConfigurePool("standardPool", pool =>
        {
            var standardCards = Card<int>.CreateMultiCards(CardRarity.OneStar, 1, 2, 3, 4, 5);
            pool.AddCards(standardCards);
            pool.BuildPool();
        });

        // 配置带有自定义概率设置的卡池
        ConfigurePool("customPool", pool =>
        {
            pool.SetPoolRarityProbability(CardRarity.OneStar, 0.7)
                .SetPoolRarityProbability(CardRarity.TwoStar, 0.3);
            // 添加你的卡牌...
            pool.BuildPool();
        });
    }
}
```

2. 在应用程序中注册服务：

```csharp
services.AddMemoryCardPool<MyGameCardPoolLoader>();
```

3. 在代码中使用卡池管理器：

```csharp
public class GameService
{
    private readonly ICardPoolManager _poolManager;

    public GameService(ICardPoolManager poolManager)
    {
        _poolManager = poolManager;
    }

    public Card DrawCard(string poolName)
    {
        var drawer = _poolManager.GetDrawer(poolName);
        return drawer?.DrawCard();
    }

    public string GetDrawStatistics(string poolName)
    {
        var drawer = _poolManager.GetDrawer(poolName);
        return drawer?.Statistician.GetReportTableString();
    }
}
```

## 高级用法

### 自定义卡牌类型

你可以通过继承 `Card<T>` 来创建自定义卡牌类型：

```csharp
public class CharacterCard : Card<CharacterCard>
{
    public string Name { get; set; }
    public int Level { get; set; }

    public CharacterCard(string name, int level, CardRarity rarity) : base(rarity)
    {
        Name = name;
        Level = level;
    }
}
```

然后使用泛型抽卡器：

```csharp
var drawer = _poolManager.GetDrawer<CharacterCard>("characterPool");
var character = drawer?.DrawCard();
```

### 概率配置

你可以为不同稀有度的卡牌配置概率：

```csharp
ConfigurePool("myPool", pool =>
{
    pool.SetPoolRarityProbability(CardRarity.OneStar, 0.6)
        .SetPoolRarityProbability(CardRarity.TwoStar, 0.3)
        .SetPoolRarityProbability(CardRarity.ThreeStar, 0.1);
    // 添加卡牌...
    pool.BuildPool();
});
```

### 抽卡统计

库会自动跟踪抽卡统计信息：

```csharp
var drawer = _poolManager.GetDrawer("myPool");
var stats = drawer?.Statistician.GetReportTableString();
Console.WriteLine(stats);
```

## 工作原理

1. **卡池构建**：
   - 每张卡牌都有一个相对于整个卡池的真实概率
   - 可以设置卡牌的稀有度或单独设置概率
   - 系统根据设置自动生成真实概率
   - 生成区间布局作为后续抽卡的基础

2. **抽卡机制**：
   - 使用二分查找快速定位概率区间
   - 采用线程安全的随机数生成
   - 支持条件抽卡（指定稀有度、包含/排除特定卡牌）

## 性能考虑

- 使用线程安全的集合实现并发访问
- 卡池构建后会被缓存以供后续抽卡使用
- 抽卡操作使用二分查找优化
- 针对大型卡池优化了内存使用

## 贡献

欢迎提交 Pull Request 来帮助改进这个项目！

## 许可证

本项目采用 MIT 许可证 - 查看 [LICENSE](LICENSE) 文件了解详情。
