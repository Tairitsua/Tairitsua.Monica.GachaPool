# MoLibrary.GachaPool

MoLibrary.GachaPool is a flexible and efficient .NET library for managing gacha pools with probability-based drawing mechanisms. It provides a robust foundation for implementing gacha systems in games or any application requiring probability-based item selection.

## Language

English | [简体中文](README.zh_CN.md)

## Features

- 🎯 Probability-based gacha drawing system
- 🔄 Support for multiple gacha pools
- 🎲 Customizable rarity and probability settings
- 📊 Built-in draw statistics tracking
- 🧩 Generic type support for custom item types
- 🔌 Easy integration with dependency injection
- 🔒 Thread-safe operations
- 🚀 High-performance implementation

## Installation

Install the package via NuGet:

```bash
dotnet add package MoLibrary.GachaPool
```

## Quick Start

1. First, create your gacha pool loader by inheriting from `CardsPoolByMemoryProvider`:

```csharp
public class MyGameGachaPoolLoader : CardsPoolByMemoryProvider
{
    public override void ConfigurePools()
    {
        // Configure a standard pool with integer-based items
        ConfigurePool("standardPool", pool =>
        {
            var standardItems = Card<int>.CreateMultiCards(CardRarity.OneStar, 1, 2, 3, 4, 5);
            pool.AddCards(standardItems);
            pool.BuildPool();
        });

        // Configure probability settings for different rarities
        ConfigurePool("customPool", pool =>
        {
            pool.SetPoolRarityProbability(CardRarity.OneStar, 0.7)
                .SetPoolRarityProbability(CardRarity.TwoStar, 0.3);
            // Add your items...
            pool.BuildPool();
        });
    }
}
```

2. Register the services in your application:

```csharp
services.AddMemoryCardPool<MyGameGachaPoolLoader>();
```

3. Use the gacha pool manager in your code:

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

## Advanced Usage

### Custom Item Types

You can create custom item types by inheriting from `Card<T>`:

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

Then use it with the generic drawer:

```csharp
var drawer = _poolManager.GetDrawer<CharacterItem>("characterPool");
var character = drawer?.DrawCard();
```

### Probability Configuration

You can configure probabilities for different item rarities:

```csharp
ConfigurePool("myPool", pool =>
{
    pool.SetPoolRarityProbability(CardRarity.OneStar, 0.6)
        .SetPoolRarityProbability(CardRarity.TwoStar, 0.3)
        .SetPoolRarityProbability(CardRarity.ThreeStar, 0.1);
    // Add items...
    pool.BuildPool();
});
```

### Draw Statistics

The library automatically tracks draw statistics:

```csharp
var drawer = _poolManager.GetDrawer("myPool");
var stats = drawer?.Statistician.GetReport().GetTableString();
Console.WriteLine(stats);
```

## Performance Considerations

- The library uses thread-safe collections for concurrent access
- Gacha pools are built once and cached for subsequent draws
- Drawing operations are optimized using binary search
- Memory usage is optimized for large pools

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

