using Euynac.Monica.GachaPool.Abstractions;

namespace Test.Euynac.Monica.GachaPool.Support;

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
