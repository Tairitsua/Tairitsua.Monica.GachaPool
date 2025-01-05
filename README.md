# CardPool

CardPool is a flexible and efficient .NET library for managing card pools with probability-based drawing mechanisms. It provides a robust foundation for implementing card-drawing systems in games or any application requiring probability-based item selection.

## Features

- 🎯 Probability-based card drawing system
- 🔄 Support for multiple card pools
- 🎲 Customizable rarity and probability settings
- 📊 Built-in draw statistics tracking
- 🧩 Generic type support for custom card types
- 🔌 Easy integration with dependency injection
- 🔒 Thread-safe operations
- 🚀 High-performance implementation

## Installation

Install the package via NuGet:

```bash
dotnet add package CardPool
```

## Quick Start

1. First, create your card pool loader by inheriting from `CardsPoolByMemoryProvider`:

```csharp
public class MyGameCardPoolLoader : CardsPoolByMemoryProvider
{
    public override void ConfigurePools()
    {
        // Configure a standard pool with integer-based cards
        ConfigurePool("standardPool", pool =>
        {
            var standardCards = Card<int>.CreateMultiCards(CardRarity.OneStar, 1, 2, 3, 4, 5);
            pool.AddCards(standardCards);
            pool.BuildPool();
        });

        // Configure probability settings for different rarities
        ConfigurePool("customPool", pool =>
        {
            pool.SetPoolRarityProbability(CardRarity.OneStar, 0.7)
                .SetPoolRarityProbability(CardRarity.TwoStar, 0.3);
            // Add your cards...
            pool.BuildPool();
        });
    }
}
```

2. Register the services in your application:

```csharp
services.AddMemoryCardPool<MyGameCardPoolLoader>();
```

3. Use the card pool manager in your code:

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

## Advanced Usage

### Custom Card Types

You can create custom card types by inheriting from `Card<T>`:

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

Then use it with the generic drawer:

```csharp
var drawer = _poolManager.GetDrawer<CharacterCard>("characterPool");
var character = drawer?.DrawCard();
```

### Probability Configuration

You can configure probabilities for different card rarities:

```csharp
ConfigurePool("myPool", pool =>
{
    pool.SetPoolRarityProbability(CardRarity.OneStar, 0.6)
        .SetPoolRarityProbability(CardRarity.TwoStar, 0.3)
        .SetPoolRarityProbability(CardRarity.ThreeStar, 0.1);
    // Add cards...
    pool.BuildPool();
});
```

### Draw Statistics

The library automatically tracks draw statistics:

```csharp
var drawer = _poolManager.GetDrawer("myPool");
var stats = drawer?.Statistician.GetReportTableString();
Console.WriteLine(stats);
```

## Performance Considerations

- The library uses thread-safe collections for concurrent access
- Card pools are built once and cached for subsequent draws
- Drawing operations are optimized using binary search
- Memory usage is optimized for large card pools

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

