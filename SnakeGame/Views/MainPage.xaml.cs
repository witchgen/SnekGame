using System;
using System.IO;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using SnakeGame;
using System.Text;

namespace SnakeGame
{
    public partial class MainPage : ContentPage
    {
        public MainPage(MainViewModel mvm)
        {
            InitializeComponent();
            BindingContext = mvm;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await (BindingContext as MainViewModel)?.InitializeAsync();
        }

        protected override bool OnBackButtonPressed()
        {
            if(BindingContext is MainViewModel mvm)
            {
                mvm.ForcePauseFromSystem();

                return true;
            }

            return base.OnBackButtonPressed();
        }
    }


}