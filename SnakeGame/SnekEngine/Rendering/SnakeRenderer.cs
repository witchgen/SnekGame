using SkiaSharp;
using SnakeGame.SnekEngine.Abstractions.Models;
using System.Collections.Generic;

namespace SnakeGame.SnekEngine.Rendering
{
    internal class SnakeRenderer
    {
        private readonly float _cellSize;
        private readonly float _margin;
        private readonly float _radius;

        public SnakeRenderer(float cellSize)
        {
            _cellSize = cellSize;
            _margin = cellSize * 0.15f;
            _radius = (cellSize - 2 * _margin) / 2f; // толщина змеи
        }

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

        public void DrawSnake(SKCanvas canvas, Snake snake,
                      (int i, int j) prevHead,
                      (int i, int j) nextHead,
                      float t)
        {
            // 1. Мировые точки тела
            var pts = new List<SKPoint>();

            // голова — интерполированная
            var headPrev = CellCenter(prevHead.i, prevHead.j);
            var headNext = CellCenter(nextHead.i, nextHead.j);
            pts.Add(Lerp(headPrev, headNext, t));

            // остальные сегменты — центры клеток
            bool skipHead = true;
            foreach (var seg in snake.Body)
            {
                if (skipHead) { skipHead = false; continue; }
                pts.Add(CellCenter(seg.i, seg.j));
            }

            // 2. Обрезаем хвост
            float targetLength = (snake.Body.Count - 1) * _cellSize;
            pts = TrimTail(pts, targetLength);

            // 3. Рисуем трубку
            DrawTube(canvas, pts);
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

        private List<SKPoint> TrimTail(List<SKPoint> pts, float targetLength)
        {
            float length = 0f;
            var result = new List<SKPoint>();
            result.Add(pts[0]);

            for (int k = 1; k < pts.Count; k++)
            {
                var a = pts[k - 1];
                var b = pts[k];
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

        private SKPoint Lerp(SKPoint a, SKPoint b, float t)
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
