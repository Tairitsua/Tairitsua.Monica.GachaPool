using Tairitsua.Monica.GachaPool.Localization;
using Tairitsua.Monica.GachaPool.Pages;
using Tairitsua.Monica.GachaPool.UIGachaPool.State;
using Microsoft.Extensions.DependencyInjection;
using Monica.Core;
using Monica.Core.Modularity;
using Monica.Core.Modularity.Abstractions;
using Monica.Modules;
using MudBlazor;

namespace Tairitsua.Monica.GachaPool.Modules;

/// <summary>
/// Registers the localized GachaPool dashboard in the Monica UI shell.
/// </summary>
public sealed class ModuleGachaPoolUI : MonicaModule<ModuleGachaPoolUIOption>, IUIModule
{
    /// <inheritdoc />
    public override void Describe(ModuleDescriptor module)
    {
        module.Require<ModuleGachaPool, ModuleGachaPoolOption>();
        module.Require<ModuleLocalization, ModuleLocalizationOption>();
        module.Require<ModuleShellUI, ModuleShellUIOption>();
    }

    /// <inheritdoc />
    public override void ConfigureServices(ModuleContext<ModuleGachaPoolUIOption> context)
    {
        context.Services.AddScoped<GachaPoolDashboardState>();
    }
}

/// <summary>
/// Adds the GachaPool UI module to a Monica host.
/// </summary>
public static class ModuleGachaPoolUIBuilderExtensions
{
    extension(IMonicaBuilder builder)
    {
        /// <summary>
        /// Registers the GachaPool dashboard and its transitive infrastructure dependency.
        /// </summary>
        /// <param name="action">Optional module configuration callback.</param>
        /// <returns>The host-bound GachaPool UI module registration.</returns>
        public ModuleRegistration<ModuleGachaPoolUI, ModuleGachaPoolUIOption> AddGachaPoolUI(
            Action<ModuleGachaPoolUIOption>? action = null)
        {
            var registration = builder.AddModule<ModuleGachaPoolUI, ModuleGachaPoolUIOption>(action);
            registration.Require<ModuleLocalization, ModuleLocalizationOption>()
                .AddResource<GachaPoolResource>();
            registration.Require<ModuleShellUI, ModuleShellUIOption>()
                .RegisterUIComponents(registry =>
                {
                    var categoryId = registry.RegisterLocalizedCategory<GachaPoolResource>(
                        "Tairitsua.Monica.GachaPool",
                        "Navigation:Category",
                        order: 450);
                    registry.RegisterLocalizedPage<UIGachaPoolPage, GachaPoolResource>(
                        UIGachaPoolPage.PAGE_URL,
                        "Navigation:Title",
                        Icons.Material.Filled.AutoAwesome,
                        categoryId: categoryId,
                        addToNav: true,
                        navOrder: 42);
                });
            return registration;
        }
    }
}

/// <summary>
/// GachaPool UI module options.
/// </summary>
public sealed class ModuleGachaPoolUIOption : ModuleOptions<ModuleGachaPoolUI>
{
}
