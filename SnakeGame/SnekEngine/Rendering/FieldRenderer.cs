using Android.Telecom;
using Javax.Net.Ssl;
using SkiaSharp;

namespace SnakeGame.SnekEngine.Rendering
{
    internal class FieldRenderer
    {
        private readonly int _rows;
        private readonly int _cols;
        private readonly float _cellSize;

        public FieldRenderer(int rows, int cols, float cellSize)
        {
            _rows = rows;
            _cols = cols;
            _cellSize = cellSize;
        }

        public void DrawFieldCells(SKCanvas canvas, SKImage cell)
        {
            using var paint = new SKPaint
            {
                IsAntialias = false
            };

            for(int i = 0; i < _rows; i++)
            {
                for(int j = 0; j < _cols; j++)
                {
                    var rect = new SKRect(
                        j * _cellSize,
                        i * _cellSize,
                        (j + 1) * _cellSize,
                        (i + 1) * _cellSize
                    );

                    canvas.DrawImage(cell, rect, paint);
                }
            }
        }

        public void DrawCells(SKCanvas canvas)
        {
            var paint = new SKPaint
            {
                Color = SKColors.DarkGray /*new SKColor(157, 157, 157, 255)*/,
                StrokeWidth = 1,
                IsAntialias = false,
                Style = SKPaintStyle.Stroke
            };

            var path = new SKPath();

            // Вертикальные линии
            for (int i = 1; i <= _cols; i++)
            {
                float y = i * _cellSize;
                path.MoveTo(y, 0);
                path.LineTo(y, _rows * _cellSize);
            }

            // Горизонтальные линии
            for (int j = 1; j <= _rows; j++)
            {
                float x = j * _cellSize;
                path.MoveTo(0, x);
                path.LineTo(_cols * _cellSize, x);
            }

            canvas.DrawPath(path, paint);
        }

        public void DrawField(SKCanvas canvas)
        {
            var paint = new SKPaint
            {
                Color = new SKColor(92, 159, 61, 255),
                StrokeWidth = 1,
                IsAntialias = false,
                Style = SKPaintStyle.Fill
            };

            var path = new SKPath();
            canvas.DrawRect(0, 0, _cols * _cellSize, _rows * _cellSize, paint);
        }
    }
}
