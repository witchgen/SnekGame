using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using SnakeGame.ViewModels;
using System.Threading.Tasks;

namespace SnakeGame.Views;

public partial class MainMenu : ContentPage
{
	public MainMenu(MainMenuViewModel mmvm)
	{
		InitializeComponent();
		BindingContext = mmvm;
	}
}