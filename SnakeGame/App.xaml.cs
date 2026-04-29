using System;
using System.IO;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

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
    }
}