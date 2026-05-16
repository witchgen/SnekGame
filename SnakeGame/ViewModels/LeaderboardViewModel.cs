using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using SnakeGame.Abstractions;
using SnakeGame.Models.LegacyGame.GameInfo;
using SnakeGame.Services.LegacyGame;
using SnakeGame.SnekEngine.Abstractions.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace SnakeGame.ViewModels;

public partial class LeaderboardViewModel : ObservableObject
{
    private IRecordsService _legacy;
    private ISnakeRecordsService _records;

    [ObservableProperty]
    private bool _isRefreshingLegacy;   // Флаг крутилки обновления легаси рекордов

    [ObservableProperty]
    private bool _isRefreshingNew;      // Флаг крутилки для рекордов основной игры

    [ObservableProperty]
    private bool _hasLegacyRecords;     // Флаг наличия рекордов легаси версии

    [ObservableProperty]
    private ObservableCollection<PlayInfo> _newRecords = new();
    public ObservableCollection<PlayData> LegacyRecords => _legacy.GetRecords();

    [ObservableProperty] 
    private bool isNewTabSelected = true;      // Выбран таб рекордов основной игры (по умолчанию)

    [ObservableProperty] 
    private bool isLegacyTabSelected = false;  // Выбран таб легаси рекордов (если вообще есть)

    public LeaderboardViewModel(IRecordsService legacy, ISnakeRecordsService records)
    {
        _legacy = legacy;
        _records = records;
        _legacy.LoadFromFile();

        LoadAll();
    }

    private async void LoadAll()
    {
        await LoadLegacyRecords();
        await LoadNewRecords();
    }

    [RelayCommand]
    public async Task LoadLegacyRecords() // Если свайпнули, обновляем список
    {
        try
        {
            IsRefreshingLegacy = true;

            await _legacy.LoadFromFile();
            HasLegacyRecords = LegacyRecords.Count > 0;
        }
        finally
        {
            IsRefreshingLegacy = false;
        }
    }

    [RelayCommand]
    public async Task LoadNewRecords()
    {
        try
        {
            IsRefreshingNew = true;

            var list = await _records.GetRecordsAsync();
            NewRecords.Clear();
            foreach (var item in list)
                NewRecords.Add(item);
        }
        finally
        {
            IsRefreshingNew = false;
        }
    }

    [RelayCommand]
    private void SelectNewTab()
    {
        IsNewTabSelected = true;
        IsLegacyTabSelected = false;
    }

    [RelayCommand]
    private void SelectLegacyTab()
    {
        IsNewTabSelected = false;
        IsLegacyTabSelected = true;
    }

    [RelayCommand]
    private async Task GoBack() // Возвращаемся на предыдущуюю страницу в стеке Shell
    {
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task SaveRecords()
    {
        await _legacy.SaveToFileAsync();
        await _records.SaveRecordsToFileAsync();
    }

    [RelayCommand]
    private void ItemTapped(PlayInfo record) 
    {
        // Обработка нажатия на будущее
        return;
    }
}
