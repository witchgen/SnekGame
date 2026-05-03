using Android.Preferences;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using SnakeGame.Models.Github;
using SnakeGame.Services;
using SnakeGame.Views;
using System;
using System.Threading.Tasks;
using static SnakeGame.Models.GameInfo.Enums;

namespace SnakeGame.ViewModels
{
    public partial class MainMenuViewModel : ObservableObject
    {
        private IGithubUpdateService _github;
        private Popup _debugPopup = new DebugModal();

        private static readonly string _updLink = "https://github.com/witchgen/SnekGame/releases/latest"; // Ссылка на релизы в GitHub

        private static readonly string _currentName = Preferences.Default.Get("PlayerName", string.Empty);
        private static DateTime _lastTappedSnapshot = DateTime.MinValue;
        private static int _numberOfTaps = 0;

        [ObservableProperty]
        public string _currentVersion = AppInfo.Current.VersionString;

        [ObservableProperty]
        public bool _isThereUpdate = false; // Флаг наличия обновы
        
        [ObservableProperty]
        private bool _showDebug = false; // Флаг показа режима отладки

        [ObservableProperty]
        private string _greeting = string.IsNullOrWhiteSpace(_currentName) ? "С подключением!" : $"Добро пожаловать, {_currentName}";

        public MainMenuViewModel(IGithubUpdateService github)
        {
            _github = github;
        }

        // ================
        // Значения для поп-апа отладки:
        [ObservableProperty]
        public bool _isBombHighlightActive = Preferences.Get("BombHighlightToggled", false); // Флаг отладки "свободной от бомб" позиции
        [ObservableProperty]
        public bool _isSnakeAIActive = Preferences.Get("SnakeAIToggled", false); // Флаг использования змеей "автопилота"
        [ObservableProperty]
        public bool _isGameSpeedSliderActive = Preferences.Get("CustomGameSpeedEnabled", false); // Флаг показа ползунка скорости игры
        [ObservableProperty]
        public bool _isAIPathVisible = Preferences.Get("DebugAIPathToggled", false); // Флаг показа построения пути ИИ
        [ObservableProperty]
        public int _gameSpeedMs = Preferences.Get("LegacySpeed", 220);
        // ================

        [RelayCommand]
        public async Task ShowDebugOptions()
        {
            _debugPopup.BindingContext = this;
            Application.Current.MainPage.ShowPopup(_debugPopup);
        }

        [RelayCommand]
        public Task CloseDebugPopup(Popup popup)
        {
            return popup?.CloseAsync() ?? Task.CompletedTask;
        }

        partial void OnIsBombHighlightActiveChanged(bool value)
        {
            Preferences.Set("BombHighlightToggled", value);
        }

        partial void OnIsSnakeAIActiveChanged(bool value)
        {
            Preferences.Set("SnakeAIToggled", value);
        }

        partial void OnIsAIPathVisibleChanged(bool value)
        {
            Preferences.Set("DebugAIPathToggled", value);
        }

        partial void OnIsGameSpeedSliderActiveChanged(bool value)
        {
            Preferences.Set("CustomGameSpeedEnabled", value);
        }

        partial void OnGameSpeedMsChanged(int value)
        {
            Preferences.Set("LegacySpeed", value);
        }

        [RelayCommand]
        private async Task GoToRecords()
        {
            await Shell.Current.GoToAsync("LeaderboardsPage");
        }

        [RelayCommand]
        private async Task GoToOptions()
        {
            await Shell.Current.GoToAsync("OptionsPage");
        }

        [RelayCommand]
        private async Task GoToLegacyGame()
        {
            await Shell.Current.GoToAsync("LegacyGamePage"); 
        }

        public async Task CheckForUpdates()
        {
            var versionInfo = new ReleaseInfo();
            try
            {
                var newVersionInfo = await _github.CheckForAppUpdates();
                versionInfo = newVersionInfo;
                if (newVersionInfo.IsSuccesfulFetch)
                {
                    var current = AppInfo.Current.Version;
                    var newVersion = Version.Parse(newVersionInfo.Version);

                    if (newVersion > current)
                    {
                        IsThereUpdate = true;
                    }
                    else IsThereUpdate = false;
                }
                else
                {
                    // Здесь уведомить пользователя, что обнов нету
                }
            }
            catch (Exception ex)
            {
                // todo: Вывести сообщение
            }
        }

        [RelayCommand]
        private async Task GoGetUpdate()
        {
            await Browser.Default.OpenAsync(_updLink, BrowserLaunchMode.SystemPreferred);
        }

        [RelayCommand]
        private void AppointDebugOptions()
        {
            var timeBuffer = DateTime.UtcNow;

            if ((timeBuffer - _lastTappedSnapshot).TotalMilliseconds < 2000 && _numberOfTaps >= 4)
            {
                ShowDebug = !ShowDebug;

                _lastTappedSnapshot = DateTime.UtcNow;
                _numberOfTaps = 0;
            }
            else
            {
                _numberOfTaps = 0;
            }
        }
    }
}
