using SkiaSharp;
using SnakeGame.SnekEngine.Abstractions.Interfaces;
using SnakeGame.SnekEngine.Abstractions.Models;

namespace SnakeGame.SnekEngine
{
    public class GameDispatcher
    {
        private readonly IGameplayService _game;
        private readonly IGraphicRenderService _graphics;

        private PlayInfo _round = new();

        public GameDispatcher(IGameplayService gameplay,
            IGraphicRenderService graphics)
        {
            _game = gameplay;
            _graphics = graphics;
        }

        public void StartRound(InitialSettings setup, float canvasW, float canvasH)
        {
            _graphics.Configure(setup.Rows, setup.Cols, canvasW, canvasH);
            _round.CurrentState = _game.InitializeLevel(setup);
        }

        public void Render(SKCanvas canvas)
        {
            if(_round.CurrentState != null)
            {
                _graphics.Render(canvas, _round.CurrentState);
            }
        }
    }
}
