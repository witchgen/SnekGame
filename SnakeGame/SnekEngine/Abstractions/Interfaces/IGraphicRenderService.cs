using SkiaSharp;
using SnakeGame.SnekEngine.Abstractions.Models;

namespace SnakeGame.SnekEngine.Abstractions.Interfaces
{
    public interface IGraphicRenderService
    {
        /// <summary>
        /// Рендерим игровой визуал
        /// </summary>
        /// <param name="snapshot">Актуальный снимок текущей игры</param>
        void Render(SKCanvas canvas, GameSnapshot snapshot);
    }
}
