using Microsoft.Extensions.DependencyInjection;

namespace CardPool.Core;

/// <summary>
/// Provides extension methods for registering CardPool services with dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds CardPool services to the specified IServiceCollection.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <returns>The IServiceCollection for chaining.</returns>
    public static IServiceCollection AddCardPool(this IServiceCollection services)
    {
        services.AddScoped<ICardsPool, CardsPool>();
        services.AddScoped<ICardDrawer, CardDrawer>();
        services.AddScoped<ICardDrawStatistician, CardDrawStatistician>();
        
        return services;
    }
    
    /// <summary>
    /// Adds CardPool services with generic card drawer to the specified IServiceCollection.
    /// </summary>
    /// <typeparam name="T">The type of card to use with the generic card drawer.</typeparam>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <returns>The IServiceCollection for chaining.</returns>
    public static IServiceCollection AddCardPool<T>(this IServiceCollection services) where T : Card<T>
    {
        services.AddScoped<ICardsPool, CardsPool>();
        services.AddScoped<ICardDrawer<T>, CardDrawer<T>>();
        services.AddScoped<ICardDrawStatistician, CardDrawStatistician>();
        
        return services;
    }
} 