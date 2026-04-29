using System;
using System.IO;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace SnakeGame
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("MainPage", typeof(MainPage));
            Routing.RegisterRoute("LeaderboardsPage", typeof(LeaderboardsPage));
        }
    }
}