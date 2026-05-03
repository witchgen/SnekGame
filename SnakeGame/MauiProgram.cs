using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Maui;
using SnakeGame.Services;
using SnakeGame.ViewModels;
//using MauiLib;


namespace SnakeGame;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddSingleton<IGameService, GameService>();
        builder.Services.AddSingleton<IBigThinkSnakeService, BigThinkSnakeService>();
        builder.Services.AddSingleton<IRecordsService, RecordsService>();
        builder.Services.AddSingleton<IGithubUpdateService, GithubUpdateService>();
        builder.Services.AddTransient<LegacyGameViewModel>();
        builder.Services.AddTransient<MainMenuViewModel>();
        builder.Services.AddTransient<LegacyGamePage>();
        builder.Services.AddTransient<LeaderboardsPage>();
        builder.Services.AddTransient<LeaderboardViewModel>();

        return builder.Build();
    }
}
