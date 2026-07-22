using Euynac.Monica.GachaPool.Bridge;
using Euynac.Monica.GachaPool.Modules;
using Euynac.Monica.GachaPool.Pages;
using Monica.Core.Modularity.Extensions;
using Monica.Modules;

var builder = WebApplication.CreateBuilder(args);

builder.AddMonica(monica =>
{
    monica.ConfigureApplication(options =>
    {
        options.ProjectName = "Euynac.Monica.GachaPool.Bridge";
        options.AppName = "GachaPool Studio";
        options.AppId = "euynac-gacha-pool-studio";
    });

    monica.AddGachaPool(options =>
        {
            options.RecentDrawHistoryLimit = 36;
            options.MaximumBatchSize = 250;
        })
        .AddPool(DemoPools.CreateStarlightPool())
        .AddPool(DemoPools.CreateMidnightPool());

    monica.AddGachaPoolUI();
    monica.AddUIShell(options =>
        {
            options.AppName = "GachaPool Studio";
            options.AppId = "community-extension";
            options.AppVersion = "preview";
            options.ShowLanguageSwitcher = true;
        })
        .AddRouteRedirect("/", UIGachaPoolPage.PAGE_URL);
});

var app = builder.Build();

app.UseMonica();
app.MapMonica();
app.Run();
