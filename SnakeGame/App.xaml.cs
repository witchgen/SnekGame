using System;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using SnakeGame.ViewModels;

namespace SnakeGame
{
    public partial class App : Application
    {
        public App(AppShell shell)
        {
            InitializeComponent();
            MainPage = shell;
            Application.Current.UserAppTheme = AppTheme.Dark;
        }

        /// <summary>
        /// Получаем текущую страницу из Shell
        /// </summary>
        private Page? GetCurrentPage()
        {
            Page? currentPage = null;

            if (Current?.MainPage is Shell shell)
            {
                currentPage = shell.CurrentPage;
            }
            else
            {
                currentPage = Current?.MainPage;
            }

            return currentPage;
        }

        protected override void OnSleep()
        {
            if (GetCurrentPage()?.BindingContext is LegacyGameViewModel lgvm)
            {
                lgvm.ForcePauseFromSystem();
            }

            base.OnSleep();            
        }

        protected override async void OnStart()
        {
            base.OnStart();

            await SetUpdateCheck();
        }

        protected override async void OnResume()
        {
            base.OnResume();

            await SetUpdateCheck();
        }

        private async Task SetUpdateCheck()
        {
            var lastCheck = Preferences.Get("LastUpdateCheck", DateTime.MinValue);

            var currentTimestamp = DateTime.UtcNow;

            if ((currentTimestamp - lastCheck) >= TimeSpan.FromHours(2))
            {

                if (GetCurrentPage()?.BindingContext is MainMenuViewModel mmvm)
                {
                    try
                    {
                        await mmvm.CheckForUpdates();
                        Preferences.Set("LastUpdateCheck", currentTimestamp);
                    }
                    catch
                    {
                        return;
                    }
                }
                
            }
        }
    }
}