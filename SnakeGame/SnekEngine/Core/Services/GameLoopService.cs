using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using System;
using System.Diagnostics;

namespace SnakeGame.SnekEngine.Core.Services
{
    public class GameLoopService : IDisposable
    {
        private readonly GameDispatcher _dispatcher;
        private readonly IDispatcherTimer _timer;
        private readonly Stopwatch _sw = new();

        private long _lastMs;

        public event Action? TickCompleted;

        public GameLoopService(GameDispatcher dispatcher)
        {
            _dispatcher = dispatcher;

            _timer = Application.Current.Dispatcher.CreateTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(16); // 60 FPS логики
            _timer.Tick += OnTick;
        }

        private void OnTick(object? sender, EventArgs e)
        {
            if (!_sw.IsRunning)
            {
                _sw.Start();
                _lastMs = _sw.ElapsedMilliseconds;
                return;
            }

            long now = _sw.ElapsedMilliseconds;
            float delta = (now - _lastMs) / 1000f;
            _lastMs = now;

            _dispatcher.Update(delta); // шаг по дельте времени
            TickCompleted?.Invoke();    // сигнал на перерисовку
        }

        public void Start()
        {
            _sw.Restart();
            _lastMs = _sw.ElapsedMilliseconds;
            _timer.Start();
        }
        public void Stop()
        {
            _timer.Stop();
            _sw.Stop();
        }
        public void Dispose()
        {
            _timer.Stop();
            _sw.Stop();
        }
    }
}
