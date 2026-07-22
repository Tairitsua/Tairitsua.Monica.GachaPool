namespace Euynac.Monica.GachaPool.Abstractions.Internal;

internal interface IGachaPoolRegistration
{
    string PoolId { get; }

    IGachaPoolRuntime CreateRuntime(int recentDrawHistoryLimit, IGachaRandomSource randomSource);
}
