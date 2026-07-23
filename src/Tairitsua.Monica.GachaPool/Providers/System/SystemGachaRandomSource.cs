using Tairitsua.Monica.GachaPool.Abstractions;

namespace Tairitsua.Monica.GachaPool.Providers.System;

internal sealed class SystemGachaRandomSource : IGachaRandomSource
{
    public double NextUnitInterval()
    {
        return Random.Shared.NextDouble();
    }
}
