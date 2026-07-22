using Euynac.Monica.GachaPool.Abstractions;
using Euynac.Monica.GachaPool.Abstractions.Internal;
using Euynac.Monica.GachaPool.Facades;
using Euynac.Monica.GachaPool.Localization;
using Euynac.Monica.GachaPool.Models;
using Euynac.Monica.GachaPool.Providers.System;
using Euynac.Monica.GachaPool.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Monica.Core;
using Monica.Core.Modularity;
using Monica.Core.Modularity.Abstractions;
using Monica.Core.Modularity.Annotations;
using Monica.Core.Modularity.Models;
using Monica.Modules;

namespace Euynac.Monica.GachaPool.Modules;

/// <summary>
/// Registers the host-scoped gacha pool catalog, weighted draw engine, and Facade.
/// </summary>
[ModuleKey("Euynac.Monica.GachaPool")]
public sealed class ModuleGachaPool(ModuleGachaPoolOption option)
    : ModuleBase<ModuleGachaPool, ModuleGachaPoolOption, ModuleGachaPoolGuide>(option)
{
    /// <inheritdoc />
    public override void ClaimDependencies()
    {
        DependsOnModule<ModuleLocalizationGuide>().Register()
            .AddResource<GachaPoolResource>();
    }

    /// <inheritdoc />
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddOptions<ModuleGachaPoolOption>()
            .Validate(
                static options => options.RecentDrawHistoryLimit is >= 0 and <= 500,
                $"{nameof(ModuleGachaPoolOption.RecentDrawHistoryLimit)} must be between 0 and 500.")
            .Validate(
                static options => options.MaximumBatchSize is >= 1 and <= 1000,
                $"{nameof(ModuleGachaPoolOption.MaximumBatchSize)} must be between 1 and 1000.")
            .ValidateOnStart();

        services.TryAddSingleton<IGachaRandomSource, SystemGachaRandomSource>();
        services.TryAddSingleton<IGachaPoolCatalog, GachaPoolCatalog>();
        services.AddScoped<GachaPoolFacade>();
    }
}

/// <summary>
/// Adds the GachaPool infrastructure module to a Monica host.
/// </summary>
public static class ModuleGachaPoolBuilderExtensions
{
    extension(IMonicaBuilder builder)
    {
        /// <summary>
        /// Registers the gacha pool engine and host-owned catalog.
        /// </summary>
        /// <param name="action">Optional callback that configures history and batch limits.</param>
        /// <returns>A guide used to register immutable pool definitions.</returns>
        public ModuleGachaPoolGuide AddGachaPool(Action<ModuleGachaPoolOption>? action = null)
        {
            return builder.AddModule<ModuleGachaPool, ModuleGachaPoolOption, ModuleGachaPoolGuide>(action);
        }
    }
}

/// <summary>
/// Configures immutable gacha pools that the host materializes when its service provider is built.
/// </summary>
public sealed class ModuleGachaPoolGuide
    : ModuleGuide<ModuleGachaPool, ModuleGachaPoolOption, ModuleGachaPoolGuide>
{
    /// <summary>
    /// Registers one typed pool definition.
    /// </summary>
    /// <typeparam name="TPrize">The publisher-owned prize value type.</typeparam>
    /// <param name="definition">An immutable, validated pool definition.</param>
    /// <returns>The current guide.</returns>
    /// <remarks>
    /// Pool identifiers are unique under ordinal case-insensitive comparison. Registering two different definitions
    /// with the same identifier causes catalog materialization to fail rather than silently replacing one.
    /// </remarks>
    public ModuleGachaPoolGuide AddPool<TPrize>(GachaPoolDefinition<TPrize> definition) where TPrize : notnull
    {
        ArgumentNullException.ThrowIfNull(definition);

        ConfigureServices(
            context => context.Services.AddSingleton<IGachaPoolRegistration>(
                new GachaPoolRegistration<TPrize>(definition)),
            secondKey: $"{definition.Id}:{Guid.NewGuid():N}");
        return this;
    }
}

/// <summary>
/// Controls bounded operational behavior for the GachaPool module.
/// </summary>
public sealed class ModuleGachaPoolOption : ModuleOptions<ModuleGachaPool>
{
    /// <summary>
    /// Gets or sets how many recent draw outcomes each pool retains for diagnostics and UI.
    /// The default is 24; set zero to disable history while retaining aggregate statistics.
    /// </summary>
    public int RecentDrawHistoryLimit { get; set; } = 24;

    /// <summary>
    /// Gets or sets the largest batch accepted by <see cref="GachaPoolFacade.DrawMany"/>.
    /// The default is 100 and the supported range is 1 through 1000.
    /// </summary>
    public int MaximumBatchSize { get; set; } = 100;
}
