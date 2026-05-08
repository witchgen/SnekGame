using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkiaSharp;
using SnakeGame.SnekEngine;
using SnakeGame.SnekEngine.Abstractions.Models;
using System;

namespace SnakeGame.ViewModels
{
    public partial class GameViewModel : ObservableObject
    {
        private readonly GameDispatcher _dispatcher;

        public event Action? RequestRedraw;
        private float _canvasWidth;
        private float _canvasHeight;

        [ObservableProperty]
        private InitialSettings _settings = new()
        {
            Rows = 20,
            Cols = 20,
            SnakeSpawnPoint = (10, 10),
            BombsCount = 1,
            CustomWalls = false
        };

        public GameViewModel(GameDispatcher dispatcher)
        {
            _dispatcher = dispatcher;
            RequestRedraw?.Invoke();
        }

        [RelayCommand]
        public void GenerateField()
        {
            _dispatcher.StartRound(Settings, _canvasWidth, _canvasHeight);
            RequestRedraw?.Invoke();
        }

        public void UpdateCanvasSize(float w, float h)
        {
            _canvasWidth = w;
            _canvasHeight = h;
        }

        public void Render(SKCanvas canvas)
        {
            _dispatcher.Render(canvas);
        }
    }
}
