using Tairitsua.Monica.GachaPool.Models;

namespace Tairitsua.Monica.GachaPool.Models.Internal;

internal sealed class GachaPrizeRuntime<TPrize>(GachaPrizeDefinition<TPrize> definition) where TPrize : notnull
{
    private int? _remainingStock = definition.InitialStock;
    private long _drawCount;

    internal GachaPrizeDefinition<TPrize> Definition { get; } = definition;

    internal bool IsAvailable => _remainingStock is null or > 0;

    internal int? RemainingStock => _remainingStock;

    internal long DrawCount => _drawCount;

    internal void RecordDraw()
    {
        if (_remainingStock is 0)
        {
            throw new InvalidOperationException($"Prize '{Definition.Id}' is exhausted.");
        }

        if (_remainingStock is not null)
        {
            _remainingStock--;
        }

        _drawCount++;
    }

    internal void Reset()
    {
        _remainingStock = Definition.InitialStock;
        _drawCount = 0;
    }
}
