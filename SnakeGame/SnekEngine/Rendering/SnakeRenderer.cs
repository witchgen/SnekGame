using SkiaSharp;
using SnakeGame.SnekEngine.Abstractions.Models;

namespace SnakeGame.SnekEngine.Rendering
{
    internal class SnakeRenderer
    {
        private readonly float _cellSize;

        public SnakeRenderer(float cellSize)
        {
            _cellSize = cellSize;
        }

        public void DrawStatic(SKCanvas canvas, Snake snake)
        {
            var bodyPaint = new SKPaint
            {
                Color = new SKColor(0, 200, 0, 255),
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };

            var headPaint = new SKPaint
            {
                Color = new SKColor(0, 255, 0, 255),
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };

            bool isHead = true;

            foreach (var segment in snake.Body)
            {
                var paint = isHead ? headPaint : bodyPaint;
                isHead = false;

                var path = new SKPath();
                path.AddRect(new SKRect(
                    segment.j * _cellSize,
                    segment.i * _cellSize,
                    (segment.j + 1) * _cellSize,
                    (segment.i + 1) * _cellSize
                ));

                canvas.DrawPath(path, paint);
            }
        }
    }
}
