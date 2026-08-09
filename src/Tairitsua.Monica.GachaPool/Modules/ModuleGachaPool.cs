using Tairitsua.Monica.GachaPool.Abstractions;
using Tairitsua.Monica.GachaPool.Abstractions.Internal;
using Tairitsua.Monica.GachaPool.Facades;
using Tairitsua.Monica.GachaPool.Localization;
using Tairitsua.Monica.GachaPool.Models;
using Tairitsua.Monica.GachaPool.Providers.System;
using Tairitsua.Monica.GachaPool.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Monica.Core;
using Monica.Core.Modularity;
using Monica.Core.Modularity.Abstractions;
using Monica.Core.Modularity.Models;
using Monica.Modules;

namespace Tairitsua.Monica.GachaPool.Modules;

/// <summary>
/// Registers the host-scoped gacha pool catalog, weighted draw engine, and Facade.
/// </summary>
public sealed class ModuleGachaPool : MonicaModule<ModuleGachaPoolOption>
{
    /// <inheritdoc />
    public override void Describe(ModuleDescriptor module)
    {
        module.Require<ModuleLocalization, ModuleLocalizationOption>(localization =>
        {
            if (!localization.ResourceMarkerTypes.Contains(typeof(GachaPoolResource)))
            {
                localization.ResourceMarkerTypes.Add(typeof(GachaPoolResource));
            }
        });
    }

    /// <inheritdoc />
    public override void ValidateOptions(ModuleGachaPoolOption options, string? profileName)
    {
        if (options.RecentDrawHistoryLimit is < 0 or > 500)
        {
            throw new InvalidOperationException(
                $"{nameof(ModuleGachaPoolOption.RecentDrawHistoryLimit)} must be between 0 and 500.");
        }

        if (options.MaximumBatchSize is < 1 or > 1000)
        {
            throw new InvalidOperationException(
                $"{nameof(ModuleGachaPoolOption.MaximumBatchSize)} must be between 1 and 1000.");
        }
    }

    /// <inheritdoc />
    public override void ConfigureServices(ModuleContext<ModuleGachaPoolOption> context)
    {
        var services = context.Services;
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
        /// <returns>The host-bound module registration.</returns>
        public ModuleRegistration<ModuleGachaPool, ModuleGachaPoolOption> AddGachaPool(
            Action<ModuleGachaPoolOption>? action = null)
        {
            return builder.AddModule<ModuleGachaPool, ModuleGachaPoolOption>(action);
        }
    }
}

/// <summary>
/// Adds host-owned pool definitions to a GachaPool module registration.
/// </summary>
public static class ModuleGachaPoolRegistrationExtensions
{
    extension(ModuleRegistration<ModuleGachaPool, ModuleGachaPoolOption> registration)
    {
        /// <summary>
        /// Registers one typed pool definition.
        /// </summary>
        /// <typeparam name="TPrize">The publisher-owned prize value type.</typeparam>
        /// <param name="definition">An immutable, validated pool definition.</param>
        /// <returns>The current host-bound module registration.</returns>
        /// <remarks>
        /// Pool identifiers are unique under ordinal case-insensitive comparison. Registering two different definitions
        /// with the same identifier causes catalog materialization to fail rather than silently replacing one.
        /// </remarks>
        public ModuleRegistration<ModuleGachaPool, ModuleGachaPoolOption> AddPool<TPrize>(
            GachaPoolDefinition<TPrize> definition)
            where TPrize : notnull
        {
            ArgumentNullException.ThrowIfNull(definition);

            return registration.ConfigureServices(context =>
                context.Services.AddSingleton<IGachaPoolRegistration>(
                    new GachaPoolRegistration<TPrize>(definition)));
        }
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
