using CardPool.Implements;
using CardPool.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace CardPool.Extensions;

/// <summary>
/// Extension methods for configuring CardPool services in an IServiceCollection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds CardPool services to the specified IServiceCollection.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <returns>The IServiceCollection so that additional calls can be chained.</returns>
    public static IServiceCollection AddCardPool(this IServiceCollection services)
    {
        services.AddSingleton<ICardPoolManager, CardPoolManager>();
        services.AddSingleton<ICardsPoolLoader, CardsPoolByMemoryProvider>();
        return services;
    }
}