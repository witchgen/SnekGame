using SkiaSharp;
using SnakeGame.SnekEngine.Abstractions.Interfaces;
using SnakeGame.SnekEngine.Abstractions.Models;
using SnakeGame.SnekEngine.Custom;
using System.Threading.Tasks;
using static SnakeGame.SnekEngine.Abstractions.GameEnums;

namespace SnakeGame.SnekEngine
{
    public class GameDispatcher
    {
        private readonly IGameplayService _game;
        private readonly IGraphicRenderService _graphics;
        private GameStatus _roundStatus = GameStatus.Idle;
        private Direction _directionBuffer;

        private PlayInfo _round = new();

        public GameDispatcher(IGameplayService gameplay,
            IGraphicRenderService graphics)
        {
            _game = gameplay;
            _graphics = graphics;
        }

        public void InitializeRound(InitialSettings setup, float canvasW, float canvasH)
        {
            _graphics.Configure(setup.Rows, setup.Cols, canvasW, canvasH);
            _round.CurrentState = _game.InitializeLevel(setup);
            _directionBuffer = setup.FirstDirection;
            _roundStatus = GameStatus.Initialized;
        }

        public void Update()
        {
            GameStatus result;
            if (_roundStatus == GameStatus.Initialized)
            {
                _roundStatus = GameStatus.Running;
            }
            if (_roundStatus != GameStatus.Running)
            {
                return; // Обрабатывать паузу
            }
            _roundStatus = _game.Tick(_roundStatus, _directionBuffer);

            if (_roundStatus == GameStatus.Ended)
                RenderEndgameScreen();
        }

        public void ChangeDirection(Direction direction)
        {
            if(direction != direction.ToOpposite())
                _directionBuffer = direction;
        }

        public void Render(SKCanvas canvas)
        {
            if(_round.CurrentState != null)
            {
                _graphics.Render(canvas, _round.CurrentState);
            }
        }

        public void RenderEndgameScreen()
        {
            _round.CurrentState = _game.GetStateSnapshot();
            _graphics.RenderResults();
        }
    }
}
