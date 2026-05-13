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
        /// Первый статичный кадр - рисуем инициализированное поле и ждем начала игры
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="initial">Стартовое состояние раунда</param>
        /// <param name="isPaused">Флаг паузы для корректной отрисовки змеи</param>
        void RenderStatic(SKCanvas canvas, GameSnapshot initial, bool isPaused);
        /// <summary>
        /// Рендерим игровой визуал
        /// </summary>
        /// <param name="previous">Предыдущий снимок игры</param>
        /// <param name="current">Актуальный снимок текущей игры</param>
        /// <param name="t">Интерполяция</param>
        void Render(SKCanvas canvas, GameSnapshot previous, GameSnapshot current, float t);
        /// <summary>
        /// Показать оверлей паузы
        /// </summary>
        /// <param name="canvas"></param>
        void RenderPauseOverlay(SKCanvas canvas, float width, float height);
        /// <summary>
        /// Рендер экрана геймовера с результатами
        /// </summary>
        /// <param name="result"></param>
        void RenderResults(SKCanvas canvas, float width, float height, PlayInfo result);
    }
}
