using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Graphics;
using System.Drawing;
using Color = Microsoft.Maui.Graphics.Color;
using SnakeGame.Models.GameInfo;
using static SnakeGame.Models.GameInfo.Enums;
using SnakeGame.Services;
using System.Diagnostics;
using System.Text.Json.Serialization;
using System.Text.Json;
using SnakeGame.Models.Github;

namespace SnakeGame;

public partial class MainViewModel : ObservableObject
{
    private IGameService _game;
    private IRecordsService _recordsService;
    private IGithubUpdateService _github;

    // Ниже параметры для инпут лага кнопок движения:
    private DateTime? _lastTappedDirection = null;
    private int _directionTimeoutMs = 160;
    private int _numberOfTaps = 0;

    private GameStatus Status => _game.Status;

    public MainViewModel(IRecordsService recordsService, IGithubUpdateService github)
    {
        _recordsService = recordsService;
        _github = github;

        // Подрубаем игровой сервис и задаем размеры поля + генерируем карту
        _game = new GameService();
        _game.FieldUpdated += OnFieldUpdated;
        _game.InitializeNewGame(14);

        IsDifficultyVisible = true;
        IsStartVisible = false;
        AreControlsVisible = false;
        SetStartBtnText();
    }

    [ObservableProperty]
    private string _playerName = Preferences.Default.Get("PlayerName", string.Empty);

    [ObservableProperty]
    private bool _showDebugMessage = false; // Флаг показа сообщения о включенном режиме отладки

    [ObservableProperty]
    private bool _IsNameChangeEnabled = false; // Флаг переключения режима смены имени (редактировать / подтвердить)

    [ObservableProperty]
    private bool _showPencil = true; // флаг иконки картинки "Редактировать"

    [ObservableProperty]
    private bool _showCheckmark = false; // Флаг иконки "Подтвердить"

    [ObservableProperty]
    private Color _nameChangeColor = Color.FromRgba("#e2c11d");

    [ObservableProperty]
    private string _gameField = string.Empty; // Видимое игровое поле

    [ObservableProperty]
    private bool _isDifficultyVisible; // Флаг отображения кнопок сложности

    [ObservableProperty]
    private bool _isStartVisible; // Флаг видимости кнопки запуска игры

    [ObservableProperty]
    private bool _areControlsVisible; // Флаг отображения кнопок управления

    [ObservableProperty]
    private Direction _next; // Направление движения змеи

    [ObservableProperty]
    private string _start = "START"; // Текст кнопки запуска новой игры

    [ObservableProperty]
    private string _score; // Параметр вывода набранных очков на экран

    [ObservableProperty]
    private bool _canGoBack = false; // Видна ли кнопка возврата к выбору сложности

    [ObservableProperty]
    private string _difficultyText = string.Empty; // Текст с названием пресета сложности:

    [ObservableProperty]
    private string _diffEasyText = "Юнга 🤓"; // Легкая

    [ObservableProperty]
    private string _diffMediumText = "Бывалый 🧙‍♂️"; // Средняя

    [ObservableProperty]
    private string _diffHardText = "Джигит 👺"; // Высокая

    [ObservableProperty]
    public bool _isThereUpdate = false; // Флаг наличия обновы

    [ObservableProperty]
    public string _updLink = "https://github.com/witchgen/SnekGame/releases/latest";

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
        catch (Exception ex) {
            // todo: Вывести сообщение
        }
    }

    [RelayCommand]
    public async Task GoGetUpdate()
    {
        await Browser.Default.OpenAsync(_updLink, BrowserLaunchMode.SystemPreferred);
    }

    [RelayCommand]
    private void ChangeName() // Задаем текущее имя игрока (будет использовано в записи рекорда)
    {
        var buffer = NameChangeColor;

        ShowPencil = !ShowPencil;
        ShowCheckmark = !ShowCheckmark;

        NameChangeColor = ShowCheckmark ? Color.FromRgba("#6bb86b") : Color.FromRgba("#e2c11d");
        //NameChangeEditImageSrc = IsNameChangeEnabled ? "checkmark.png" : "pencil.edit.png";
        
        if (!string.IsNullOrWhiteSpace(PlayerName))
        {
            Preferences.Default.Set("PlayerName", PlayerName);
        }
        SetStartBtnText();
    }

    [RelayCommand]
    private async Task GoToRecords()
    {
        await Shell.Current.GoToAsync("LeaderboardsPage");
    }

    [RelayCommand]
    private void SelectDifficulty(string lvl)
    {
        _game.SetDifficulty((Difficulty)Convert.ToInt32(lvl));
        DifficultyText = "Сложнасть: " + lvl switch
        {
            "0" => DiffEasyText,
            "1" => DiffMediumText,
            "2" => DiffHardText,
            _ => "Хз"
        };
        IsDifficultyVisible = false;
        IsStartVisible = true;
        CanGoBack = true;

        _game.InitializeNewGame(14);
    }

    [RelayCommand]
    private void GoBack()
    {
        IsStartVisible = false;
        IsDifficultyVisible = true;
        CanGoBack = false;
    }

    [RelayCommand]
    private async Task PauseActiveGame()
    {
        _numberOfTaps++;

        if ( (Status == GameStatus.Initialized || Status == GameStatus.Ended) && _numberOfTaps > 4 )
        {
            _game.SwitchDebugMode();

            ShowDebugMessage = !ShowDebugMessage;
            _numberOfTaps = 0;
        }

        if (Status == GameStatus.Running)
        {
            _game.PauseGame();
            Score = "- ПАУЗА -";
            return;
        }

        if (Status == GameStatus.Paused)
        {
            //return;
            StartGameCommand.Execute(null);
        }
    }

    public void ForcePauseFromSystem()
    {
        if (_game.Status == GameStatus.Running)
        {
            _game.PauseGame();
            Score = "- ПАУЗА -";
        }
    }

    private GameState CreateNewState()
    {
        return new GameState
        {
            IsNewGame = true,
            CurrentGameData = new PlayData
            {
                PlayerName = PlayerName,
            }
        };
    }

    // Ждем нужное состояние игрового цикла
    // ИНИЦИАЛИЗИРОВАНА либо ЗАКОНЧЕНА - рисуем кнопки сложности и вариант начать новый раунд
    // НА ПАУЗЕ - ждем снятия с паузы и передаем хранимое состояние игрового процесса
    // ЗАКОНЧЕНА - показываем итоги и вызываем сохранение записи
    [RelayCommand]
    private async Task StartGame()
    {
        GameStatus result;

        _lastTappedDirection = DateTime.UtcNow;

        if (Status == GameStatus.Initialized || Status == GameStatus.Ended)
        {
            IsStartVisible = false;
            AreControlsVisible = true;

            result = await _game.StartNewGame(CreateNewState());
        }
        else if (Status == GameStatus.Paused)
        {
            result = await _game.ContinueGame( _game.GetGameState() );
        }
        else
            return;

        if (result == GameStatus.Paused)
            return;

        if (result == GameStatus.Ended)
            ShowEndgameScreen();
    }

    // Показываем игроку итоги
    private void ShowEndgameScreen()
    {
        var result = _game.GetGameState().CurrentGameData;

        IsStartVisible = false;
        AreControlsVisible = false;
        IsDifficultyVisible = true;

        Score = result.DeathReason == GameOverReason.Starvation
            ? $"Вкусностей не осталось, твой счёт: {result.Score} очков"
            : $"💀 GG WP, у тебя {result.Score} очков 💀";

        _recordsService.AddNewRecordAsync(result);
    }


    [RelayCommand]
    private async Task SaveRecords()
    {
        await _recordsService.SaveToFileAsync();
    }

    // Херачим полюшко-поле, привязанное к наблюдаемому массиву игрового сервиса
    private void OnFieldUpdated(object? sender, string fieldValue)
    {
        // Обновляем ObservableProperty

        MainThread.BeginInvokeOnMainThread(() =>
        {
            GameField = fieldValue;
            Score = Status == GameStatus.Running
            ? _game.GetCurrentScore()
            : Score;
        });
    }

    // Считываем нажатие кнопки направления (с задержкой)
    [RelayCommand]
    private void SetDirection(Int16 newDir)
    {
        if (Status != GameStatus.Paused)
        {
            var timeBuffer = DateTime.UtcNow;

            if((timeBuffer - _lastTappedDirection).Value.TotalMilliseconds > _directionTimeoutMs)
            {
                _game.ChangeDirection((Direction) newDir);

                _lastTappedDirection = DateTime.UtcNow;
            }
        }
    }

    private void SetStartBtnText()
    {
        if(string.IsNullOrWhiteSpace(PlayerName))
        {
            Start = "Стартуй, безымянный ниндзя 🥷";
        }
        else 
            Start = $"Стартуй же, {PlayerName}!";
    }
}
