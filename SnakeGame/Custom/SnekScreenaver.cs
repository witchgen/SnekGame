using Android.Runtime;
using DrawnUi.Draw;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Storage;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SnakeGame.Custom
{
    public class SnekScreensaver : SkiaLayout
    {
        // ─── Настройки ───
        public float Speed { get; set; } = 35f;                                         // Скорость полёта (пикселей/сек)
        public float StarSize { get; set; } = 35f;                                      // Базовый размер "звезды"
        public int StarCount { get; set; } = 30;                                        // Количество звёзд
        public float Spread { get; set; } = 2f;                                         // Разброс направлений (>1 = шире конус)
        public SKColor StarColor { get; set; } = SKColors.White;                        // Цвет "звездочек" - элементов, рисуемых если не смогли загрузить картинки
        public SKColor BgColor { get; set; } = new SKColor(43, 56, 81);                 // Цвет заднего фона ("космос")
        public bool UseImages { get; set; } = true;                                    // true = SVG/PNG, false = круги
        public bool ShowFps { get; set; } = Preferences.Get("ShowMainMenuFPS", false);  // Флаг показа FPS

        // ─── Состояние ───
        private float _time;
        private IDispatcherTimer _timer;
        private DateTime _lastFrameTime;
        private List<Star> _stars = new();
        private Random _rnd = new(42);

        // ─── FPS ───
        private int _frameCount;
        private DateTime _fpsCheckTime = DateTime.UtcNow;
        private int _currentFps;

        // ─── Кэш для FPS-отрисовки ───
        private readonly SKPaint _fpsPaint = new()
        {
            Style = SKPaintStyle.Fill,
            Color = SKColors.Orange,
            IsAntialias = true
        };

        // ─── Кэш изображений ───
        private SKImage[] _images;
        private int _imageCount = 0;

        // ─── Данные звезды ───
        private class Star
        {
            public float Angle;      // Направление полёта (радианы)
            public float Distance;   // Текущее расстояние от центра
            public float Size;       // Базовый размер
            public float SpeedMult;  // Множитель скорости (индивидуальный)
            public int ImageIndex;   // Индекс картинки (-1 = круг)
            public float Rotation;   // Вращение картинки
            public float RotSpeed;   // Скорость вращения
        }

        public SnekScreensaver()
        {
            HorizontalOptions = LayoutOptions.Fill;
            VerticalOptions = LayoutOptions.Fill;
            UseCache = SkiaCacheType.ImageDoubleBuffered;
            LoadImages();

            InitStars();
            StartTimer();
        }

        private void DrawFps(SKCanvas canvas)
        {
            _frameCount++;
            var now = DateTime.UtcNow;
            var delta = (now - _fpsCheckTime).TotalSeconds;

            if (delta >= 1.0)
            {
                _currentFps = (int)(_frameCount / delta);
                _frameCount = 0;
                _fpsCheckTime = now;
            }

            string fpsText = $"FPS: {_currentFps}";
            float padding = 8;
            float textWidth = _fpsPaint.MeasureText(fpsText);
            float boxWidth = textWidth + padding * 2;
            float boxHeight = _fpsPaint.TextSize + padding * 2;
            var font = new SKFont(typeface: SKTypeface.Default, size: 40);

            var bgPaint = new SKPaint
            {
                Color = SKColors.Transparent,
                Style = SKPaintStyle.Stroke
            };

            // Фон под текст
            //canvas.DrawRect(10, 10, boxWidth, boxHeight, bgPaint);
            // Текст
            canvas.DrawText(fpsText, 10 + padding, 10 + boxHeight - padding / 2, font, _fpsPaint);
        }

        private void StartTimer()
        {
            _lastFrameTime = DateTime.UtcNow;
            _timer = Dispatcher.CreateTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(16);
            _timer.Tick += (s, e) =>
            {
                var now = DateTime.UtcNow;
                var dt = (float)(now - _lastFrameTime).TotalSeconds;
                _lastFrameTime = now;
                if (dt > 0.1f) dt = 0.1f;

                _time += dt;
                UpdateStars(dt);
                Update();
            };
            _timer.Start();
        }

        private void InitStars()
        {
            _stars.Clear();
            for (int i = 0; i < StarCount; i++)
            {
                SpawnStar(initial: true);
            }
        }

        private void SpawnStar(bool initial = false)
        {
            var star = new Star
            {
                Angle = (float)(_rnd.NextDouble() * Math.PI * 2),
                Distance = initial ? (float)(_rnd.NextDouble() * 1000) : 1f, // 1 = почти в центре
                Size = StarSize * (0.5f + (float)_rnd.NextDouble()),
                SpeedMult = 0.5f + (float)_rnd.NextDouble(),
                ImageIndex = UseImages && _images?.Length > 0 ? _rnd.Next(_images.Length) : -1,
                Rotation = (float)(_rnd.NextDouble() * 360),
                RotSpeed = (float)(_rnd.NextDouble() * 100 - 50)
            };
            _stars.Add(star);
        }

        private void UpdateStars(float dt)
        {
            float moveSpeed = Speed * dt;

            foreach (var star in _stars)
            {
                // Чем дальше, тем быстрее (эффект перспективы)
                star.Distance += moveSpeed * star.SpeedMult * (0.1f + star.Distance * 0.01f);
                star.Rotation += star.RotSpeed * dt;
            }

            // Удаляем улетевшие за экран, добавляем новые из центра
            _stars.RemoveAll(s => s.Distance > 2000);
            while (_stars.Count < StarCount)
            {
                SpawnStar();
            }
            //_stars.Sort((a, b) => b.Distance.CompareTo(a.Distance));
        }

        protected override void Paint(DrawingContext ctx)
        {
            var canvas = ctx.Context.Canvas;
            var width = ctx.Context.Width;
            var height = ctx.Context.Height;
            var cx = width / 2;
            var cy = height / 2;

            // Фон
            canvas.Clear(BgColor);

            // Рисуем от дальних к ближним (чтобы ближние перекрывали)
            foreach (var star in _stars.OrderByDescending(s => s.Distance))
            {
                // Позиция с учётом угла
                float x = cx + MathF.Cos(star.Angle) * star.Distance;
                float y = cy + MathF.Sin(star.Angle) * star.Distance * (height / width); // Коррекция аспекта

                // Масштаб: чем дальше, тем крупнее (эффект приближения)
                float scale = star.Distance * 0.01f;
                float drawSize = star.Size * scale;

                // Отсечение за экраном
                if (x < -drawSize || x > width + drawSize ||
                    y < -drawSize || y > height + drawSize)
                    continue;

                // Яркость: ближние ярче
                float brightness = Math.Clamp(scale / 5f, 0.3f, 1f);
                var color = StarColor.WithAlpha((byte)(255 * brightness));

                if (star.ImageIndex >= 0 && _images?[star.ImageIndex] != null)
                {
                    // Рисуем картинку
                    DrawImage(canvas, _images[star.ImageIndex], x, y, drawSize, star.Rotation, color);
                }
                else
                {
                    // Рисуем "звезду" — круг с бликом
                    DrawStar(canvas, x, y, drawSize, color);
                }
            }

            if(ShowFps)
            {
                DrawFps(canvas);
            }
        }

        private void DrawStar(SKCanvas canvas, float x, float y, float size, SKColor color)
        {
            // Основной круг
            using var paint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = color,
                IsAntialias = true
            };
            canvas.DrawCircle(x, y, size / 2, paint);

            // Блик (белый центр)
            if (size > 4)
            {
                using var glow = new SKPaint
                {
                    Style = SKPaintStyle.Fill,
                    Color = SKColors.White.WithAlpha((byte)(color.Alpha * 0.7f)),
                    IsAntialias = true
                };
                canvas.DrawCircle(x, y, size / 4, glow);
            }

            // Лучи для крупных
            if (size > 10)
            {
                using var rayPaint = new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    Color = color.WithAlpha((byte)(color.Alpha * 0.5f)),
                    StrokeWidth = 1,
                    IsAntialias = true
                };
                canvas.DrawLine(x - size, y, x + size, y, rayPaint);
                canvas.DrawLine(x, y - size, x, y + size, rayPaint);
            }
        }

        private void DrawImage(SKCanvas canvas, SKImage image, float x, float y, float size, float rotation, SKColor tint)
        {
            canvas.Save();
            canvas.Translate(x, y);
            canvas.RotateDegrees(rotation);
            canvas.Scale(size / image.Width);

            // Тонировка цветом
            using var paint = new SKPaint
            {
                ColorFilter = SKColorFilter.CreateBlendMode(tint, SKBlendMode.Modulate),
                IsAntialias = true
            };

            canvas.DrawImage(image, -image.Width / 2, -image.Height / 2, paint);
            canvas.Restore();
        }

        // ═══════════════════════════════════════════════════════════
        // ЗАГРУЗКА ИЗОБРАЖЕНИЙ
        // ═══════════════════════════════════════════════════════════

        public void LoadImages()
        {
            var asm = typeof(App).Assembly;

            var names = asm.GetManifestResourceNames()
                .Where(n => n.Contains("Screensaver") && n.EndsWith(".png"))
                .ToArray();

            var images = new List<SKImage>();
            foreach (var resourceName in names)
            {
                try
                {
                    using var stream = asm.GetManifestResourceStream(resourceName);
                    if (stream == null)
                        continue;

                    var image = SKImage.FromEncodedData(stream);
                    if (image != null)
                        images.Add(image);
                }
                catch { /* игнорируем битые файлы */ }
            }

            _images = images.ToArray();
            _imageCount = _images.Length;
            UseImages = _imageCount > 0;
        }

        //public void LoadSvgImages(params string[] svgPaths)
        //{
        //    var images = new List<SKImage>();
        //    foreach (var path in svgPaths)
        //    {
        //        try
        //        {
        //            var svg = SkiaSharp.Extended.Svg.SKSvg.CreateFromFile(path);
        //            if (svg?.Picture != null)
        //            {
        //                // Растеризуем SVG в bitmap
        //                var info = new SKImageInfo(128, 128);
        //                using var surface = SKSurface.Create(info);
        //                var canvas = surface.Canvas;
        //                canvas.Clear(SKColors.Transparent);
        //                canvas.DrawPicture(svg.Picture);
        //                images.Add(surface.Snapshot());
        //            }
        //        }
        //        catch { /* игнорируем */ }
        //    }

        //    _images = images.ToArray();
        //    _imageCount = _images.Length;
        //    UseImages = _imageCount > 0;
        //}
    }
}