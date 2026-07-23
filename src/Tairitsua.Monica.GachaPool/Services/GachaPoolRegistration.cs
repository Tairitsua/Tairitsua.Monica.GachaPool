using Tairitsua.Monica.GachaPool.Abstractions;
using Tairitsua.Monica.GachaPool.Abstractions.Internal;
using Tairitsua.Monica.GachaPool.Models;

namespace Tairitsua.Monica.GachaPool.Services;

internal sealed class GachaPoolRegistration<TPrize>(GachaPoolDefinition<TPrize> definition)
    : IGachaPoolRegistration where TPrize : notnull
{
    public string PoolId => definition.Id;

    public IGachaPoolRuntime CreateRuntime(int recentDrawHistoryLimit, IGachaRandomSource randomSource)
    {
        return new GachaPoolRuntime<TPrize>(definition, recentDrawHistoryLimit, randomSource);
    }
}
