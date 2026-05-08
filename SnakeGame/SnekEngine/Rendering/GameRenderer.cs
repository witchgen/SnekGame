
using SkiaSharp;
using SnakeGame.SnekEngine.Abstractions.Models;
using System.Collections.Generic;

namespace SnakeGame.SnekEngine.Rendering
{
    internal class GameRenderer
    {
        private readonly FieldRenderer _fieldR;
        private readonly SnakeRenderer _snakeR;
        private readonly float _cellSize;
        private List<(int i, int j)> _wallsCache = new();

        public GameRenderer(int rows, int cols, float cellSize)
        {
            _cellSize = cellSize;
            _fieldR = new FieldRenderer(rows, cols, cellSize);
            _snakeR = new SnakeRenderer(_cellSize);
        }

        public void Draw(SKCanvas canvas, GameSnapshot snapshot)
        {
            _wallsCache.Clear();
            canvas.Clear(SKColors.Transparent);
            _fieldR.DrawField(canvas);
            _fieldR.DrawCells(canvas);
            CacheWalls(snapshot.Field);
            DrawWalls(canvas, snapshot.Field);
            DrawApple(canvas, snapshot.Apple);
            if (snapshot.Bombs != null)
                DrawBomb(canvas, snapshot.Bombs);

            _snakeR.DrawStatic(canvas, snapshot.CurrentSnake);
        }

        private void DrawWalls(SKCanvas canvas, int[,] field)
        {
            using var paint = new SKPaint
            {
                Color = new SKColor(80, 80, 80),
                IsAntialias = false,
                Style = SKPaintStyle.Fill
            };

            foreach (var wall in _wallsCache)
            {
                var path = new SKPath();
                path.AddRect(new SKRect(
                    wall.j * _cellSize,
                    wall.i * _cellSize,
                    (wall.j + 1) * _cellSize,
                    (wall.i + 1) * _cellSize
                ));

                canvas.DrawPath(path, paint);
            }
        }

        public void CacheWalls(int[,] field)
        {
            for (int i = 0; i < field.GetLength(0); i++)
            {
                for (int j = 0; j < field.GetLength(1); j++)
                {
                    if (field[i, j] == 1)
                        _wallsCache.Add((i, j));
                }
            }
        }

        private void DrawApple(SKCanvas canvas, (int i, int j) apple)
        {
            var paint = new SKPaint
            {
                Color = SKColors.IndianRed,
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };

            float cx = apple.j * _cellSize + _cellSize / 2;
            float cy = apple.i * _cellSize + _cellSize / 2;
            float radius = _cellSize * 0.35f;

            var path = new SKPath();
            path.AddCircle(cx, cy, radius);

            canvas.DrawPath(path, paint);
        }

        private void DrawBomb(SKCanvas canvas, HashSet<(int i, int j)> bombs)
        {
            using var paint = new SKPaint
            {
                Color = SKColors.Black,
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };

            foreach(var bomb in bombs)
            {
                float cx = bomb.j * _cellSize + _cellSize / 2;
                float cy = bomb.i * _cellSize + _cellSize / 2;
                float r = _cellSize * 0.35f;

                var path = new SKPath();
                path.AddCircle(cx, cy, r);

                canvas.DrawPath(path, paint);
            }
        }
    }
}
