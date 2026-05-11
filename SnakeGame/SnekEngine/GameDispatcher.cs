using Microsoft.Maui.Storage;
using SkiaSharp;
using SnakeGame.SnekEngine.Abstractions.Interfaces;
using SnakeGame.SnekEngine.Abstractions.Models;
using SnakeGame.SnekEngine.Custom;
using System;
using static SnakeGame.SnekEngine.Abstractions.GameEnums;

namespace SnakeGame.SnekEngine
{
    public class GameDispatcher
    {
        // СЕРВИСЫ:
        private readonly IGameplayService _game;
        private readonly IGraphicRenderService _graphics;

        // СОСТОЯНИЕ ИГРОВОГО ПРОЦЕССА:
        private GameStatus _playStatus = GameStatus.Idle;
        private Direction _directionBuffer;

        // СНИМКИ СОСТОЯНИЯ:
        public PlayInfo Round { get; private set; }
        private GameSnapshot _prev;
        private GameSnapshot _curr;

        private float _accumulated;
        private float _tickDuration = 0.3f; // Логический тик в 200 мс

        public GameDispatcher(IGameplayService gameplay,
            IGraphicRenderService graphics)
        {
            _game = gameplay;
            _graphics = graphics;
        }

        /// <summary>
        /// Инициализируем поле и позиции объектов по начальным настройкам
        /// </summary>
        /// <param name="setup">Переданные параметры новой игры</param>
        /// <param name="canvasW">Ширина холста</param>
        /// <param name="canvasH">Высота холста</param>
        public void InitializeRound(InitialSettings setup, float canvasW, float canvasH)
        {
            _graphics.Configure(setup.Rows, setup.Cols, canvasW, canvasH);

            //var newSeed = setup.Seed == 0 ? new Random().Next(1, Int32.MaxValue - 1) : setup.Seed;

            //setup.Seed = newSeed;

            _curr = _game.InitializeLevel(setup);
            Round = new PlayInfo
            {
                PlayerName = Preferences.Get("PlayerName", "анонимус"),
                Seed = setup.Seed,
                CurrentState = _curr
            };
            _prev = _curr;
            _directionBuffer = setup.FirstDirection;

            _playStatus = GameStatus.Initialized;
        }

        public void StartRound()
        {
            if (_playStatus == GameStatus.Initialized)
            {
                _accumulated = 0f;
                _playStatus = GameStatus.Running;
            }

            if (_playStatus == GameStatus.Initialized)
                _playStatus = GameStatus.Running;
        }

        /// <summary>
        /// Обновление "тика" игровой логики
        /// </summary>
        /// <param name="deltaTime"></param>
        public void Update(float deltaTime)
        {
            if (_playStatus != GameStatus.Running || _curr == null)
                return;

            // Капируем delta — при лаге не прыгаем далеко
            deltaTime = Math.Min(deltaTime, 0.05f); // max 50ms за кадр
            _accumulated += deltaTime;

            var dir = _directionBuffer;

            if (_accumulated >= _tickDuration)
            {
                _accumulated -= _tickDuration;

                // Если всё ещё >= _tickDuration — сбросим, иначе змейка скачет
                if (_accumulated >= _tickDuration)
                    _accumulated = 0;

                _prev = _curr.Clone();
                _curr = _game.Tick(_curr, dir);

                if (_curr.EndReason != null)
                {
                    Round.CurrentState = _curr;
                    Round.FinalScore = _curr.Score;
                    Round.DtEnded = DateTime.UtcNow;
                    _playStatus = GameStatus.Ended;
                    return;
                }
            }
        }

        /// <summary>
        /// Запись в буфер переданной игроком команды управления
        /// </summary>
        /// <param name="direction"></param>
        public void RefreshDirection(Direction direction)
        {
            // Не позволяем завернуть "в себя"
            if(direction != _directionBuffer.ToOpposite())
                _directionBuffer = direction;
        }

        /// <summary>
        /// Рисуем контент игрового экрана в зависимости от состояния
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void Render(SKCanvas canvas, float width, float height)
        {
            if (_curr == null)
                return;

            switch (_playStatus)
            {
                case GameStatus.Idle:
                    // фон сюда
                    break;

                case GameStatus.Initialized:
                    // поле + статичная змея
                    _graphics.RenderStatic(canvas, _curr);
                    break;

                case GameStatus.Running:
                    {
                        //double t = Math.Clamp(_accumulated / _tickDuration, 0f, 1f);
                        //var speedCurve = 12.5d; // difficulty?
                        //t = Math.Pow(t, speedCurve);
                        //_graphics.Render(canvas, _prev!, _curr, (float)t);
                        //break;

                        //double t = Math.Clamp(_accumulated / _tickDuration, 0f, 1f);

                        //// speed ∈ [1..5]
                        //double speed = 2.0; // например

                        //// интерполяция с ускорением
                        //double eased = Math.Pow(t, 1.0 / speed);

                        //_graphics.Render(canvas, _prev!, _curr, (float)eased);
                        //break;

                        // Чем выше уровень — тем быстрее (меньше duration)
                        //_tickDuration = Math.Max(0.05f, 0.3f - (2 * 0.05f)); // 1 - 5 * 0.05f

                        double t = Math.Clamp(_accumulated / _tickDuration, 0f, 1f);

                        //double eased = Math.Pow(t, 1.0 / _visualSmoothness); // всегда плавно
                        double eased = t ; // smoothstep
                        _graphics.Render(canvas, _prev!, _curr, (float)eased);

                        System.Diagnostics.Debug.WriteLine($"accum={_accumulated:F4} t={t:F4} eased={eased:F4}");

                        break;
                    }

                case GameStatus.Ended:
                    _graphics.RenderResults(canvas, width, height, Round);
                    break;
            }
        }
    }
}
