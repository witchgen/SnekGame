using Microsoft.Maui.Controls;
using SnakeGame.ViewModels;

namespace SnakeGame.Views;

public partial class OptionsPage : ContentPage
{
	public OptionsPage(OptionsViewModel ovm)
	{
		InitializeComponent();
		BindingContext = ovm;
	}
}