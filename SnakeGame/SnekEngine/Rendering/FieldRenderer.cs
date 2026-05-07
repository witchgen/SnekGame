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

        public void Draw(SKCanvas canvas)
        {
            var paint = new SKPaint
            {
                Color = new SKColor(157, 157, 157, 120),
                StrokeWidth = 1,
                IsAntialias = false,
                Style = SKPaintStyle.Stroke
            };

            var path = new SKPath();

            // Вертикальные линии
            for (int i = 0; i <= _cols; i++)
            {
                float y = i * _cellSize;
                path.MoveTo(y, 0);
                path.LineTo(y, _rows * _cellSize);
            }

            // Горизонтальные линии
            for (int j = 0; j <= _rows; j++)
            {
                float x = j * _cellSize;
                path.MoveTo(0, x);
                path.LineTo(_cols * _cellSize, x);
            }

            canvas.DrawPath(path, paint);
        }
    }
}
