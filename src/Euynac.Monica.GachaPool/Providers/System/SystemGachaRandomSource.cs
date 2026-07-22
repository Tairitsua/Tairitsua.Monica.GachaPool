using Euynac.Monica.GachaPool.Abstractions;

namespace Euynac.Monica.GachaPool.Providers.System;

internal sealed class SystemGachaRandomSource : IGachaRandomSource
{
    public double NextUnitInterval()
    {
        return Random.Shared.NextDouble();
    }
}
