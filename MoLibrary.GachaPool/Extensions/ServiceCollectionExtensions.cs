using MoLibrary.GachaPool.Implements;
using MoLibrary.GachaPool.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;


namespace MoLibrary.GachaPool.Extensions;

/// <summary>
/// Extension methods for configuring CardPool services in an IServiceCollection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds CardPool services with a memory-based card pool loader to the specified IServiceCollection.
    /// </summary>
    /// <typeparam name="TMemoryLoader">The type of the memory-based card pool loader to use.</typeparam>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <returns>The IServiceCollection so that additional calls can be chained.</returns>
    public static IServiceCollection AddMemoryCardPool<TMemoryLoader>(this IServiceCollection services)
        where TMemoryLoader : CardsPoolByMemoryProvider
    {
        services.AddSingleton<ICardPoolManager, CardPoolManager>();
        services.AddSingleton<ICardsPoolLoader, TMemoryLoader>();
        return services;
    }

    /// <summary>
    /// Add autoload card pool
    /// </summary>
    /// <param name="app"></param>
    public static void UseCardPool(this IApplicationBuilder app)
    {
        var loader = app.ApplicationServices.GetRequiredService<ICardsPoolLoader>();
        var manager = app.ApplicationServices.GetRequiredService<ICardPoolManager>();
        loader.LoadPoolsAsync(manager);
    }
    //public static void UseCardPoolEndpoints(this IApplicationBuilder app, string? groupName = "GachaPool")
    //{
    //    app.UseEndpoints(endpoints =>
    //    {
    //        var tagGroup = new List<OpenApiTag>
    //        {
    //            new() { Name = groupName, Description = "GachaPool APIs" }
    //        };
    //        endpoints.Map("/gacha-pool/", async (HttpResponse response, HttpContext context) =>
    //        {


    //        }).WithName("").WithOpenApi(operation =>
    //        {
    //            operation.Summary = "";
    //            operation.Description = "";
    //            operation.Tags = tagGroup;
    //            return operation;
    //        });

    //    });
    //}

}

//example
//public class MyGameCardPoolLoader : CardsPoolByMemoryProvider
//{
//    public override void ConfigurePools()
//    {
//        // Configure a standard pool
//        ConfigurePool("standardPool", pool =>
//        {
//            var standardCards = Card<int>.CreateMultiCards(CardRarity.OneStar, 1, 2, 3, 4, 5);
//            pool.AddCards(standardCards);
//            pool.BuildPool();
//        });

//        // Configure a special pool with generic cards
//        var specialCards = new List<Card<SpecialCard>>();
//        // Add special cards...
//        ConfigurePool("specialPool", specialCards, pool =>
//        {
//            pool.SetPoolRarityProbability(CardRarity.OneStar, 0.7);
//            pool.SetPoolRarityProbability(CardRarity.TwoStar, 0.3);
//        });
//    }
//}

//// In Startup.cs or Program.cs
//services.AddMemoryCardPool<MyGameCardPoolLoader>();