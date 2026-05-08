using Android.App;
using SkiaSharp;
using SnakeGame.SnekEngine.Abstractions.Interfaces;
using SnakeGame.SnekEngine.Abstractions.Models;
using SnakeGame.SnekEngine.Rendering;
using System;

namespace SnakeGame.SnekEngine.Core.Services
{
    internal class GraphicRenderService : IGraphicRenderService
    {
        private GameRenderer _renderer;

        public void Configure(int rows, int cols, float canvasWidth, float canvasHeight)
        {
            float cellW = canvasWidth / cols;
            float cellH = canvasHeight / rows;

            float cellSize = Math.Min(cellW, cellH);

            _renderer = new GameRenderer(rows, cols, cellSize);
        }

        public void Render(SKCanvas canvas, GameSnapshot snapshot)
        {
            _renderer.Draw(canvas, snapshot);
        }
    }
}
