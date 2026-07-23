using Tairitsua.Monica.GachaPool.Abstractions;

namespace Test.Tairitsua.Monica.GachaPool.Support;

internal sealed class SequenceGachaRandomSource(params double[] values) : IGachaRandomSource
{
    private readonly Queue<double> _values = new(values);
    private readonly Lock _lock = new();

    public double NextUnitInterval()
    {
        lock (_lock)
        {
            return _values.Count == 0 ? 0 : _values.Dequeue();
        }
    }
}
