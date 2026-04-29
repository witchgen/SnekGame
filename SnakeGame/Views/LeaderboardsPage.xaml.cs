using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Mvvm.Input;

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


