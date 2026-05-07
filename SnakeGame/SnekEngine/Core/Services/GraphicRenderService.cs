using Android.App;
using SkiaSharp;
using SnakeGame.SnekEngine.Abstractions.Interfaces;
using SnakeGame.SnekEngine.Abstractions.Models;
using SnakeGame.SnekEngine.Rendering;

namespace SnakeGame.SnekEngine.Core.Services
{
    internal class GraphicRenderService : IGraphicRenderService
    {
        private readonly GameRenderer _renderer;

        public GraphicRenderService(GameRenderer render)
        {
            _renderer = render;
        }

        public void Render(SKCanvas canvas, GameSnapshot snapshot)
        {
            _renderer.Draw(canvas, snapshot);
        }
    }
}
