using SkiaSharp;
using SnakeGame.SnekEngine.Abstractions.Models;
using System.Collections.Generic;

namespace SnakeGame.SnekEngine.Rendering
{
    /// <summary>
    /// Рендерер для змеи и только змеи
    /// Рисует в двух вариантах - статика (стартовая позиция, экран инициализации) и динамика (геймплей, в движении)
    /// </summary>
    internal class SnakeRenderer
    {
        private readonly float _cellSize;
        private readonly float _margin;
        private readonly float _radius;
        private readonly List<SKPoint> _renderPath = new();

        public SnakeRenderer(float cellSize)
        {
            _cellSize = cellSize;
            _margin = cellSize * 0.15f;
            _radius = (cellSize - 2 * _margin) / 2f; // толщина змеи
        }

        public void Draw(SKCanvas canvas,
                 Snake prevSnake,
                 Snake currSnake,
                 float t)
        {
            var prevHead = prevSnake.Body.First.Value;
            var currHead = currSnake.Body.First.Value;

            DrawSnake(canvas, prevSnake, currSnake, prevHead, currHead, t);
        }

        /// <summary>
        /// Статичная "змея" (рисуется на месте обычной после генерации поля, но перед началом раунда)
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="snake"></param>
        public void DrawStatic(SKCanvas canvas, Snake snake)
        {
            var bodyPaint = new SKPaint
            {
                Color = new SKColor(0, 123, 0, 255),
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };

            var headPaint = new SKPaint
            {
                Color = new SKColor(0, 123, 0, 255),
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };

            bool isHead = true;

            foreach (var segment in snake.Body)
            {
                var paint = isHead ? headPaint : bodyPaint;
                isHead = false;

                float cx = segment.j * _cellSize + _cellSize / 2;
                float cy = segment.i * _cellSize + _cellSize / 2;
                float radius = _cellSize * 0.35f;

                var path = new SKPath();
                path.AddCircle(cx, cy, radius);

                canvas.DrawPath(path, paint);
            }
        }

        /// <summary>
        /// Отрисовка статичной змеи во время паузы (с сохранением длины)
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="snake"></param>
        public void DrawStaticPause(SKCanvas canvas, Snake snake)
        {
            float targetLength = (snake.Body.Count - 1) * _cellSize;
            var trimmedSnake = TrimPath(_renderPath, targetLength);
            DrawTube(canvas, trimmedSnake);
        }

        /// <summary>
        /// Рисуем змею по ходу движения: просчитываем точки, опираясь на разницу между старой / новой позицией и рисуем линию по этим точкам
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="prev"></param>
        /// <param name="curr"></param>
        /// <param name="prevHead"></param>
        /// <param name="nextHead"></param>
        /// <param name="t"></param>
        public void DrawSnake(SKCanvas canvas, 
                        Snake prev, Snake curr,
                        Snake.SnakeSegment prevHead,
                        Snake.SnakeSegment nextHead,
                        float t)
        {
            //
            // 1. Обновляем renderPath на ТИКЕ
            //
            if (_renderPath.Count == 0)
            {
                // Инициализация пути — вся змея по центрам клеток curr
                foreach (var seg in curr.Body)
                    _renderPath.Add(CellCenter(seg.i, seg.j));
            }

            // Если голова перешла в новую клетку — добавляем новую точку пути
            var currHeadPos = CellCenter(nextHead.i, nextHead.j);
            if (_renderPath.Count == 0 || _renderPath[0] != currHeadPos)
                _renderPath.Insert(0, currHeadPos);


            //
            // 2. Интерполируем голову между prev > curr
            //
            var prevHeadPos = CellCenter(prevHead.i, prevHead.j);
            var headPos = Lerp(prevHeadPos, currHeadPos, t);

            // Обновляем первую точку пути (голову)
            _renderPath[0] = headPos;


            //
            // 3. Обрезаем путь по длине змеи
            //
            float targetLength = (curr.Body.Count - 1) * _cellSize;
            var trimmed = TrimPath(_renderPath, targetLength);


            //
            // 4. Рисуем трубку
            //
            DrawTube(canvas, trimmed);
        }

        /// <summary>
        /// Обрезаем линию по длине змеи
        /// </summary>
        /// <param name="path"></param>
        /// <param name="targetLength"></param>
        /// <returns></returns>
        private List<SKPoint> TrimPath(List<SKPoint> path, float targetLength)
        {
            float length = 0f;
            var result = new List<SKPoint>();
            result.Add(path[0]);

            for (int i = 1; i < path.Count; i++)
            {
                var a = path[i - 1];
                var b = path[i];
                float seg = SKPoint.Distance(a, b);

                if (length + seg >= targetLength)
                {
                    float remain = targetLength - length;
                    float t = remain / seg;
                    result.Add(Lerp(a, b, t));
                    return result;
                }

                result.Add(b);
                length += seg;
            }

            return result;
        }

        private void DrawTube(SKCanvas canvas, List<SKPoint> pts)
        {
            if (pts.Count < 2) return;

            using var paint = new SKPaint
            {
                Color = new SKColor(0, 123, 0),
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeCap = SKStrokeCap.Round,
                StrokeJoin = SKStrokeJoin.Round,
                StrokeWidth = _radius * 2f
            };

            using var path = new SKPath();
            path.MoveTo(pts[0]);

            for (int i = 1; i < pts.Count; i++)
                path.LineTo(pts[i]);

            canvas.DrawPath(path, paint);
        }

        private static SKPoint Lerp(SKPoint a, SKPoint b, float t)
            => new SKPoint(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t);

        private SKPoint CellCenter(int i, int j)
        {
            return new SKPoint(
                j * _cellSize + _cellSize / 2f,
                i * _cellSize + _cellSize / 2f
            );
        }
    }
}
