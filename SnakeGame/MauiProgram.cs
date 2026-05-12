using CommunityToolkit.Maui;
using DrawnUi.Draw;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using SkiaSharp.Views.Maui.Controls.Hosting;
using SnakeGame.Services;
using SnakeGame.SnekEngine;
using SnakeGame.SnekEngine.Abstractions.Interfaces;
using SnakeGame.SnekEngine.Core.Services;
using SnakeGame.SnekEngine.Rendering;
using SnakeGame.SnekEngine.World;
using SnakeGame.ViewModels;
using SnakeGame.Views;
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
            .UseSkiaSharp()
            .UseDrawnUi(new DrawnUiStartupSettings
            {
                MobileIsFullscreen = true,
            })
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddSingleton<IGameService, LegacyGameService>();
        builder.Services.AddSingleton<IBigThinkSnakeService, BigThinkSnakeService>();
        builder.Services.AddSingleton<IRecordsService, RecordsService>();
        builder.Services.AddSingleton<IGithubUpdateService, GithubUpdateService>();
        builder.Services.AddTransient<LegacyGameViewModel>();
        builder.Services.AddTransient<MainMenuViewModel>();
        builder.Services.AddTransient<LegacyGamePage>();
        builder.Services.AddTransient<LeaderboardsPage>();
        builder.Services.AddTransient<LeaderboardViewModel>();
        builder.Services.AddTransient<OptionsViewModel>();
        builder.Services.AddTransient<MainMenu>();
        builder.Services.AddTransient<OptionsPage>();

        builder.Services.AddSingleton<FieldInitializer>();
        builder.Services.AddSingleton<FieldUpdater>();
        builder.Services.AddSingleton<GameLoopService>();
        //builder.Services.AddSingleton<GameRenderer>();
        builder.Services.AddSingleton<IGameplayService, GameplayService>();
        builder.Services.AddSingleton<IGraphicRenderService, GraphicRenderService>();
        builder.Services.AddSingleton<GameDispatcher>();
        builder.Services.AddTransient<GamePage>();
        builder.Services.AddTransient<GameViewModel>();

        return builder.Build();
    }
}
