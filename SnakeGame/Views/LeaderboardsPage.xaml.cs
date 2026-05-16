using Microsoft.Maui.Controls;
using SnakeGame.ViewModels;

namespace SnakeGame;

public partial class LeaderboardsPage : ContentPage
{
    public LeaderboardsPage(LeaderboardViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        //if (BindingContext is LeaderboardViewModel vm)
        //{
        //    await vm.LoadRecords();
        //}
    }

}


