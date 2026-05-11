using SkiaSharp;
using SnakeGame.SnekEngine.Abstractions.Models;
using System;
using static SnakeGame.SnekEngine.Abstractions.GameEnums;

namespace SnakeGame.SnekEngine.Rendering
{
    /// <summary>
    /// "Рисователь" для экрана конца игры. Выводит информацию о набраных очках, причинах конца игры и зовет игрока по имени (если указано)
    /// </summary>
    internal class ResultsRenderer
    {
        private static int _score = 0;
        private static string _playerName = string.Empty;
        private GameOverReason _endReason;

        public ResultsRenderer(PlayInfo results)
        {
            _score = results.FinalScore;
            _playerName = results.PlayerName;
            _endReason = results.DeathReason;
        }

        public void RenderEndScreen(SKCanvas canvas, float width, float height)
        {
            var bounds = canvas.DeviceClipBounds;

            //var endSnapshot = canvas.Surface.Snapshot();
            //var grayscale = new SKPaint();
            //grayscale.ColorFilter = SKColorFilter.CreateColorMatrix(new float[]
            //{
            //    0.21f, 0.72f, 0.07f, 0, 0, // Red channel
            //    0.21f, 0.72f, 0.07f, 0, 0, // Green channel
            //    0.21f, 0.72f, 0.07f, 0, 0, // Blue channel
            //    0,     0,     0,     1, 0  // Alpha channel
            //});
            //canvas.Clear(SKColors.Gray);
            //canvas.DrawBitmap(SKBitmap.FromImage(endSnapshot), 0, 0, grayscale);

            // Простой серый оверлей
            using var overlay = new SKPaint
            {
                Color = new SKColor(0, 0, 0, 110)
            };
            canvas.DrawRect(0, 0, width, height, overlay);

            //var overlay = new SKPaint { Color = new SKColor(0, 0, 0, 180) };
            //canvas.DrawRect(bounds, overlay);
            // текст
            string endText =
                $"WASTED\n" +
                $"Ты набрал: {_score} очков\n" +
                $"Спасибо за игру, {_playerName}";

            // Шрифт и стиль
            using var font = new SKFont(SKTypeface.FromFamilyName(null, SKFontStyle.Bold), 64);
            using var textPaint = new SKPaint
            {
                Color = SKColors.DarkRed,
                IsAntialias = true
            };

            // Разбиваем текст на строки
            var lines = endText.Split('\n');
            float lineHeight = font.Size * 1.2f; // Межстрочный интервал

            // Вычисляем общую высоту и максимальную ширину
            float totalHeight = lines.Length * lineHeight;
            float maxWidth = 0;
            foreach (var line in lines)
            {
                font.MeasureText(line, out SKRect textBounds);
                maxWidth = Math.Max(maxWidth, textBounds.Width);
            }

            // Центрирование
            float startX = (width - maxWidth) / 2f;
            float startY = (height - totalHeight) / 2f;

            // Рисуем каждую строку
            for (int i = 0; i < lines.Length; i++)
            {
                float lineY = startY + (i * lineHeight) + font.Size; // +font.Size для базовой линии
                font.MeasureText(lines[i], out SKRect lineBounds);
                float lineX = (width - lineBounds.Width) / 2f; // Центрирование каждой строки

                canvas.DrawText(lines[i], lineX, lineY, SKTextAlign.Left, font, textPaint);
            }
        }
    }
}
