using Tairitsua.Monica.GachaPool.Localization;
using Tairitsua.Monica.GachaPool.Pages;
using Tairitsua.Monica.GachaPool.UIGachaPool.State;
using Microsoft.Extensions.DependencyInjection;
using Monica.Core;
using Monica.Core.Modularity;
using Monica.Core.Modularity.Abstractions;
using Monica.Core.Modularity.Annotations;
using Monica.Modules;
using MudBlazor;

namespace Tairitsua.Monica.GachaPool.Modules;

/// <summary>
/// Registers the localized GachaPool dashboard in the Monica UI shell.
/// </summary>
[ModuleKey("Tairitsua.Monica.GachaPool.UI")]
public sealed class ModuleGachaPoolUI(ModuleGachaPoolUIOption option)
    : ModuleBase<ModuleGachaPoolUI, ModuleGachaPoolUIOption, ModuleGachaPoolUIGuide>(option)
{
    /// <inheritdoc />
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<GachaPoolDashboardState>();
    }

    /// <inheritdoc />
    public override void ClaimDependencies()
    {
        DependsOnModule<ModuleGachaPoolGuide>().Register();
        if (Option.DisableDashboardPage)
        {
            return;
        }

        DependsOnModule<ModuleShellUIGuide>().Register()
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
        /// <param name="action">Optional callback that can disable page registration.</param>
        /// <returns>The GachaPool UI guide.</returns>
        public ModuleGachaPoolUIGuide AddGachaPoolUI(Action<ModuleGachaPoolUIOption>? action = null)
        {
            return builder.AddModule<ModuleGachaPoolUI, ModuleGachaPoolUIOption, ModuleGachaPoolUIGuide>(action);
        }
    }
}

/// <summary>
/// Fluent configuration guide for the GachaPool UI module.
/// </summary>
public sealed class ModuleGachaPoolUIGuide
    : ModuleGuide<ModuleGachaPoolUI, ModuleGachaPoolUIOption, ModuleGachaPoolUIGuide>
{
}

/// <summary>
/// Controls page registration for the GachaPool UI module.
/// </summary>
public sealed class ModuleGachaPoolUIOption : ModuleOptions<ModuleGachaPoolUI>
{
    /// <summary>
    /// Gets or sets whether the dashboard page is omitted while the infrastructure module remains available.
    /// The default is <see langword="false"/>.
    /// </summary>
    public bool DisableDashboardPage { get; set; }
}
