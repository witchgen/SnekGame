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
        public event Action<GameOverReason>? GameEnded; // Событие завершения раунда, подхватываем в модели представления для корректной регистрации

        private float _accumulated;
        private float _baseTickDuration = 0.2f; // базовый тик, 200 мс
        private float _tickDuration = 0.2f;     // текущий тик с учётом сложности
        private float _speedFactor = 1.0f;      // множитель сложности

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

            _curr = _game.InitializeLevel(setup);
            Round = new PlayInfo
            {
                PlayerName = Preferences.Get("PlayerName", "анонимус"),
                Seed = setup.Seed,
                CurrentState = _curr
            };
            _prev = _curr.Clone();
            _directionBuffer = setup.FirstDirection;
            SetSpeedFactor(setup.SpeedFactor);

            _playStatus = GameStatus.Initialized;
        }

        /// <summary>
        /// Устанавливаем множитель скорости игры (сложность).
        /// 1.0 = базовая скорость, >1.0 = быстрее, <1.0 = медленнее.
        /// </summary>
        public void SetSpeedFactor(float factor)
        {
            // Ограничим разумный диапазон
            _speedFactor = Math.Clamp(factor, 0.25f, 5.0f);

            // Итоговая длительность тика: базовая / скорость
            _tickDuration = _baseTickDuration / _speedFactor;
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
            //deltaTime = Math.Min(deltaTime, 0.05f); // max 50ms за кадр

            // Кэп на случай лагов, чтобы не улететь в безумие
            deltaTime = Math.Min(deltaTime, 0.1f); // максимум 100 мс за кадр
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

                    GameEnded?.Invoke(_curr.EndReason.Value);
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
            // Не позволяем завернуть "в себя" + проверяем, что направление не заблокировано препятствием
            if(direction != _directionBuffer.ToOpposite() && _curr.AvailableDirections.Contains(direction))
                _directionBuffer = direction;
        }

        /// <summary>
        /// Рендер с интерполяцией между _prev и _curr.
        /// t = _accumulated / _tickDuration ∈ [0..1]
        /// </summary>
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
                        //double eased = t ; // smoothstep
                        float t = 0f;
                        if (_tickDuration > 0)
                            t = Math.Clamp(_accumulated / _tickDuration, 0f, 1f);
                        _graphics.Render(canvas, _prev!, _curr, t);

                        break;
                    }

                case GameStatus.Ended:
                    _graphics.RenderResults(canvas, width, height, Round);
                    break;
            }
        }
    }
}
