using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using SnakeGame.Models.LegacyGame.GameInfo;
using SnakeGame.Services.LegacyGame;
using System;
using System.Threading.Tasks;
using static SnakeGame.Models.LegacyGame.GameInfo.Enums;

namespace SnakeGame;

public partial class LegacyGameViewModel : ObservableObject
{
    private IGameService _game;
    private IRecordsService _recordsService;

    // Ниже параметры для инпут лага кнопок движения:
    private DateTime? _lastTappedDirection = null;
    private int _directionTimeoutMs = 80;
    private int _numberOfTaps = 0;
    private GameStatus Status => _game.Status;

    public LegacyGameViewModel(IGameService game,
        IRecordsService recordsService)
    {
        _game = game;
        _recordsService = recordsService;

        //// Подрубаем игровой сервис и задаем размеры поля + генерируем карту
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

    // Метод инициализации при старте
    public async Task InitializeAsync()
    {
        // Загружаем сохранённые значения
        bool savedStateBombHighlight = Preferences.Get("BombHighlightToggled", false);
        bool savedStateAIEnabled = Preferences.Get("SnakeAIToggled", false);
        bool savedStateCustomSpeed = Preferences.Get("CustomGameSpeedEnabled", false);
        bool savedStateAIDebugPath = Preferences.Get("DebugAIPathToggled", false);
        int savedStateInGameSpeed = Preferences.Get("LegacySpeed", 220);

        if(savedStateCustomSpeed)
        {
            _game.ToggleCustomSpeedChange(savedStateCustomSpeed);
            _game.SetIngameDebugSpeed(savedStateInGameSpeed);
        }
        else
        {
            _game.ToggleCustomSpeedChange(false);
        }
        _game.ToggleDebugOption(DebugOption.ToggleBombSpawnAreaHighlight, savedStateBombHighlight);
        _game.ToggleDebugOption(DebugOption.ToggleSnakeAi, savedStateAIEnabled);
        _game.ToggleDebugOption(DebugOption.DrawAIpath, savedStateAIDebugPath);
    }

    [RelayCommand]
    private async Task GoBack() // Возвращаемся на предыдущуюю страницу в стеке Shell
    {
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task GoToRecords()
    {
        await Shell.Current.GoToAsync("LeaderboardsPage");
    }

    [RelayCommand]
    private void SelectDifficulty(string lvl)
    {
        _game.SetDifficulty((LegacyDifficulty)Convert.ToInt32(lvl));
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
    private void CancelGame()
    {
        IsStartVisible = false;
        IsDifficultyVisible = true;
        CanGoBack = false;
    }

    [RelayCommand]
    private async Task PauseActiveGame()
    {
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

    /// <summary>
    /// Если сворачиваем приожение, игра ставится на паузу
    /// </summary>
    /// <returns></returns>
    public void ForcePauseFromSystem()
    {
        if (_game.Status == GameStatus.Running)
        {
            _game.PauseGame();
            Score = "- ПАУЗА -";
        }
    }

    /// <summary>
    /// Если мы нажали один раз кнопку "Назад" на смартфоне - может, случайно - игра ставится на паузу.
    /// Повторное нажатие закрывает текущий экран и завершает сеанс игры
    /// </summary>
    [RelayCommand]
    public async Task UseBackButtonAsSingleTimePause()
    {
        if (_game.Status == GameStatus.Running)
        {
            _game.PauseGame();
            Score = "- ПАУЗА -";
        }
        else
        {
            await _recordsService.SaveToFileAsync();
            await Shell.Current.GoToAsync("..");
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
            await ShowEndgameScreen();
    }

    // Показываем игроку итоги
    private async Task ShowEndgameScreen()
    {
        var result = _game.GetGameState().CurrentGameData;

        IsStartVisible = false;
        AreControlsVisible = false;
        IsDifficultyVisible = true;

        Score = result.DeathReason == LegacyGameOverReason.Victory
            ? $"Вкусностей не осталось, твой счёт: {result.Score} очков"
            : $"💀 GG WP, у тебя {result.Score} очков 💀";

        await _recordsService.AddNewRecordAsync(result);
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
    private void SetDirection(int newDir)
    {
        //if (Status != GameStatus.Paused)
        //{
        //    var timeBuffer = DateTime.UtcNow;

        //    if((timeBuffer - _lastTappedDirection).Value.TotalMilliseconds > _directionTimeoutMs)
        //    {
        //        _game.ChangeDirection((Direction) newDir);

        //        _lastTappedDirection = DateTime.UtcNow;
        //    }
        //}
        _game.ChangeDirection((Direction)newDir);
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
