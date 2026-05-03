// ViewModels/LeaderboardViewModel.cs
using System.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using static SnakeGame.Models.GameInfo.GameState;
using SnakeGame.Services;
using SnakeGame.Models.GameInfo;

namespace SnakeGame;

public partial class LeaderboardViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isRefreshing = false; // Флаг для графики крутилки обновления списка

    private IRecordsService _recordsService;

    public ObservableCollection<PlayData> Records => _recordsService.GetRecords();

    public LeaderboardViewModel(IRecordsService recordsService)
    {
        _recordsService = recordsService;
        _recordsService.LoadFromFile();
    }

    [RelayCommand]
    public async Task LoadRecords() // Если свайпнули, обновляем список
    {
        try
        {
            IsRefreshing = true;

            await _recordsService.LoadFromFile();
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task GoBack() // Возвращаемся на предыдущуюю страницу в стеке Shell
    {
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task SaveRecords()
    {
        await _recordsService.SaveToFileAsync();
    }

    [RelayCommand]
    private void ItemTapped(PlayData record)
    {
        // Обработка нажатия на будущее
        return;
        //System.Diagnostics.Debug.WriteLine($"Tapped: {record.PlayerName}");
    }
}
