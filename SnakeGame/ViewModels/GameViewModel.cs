using Android.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Storage;
using SkiaSharp;
using SnakeGame.SnekEngine;
using SnakeGame.SnekEngine.Abstractions.Models;
using SnakeGame.SnekEngine.Core.Services;
using System;
using System.Threading.Tasks;
using static SnakeGame.SnekEngine.Abstractions.GameEnums;

namespace SnakeGame.ViewModels
{
    public partial class GameViewModel : ObservableObject
    {
        private readonly GameDispatcher _dispatcher;
        private readonly GameLoopService _loop;

        public event Action? RequestRedraw; // Запрос перерисовки, вызываем при изменении состояния либо изменении самого холста (поменялось поле)
        private float _canvasWidth;
        private float _canvasHeight;

        [ObservableProperty]
        private GameScreenState _screenState = GameScreenState.Setup;

        [ObservableProperty]
        private bool _isSetupVisible = true;
        //public bool IsSetupVisible => ScreenState is GameScreenState.Setup or GameScreenState.GameOver;
        public bool IsGameFieldVisible => ScreenState is GameScreenState.Ready or GameScreenState.Playing or GameScreenState.GameOver;
        [ObservableProperty]
        private bool _canGenerateField = true;

        [ObservableProperty]
        private bool _canStartGame = false;

        [ObservableProperty]
        private bool _isPlaying = false;

        [ObservableProperty]
        private bool _showGameOver = false;

        // Валидация настроек (не зависит от состояния экрана)
        [ObservableProperty]
        private bool _settingsAreValid = true;

        [ObservableProperty]
        private InitialSettings _settings = new()
        {
            Rows = 17,
            Cols = 17,
            SnakeSpawnPointI = 8,
            SnakeSpawnPointJ = 8,
            BombsCount = 1,
            CustomWalls = false,
            SpeedFactor = 1.0f
        };

        public GameViewModel(GameDispatcher dispatcher, GameLoopService loop)
        {
            _dispatcher = dispatcher;
            _loop = loop;
            Settings.PropertyChanged += (s, e) => ValidateSettings();

            _loop.TickCompleted += () => RequestRedraw?.Invoke();
            // Подписка на завершение игры
            _dispatcher.GameEnded += OnGameEnded;
        }


        partial void OnScreenStateChanged(GameScreenState value)
        {
            CanGenerateField = value == GameScreenState.Setup;
            CanStartGame = value == GameScreenState.Ready;
            IsPlaying = value == GameScreenState.Playing || value == GameScreenState.Paused;
            ShowGameOver = value == GameScreenState.GameOver || value == GameScreenState.Ready;
            IsSetupVisible = value is (GameScreenState.Setup or GameScreenState.GameOver or GameScreenState.Ready);
        }

        private void OnGameEnded(GameOverReason reason)
        {
            _loop.Stop(); // останавливаем игровой цикл
            ScreenState = GameScreenState.GameOver;
            //ShowGameOver = true;

            RequestRedraw?.Invoke();
        }

        // Частичный метод, автоматически вызывается при изменении Settings
        partial void OnSettingsChanged(InitialSettings value)
        {
            ValidateSettings();
        }


        // Валидируем при изменении отдельных свойств внутри Settings
        public void ValidateSettings()
        {
            var s = Settings;

            // 1. Проверка размеров поля: 5-99
            bool validDimensions = s.Rows >= 5 && s.Rows <= 99
                                && s.Cols >= 5 && s.Cols <= 99;

            // 2. Проверка позиции спавна змеи в пределах поля
            bool validSpawn = s.SnakeSpawnPointI >= 1 && s.SnakeSpawnPointI < s.Rows
                           && s.SnakeSpawnPointJ >= 1 && s.SnakeSpawnPointJ < s.Cols;

            // 3. Проверка количества бомб с линейной интерполяцией
            // Мин: 1 бомба при 5x5, Макс: 25 бомб при 99x99
            int minBombs = 1;
            int maxBombs = 25;

            // Нормализованный размер поля (0.0 при 5x5, 1.0 при 99x99)
            double normalizedSize = (double)((s.Rows - 5) * (s.Cols - 5))
                                  / ((99 - 5) * (99 - 5));

            int allowedMaxBombs = minBombs + (int)Math.Round(
                (maxBombs - minBombs) * normalizedSize
            );

            bool validBombs = s.BombsCount >= 1 && s.BombsCount <= allowedMaxBombs;

            SettingsAreValid = validDimensions && validSpawn && validBombs;
        }

        // == УПРАВЛЕНИЕ ПРЕДПОЧТЕНИЯМИ: ==
        [RelayCommand]
        public void SavePreferences()
        {
            Preferences.Set("Settings.Rows", Settings.Rows);
            Preferences.Set("Settings.Cols", Settings.Cols);
            Preferences.Set("Settings.SpawnI", Settings.SnakeSpawnPointI);
            Preferences.Set("Settings.SpawnJ", Settings.SnakeSpawnPointJ);
            Preferences.Set("Settings.Bombs", Settings.BombsCount);
            Preferences.Set("Settings.Speed", Settings.SpeedFactor);
        }

        [RelayCommand]
        public void LoadPreferences()
        {
            Settings.Rows = Preferences.Get("Settings.Rows", 17);
            Settings.Cols = Preferences.Get("Settings.Cols", 17);
            Settings.SnakeSpawnPointI = Preferences.Get("Settings.SpawnI", 8);
            Settings.SnakeSpawnPointJ = Preferences.Get("Settings.SpawnJ", 8);
            Settings.BombsCount = Preferences.Get("Settings.Bombs", 1);
            Settings.SpeedFactor = Preferences.Get("Settings.Speed", 1.0f);
        }

        [RelayCommand]
        public void ResetPreferences()
        {
            Settings.Rows = 17;
            Settings.Cols = 17;
            Settings.SnakeSpawnPointI = 8;
            Settings.SnakeSpawnPointJ = 8;
            Settings.BombsCount = 1;
            Settings.SpeedFactor = 1.0f;
        }
        // ================

        /// <summary>
        /// Команда для кнопки инкремента скорости
        /// </summary>
        [RelayCommand]
        public void IncrementStepSpeed()
        {
            if(Settings.SpeedFactor < 2.5f)
                Settings.SpeedFactor += 0.1f;
        }

        /// <summary>
        /// Команда для кнопки декремента скорости
        /// </summary>
        [RelayCommand]
        public void DecrementStepSpeed()
        {
            if(Settings.SpeedFactor >= 0.5f)
            {
                Settings.SpeedFactor -= 0.1f;
            }
        }

        [RelayCommand]
        public void GenerateField()
        {
            // Если игрок не задал зерно — генерируем новое и показываем
            if (Settings.Seed == 0)
            {
                Settings.Seed = new Random().Next(1, int.MaxValue - 1);
            }
            _dispatcher.InitializeRound(Settings, _canvasWidth, _canvasHeight);
            ScreenState = GameScreenState.Ready;
            RequestRedraw?.Invoke();
        }

        [RelayCommand]
        public void StartGame()
        {
            ScreenState = GameScreenState.Playing;
            _dispatcher.StartRound();
            _loop.Start();
        }

        public void PauseGame()
        {
            if (ScreenState != GameScreenState.Playing)
                return;

            //_loop.Stop();   // Останавливаем игровой цикл (НЕ ДЕЛАЕМ ЭТОГО В НЫНЕШНЕМ ВАРИАНТЕ!)
            _dispatcher.SwitchPause();
            ScreenState = GameScreenState.Paused;
            RequestRedraw?.Invoke();    // Перерисовать оверлей
        }

        public void ResumeGame()
        {
            if (ScreenState != GameScreenState.Paused)
                return;

            //_loop.Start();  // Возобновляем игровой цикл (ТОЖЕ УБРАТЬ!)
            _dispatcher.SwitchPause();
            ScreenState = GameScreenState.Playing;
            RequestRedraw?.Invoke();
        }

        [RelayCommand]
        public void ShowSetup()
        {
            _loop.Stop();
            Settings.Seed = 0; // Сбрасываем зерно при возврате в настройки
            ScreenState = GameScreenState.Setup;
            RequestRedraw?.Invoke();
        }

        //public void Update(float deltaTime)
        //{
        //    var endReason = _dispatcher.Round?.CurrentState?.EndReason;
        //    if(endReason != null && ScreenState == GameScreenState.Playing)
        //    {
        //        ScreenState = GameScreenState.GameOver;
        //        return;
        //    }

        //    _dispatcher.Update(deltaTime);
        //    RequestRedraw?.Invoke();
        //}

        public void UpdateCanvasSize(float w, float h)
        {
            _canvasWidth = w;
            _canvasHeight = h;
        }

        public void ChangeDirection(Direction dir)
        {
            _dispatcher.RefreshDirection(dir);
        }

        public void Render(SKCanvas canvas, float width, float height)
        {
            _dispatcher.Render(canvas, width, height);
        }

        public void TickRender()
        {
            RequestRedraw?.Invoke();
        }
    }
}
