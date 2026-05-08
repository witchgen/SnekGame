using Microsoft.Maui.Controls;
using SnakeGame.Views;

namespace SnakeGame
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("MainMenu", typeof(MainMenu));
            Routing.RegisterRoute("OptionsPage", typeof(OptionsPage));
            Routing.RegisterRoute("GamePage", typeof(GamePage));
            Routing.RegisterRoute("LegacyGamePage", typeof(LegacyGamePage));
            Routing.RegisterRoute("LeaderboardsPage", typeof(LeaderboardsPage));
        }
    }
}