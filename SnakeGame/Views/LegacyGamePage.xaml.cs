using System;
using System.IO;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using SnakeGame;
using System.Text;

namespace SnakeGame
{
    public partial class LegacyGamePage : ContentPage
    {
        public LegacyGamePage(LegacyGameViewModel mvm)
        {
            InitializeComponent();
            BindingContext = mvm;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await (BindingContext as LegacyGameViewModel)?.InitializeAsync();
        }

        protected override bool OnBackButtonPressed()
        {
            if(BindingContext is LegacyGameViewModel mvm)
            {
                mvm.ForcePauseFromSystem();

                return true;
            }

            return base.OnBackButtonPressed();
        }
    }


}