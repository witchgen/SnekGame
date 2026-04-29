using Android.App;
using Microsoft.Maui;
using Android.Content.PM;

namespace SnakeGame.Platforms.Android
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.UiMode | ConfigChanges.Orientation |
        ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
    }
}
