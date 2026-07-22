using Euynac.Monica.GachaPool.Abstractions;
using Euynac.Monica.GachaPool.Abstractions.Internal;
using Euynac.Monica.GachaPool.Models;

namespace Euynac.Monica.GachaPool.Services;

internal sealed class GachaPoolRegistration<TPrize>(GachaPoolDefinition<TPrize> definition)
    : IGachaPoolRegistration where TPrize : notnull
{
    public string PoolId => definition.Id;

    public IGachaPoolRuntime CreateRuntime(int recentDrawHistoryLimit, IGachaRandomSource randomSource)
    {
        return new GachaPoolRuntime<TPrize>(definition, recentDrawHistoryLimit, randomSource);
    }
}
