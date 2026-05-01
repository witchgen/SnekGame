using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

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
        protected override void OnSleep()
        {
            base.OnSleep();

            if( Current?.MainPage?.BindingContext is MainViewModel mvm)
            {
                mvm.ForcePauseFromSystem();
            }
        }

        protected override async void OnStart()
        {
            base.OnStart();

            SetUpdateCheck();
        }

        protected override void OnResume()
        {
            base.OnResume();

            SetUpdateCheck();
        }

        private async void SetUpdateCheck()
        {
            var lastCheck = Preferences.Get("LastUpdateCheck", DateTime.MinValue);

            var currentTimestamp = DateTime.UtcNow;

            if ((currentTimestamp - lastCheck) >= TimeSpan.FromHours(2))
            {
                // Получаем текущую страницу из Shell
                Page? currentPage = null;

                if (Current?.MainPage is Shell shell)
                {
                    currentPage = shell.CurrentPage;
                }
                else
                {
                    currentPage = Current?.MainPage;
                }

                if (currentPage?.BindingContext is MainViewModel mvm)
                {
                    await mvm.CheckForUpdates();
                }

                Preferences.Set("LastUpdateCheck", currentTimestamp);
            }
        }
    }
}