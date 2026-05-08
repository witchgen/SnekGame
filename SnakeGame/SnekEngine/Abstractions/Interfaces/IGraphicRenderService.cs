using SkiaSharp;
using SnakeGame.SnekEngine.Abstractions.Models;

namespace SnakeGame.SnekEngine.Abstractions.Interfaces
{
    public interface IGraphicRenderService
    {
        /// <summary>
        /// Начальная конфигурация рендеринга (для поля)
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="cols"></param>
        void Configure(int rows, int cols, float canvasWidth, float canvasHeight);
        /// <summary>
        /// Рендерим игровой визуал
        /// </summary>
        /// <param name="snapshot">Актуальный снимок текущей игры</param>
        void Render(SKCanvas canvas, GameSnapshot snapshot);
    }
}
