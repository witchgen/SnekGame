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

        /// <summary>
        /// ИСПРАВЛЕННЫЙ МЕТОД!
        /// width/height — размеры игрового поля (не Canvas!)
        /// cellSize — для масштабирования элементов относительно клеток
        /// </summary>
        public void RenderEndScreen(SKCanvas canvas, float width, float height)
        {
            // === ОВЕРЛЕЙ ТОЧНО ПО РАЗМЕРУ ПОЛЯ ===
            using var overlay = new SKPaint
            {
                Color = new SKColor(0, 0, 0, 110)
            };
            canvas.DrawRect(0, 0, width, height, overlay);

            // === ТЕКСТ ПРОПОРЦИОНАЛЕН РАЗМЕРУ ПОЛЯ ===
            string endText = _endReason switch
            {
                GameOverReason.BitTail => "WASTED\nУкусил сам себя!",
                GameOverReason.Wall => "WASTED\nНе справился с управлением!",
                GameOverReason.Bomb => "BOOM!\nБомба взорвалась!",
                _ => "WASTED"
            };

            // Размер шрифта: пропорционален cellSize и размеру поля
            float baseFontSize = Math.Min(width, height) * 0.08f; // 8% от меньшей стороны
            float titleFontSize = baseFontSize * 1.0f;
            float scoreFontSize = baseFontSize * 0.9f;
            float nameFontSize = baseFontSize * 1.1f;

            // Центрирование
            float centerX = width / 2f;
            float centerY = height / 2f;

            // Рисуем заголовок "WASTED"
            using var titleFont = new SKFont(SKTypeface.FromFamilyName(null, SKFontStyle.Bold), titleFontSize);
            using var titlePaint = new SKPaint
            {
                Color = SKColors.DarkRed,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center
            };

            float titleY = centerY - height * 0.15f;
            canvas.DrawText("WASTED", centerX, titleY, titleFont, titlePaint);

            // Рисуем причину смерти
            using var reasonFont = new SKFont(SKTypeface.FromFamilyName(null, SKFontStyle.Bold), scoreFontSize);
            using var reasonPaint = new SKPaint
            {
                Color = SKColors.DarkRed,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center
            };
            float reasonY = centerY - height * 0.02f;
            canvas.DrawText(endText.Replace("WASTED\n", ""), centerX, reasonY, reasonFont, reasonPaint);

            // Рисуем счёт
            using var scoreFont = new SKFont(SKTypeface.FromFamilyName(null, SKFontStyle.Bold), scoreFontSize);
            using var scorePaint = new SKPaint
            {
                Color = SKColors.Gold,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center
            };

            float scoreY = centerY + height * 0.12f;
            canvas.DrawText($"Очки: {_score}", centerX, scoreY, scoreFont, scorePaint);

            // Рисуем имя игрока
            using var nameFont = new SKFont(SKTypeface.FromFamilyName(null, SKFontStyle.Normal), nameFontSize);
            using var namePaint = new SKPaint
            {
                Color = SKColors.PaleGoldenrod,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center
            };

            float nameY = centerY + height * 0.25f;
            canvas.DrawText($"Спасибо за игру, {_playerName}", centerX, nameY, nameFont, namePaint);
        }

        [Obsolete]
        public void RenderEndScreen(SKCanvas canvas, float proto_width, float proto_height, bool isObsolete)
        {
            var bounds = canvas.DeviceClipBounds;
            var width = bounds.Width;
            var height = bounds.Height;

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
