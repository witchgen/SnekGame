using SkiaSharp;
using SnakeGame.SnekEngine.Abstractions.Models;
using System;
using System.Collections.Generic;
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
        /// Отрисовка экрана геймовера с результатами раунда
        /// </summary>
        public void RenderEndScreen(SKCanvas canvas, float width, float height)
        {
            // Оверлей по размеру поля
            using var overlay = new SKPaint
            {
                Color = new SKColor(0, 0, 0, 110)
            };
            canvas.DrawRect(0, 0, width, height, overlay);

            // Текст нашей безвременной кончины:
            string endText = _endReason switch
            {
                GameOverReason.BitTail => "Укусил сам себя!",
                GameOverReason.Wall => "Не справился с управлением!",
                GameOverReason.Bomb => "BOOM!\nБомба взорвалась!",
                _ => "ПОТРАЧЕНО"
            };

            // Размер шрифта: пропорционален cellSize и размеру поля
            float baseFontSize = Math.Min(width, height) * 0.08f;
            float titleFontSize = baseFontSize * 1.4f;
            float reasonFontSize = baseFontSize * 0.9f;
            float scoreFontSize = baseFontSize * 0.9f;
            float nameFontSize = baseFontSize * 0.8f;

            float centerX = width / 2f;
            float centerY = height / 2f;

            // 2) Заголовок "WASTED" с обводкой
            float titleY = centerY - height * 0.22f;
            DrawTextWithStroke(canvas, "WASTED!", centerX, titleY, titleFontSize,
                SKFontStyle.Bold, SKColors.DarkRed, SKColors.White, 5f);

            // 3) Причина смерти с переносом строк
            float reasonY = centerY - height * 0.06f;
            float maxReasonWidth = width * 0.85f; // 85% ширины экрана
            DrawWrappedText(canvas, endText, centerX, reasonY, reasonFontSize,
                SKFontStyle.Bold, SKColors.DarkRed, SKColors.White, 2.5f, maxReasonWidth, lineSpacing: 1.3f);

            // 4) Счёт с обводкой
            float scoreY = centerY + height * 0.12f;
            DrawTextWithStroke(canvas, $"Очки: {_score}", centerX, scoreY, scoreFontSize,
                SKFontStyle.Bold, SKColors.Gold, SKColors.Black, 2.5f);

            // 5) Имя игрока с обводкой и переносом
            float nameY = centerY + height * 0.25f;
            float maxNameWidth = width * 0.9f;
            string nameText = $"Спасибо за игру, {_playerName}";
            DrawWrappedText(canvas, nameText, centerX, nameY, nameFontSize,
                SKFontStyle.Normal, SKColors.PaleGoldenrod, SKColors.Black, 2.5f, maxNameWidth, lineSpacing: 1.2f);
        }

        /// <summary>
        /// Рисует однострочный текст с обводкой (stroke). 
        /// Сначала рисуется обводка (толстый чёрный контур), 
        /// потом поверх — основной текст.
        /// </summary>
        private void DrawTextWithStroke(SKCanvas canvas, string text, float x, float y,
            float fontSize, SKFontStyle fontStyle, SKColor fillColor, SKColor strokeColor, float strokeWidth)
        {
            using var typeface = SKTypeface.FromFamilyName(null, fontStyle);
            using var font = new SKFont(typeface, fontSize);

            // Обводка (рисуем первой — будет "под" текстом)
            using var strokePaint = new SKPaint
            {
                Color = strokeColor,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center,
                Style = SKPaintStyle.Stroke,      // Режим обводки
                StrokeWidth = strokeWidth,        // Толщина контура
                StrokeJoin = SKStrokeJoin.Round,  // Скруглённые углы обводки
                StrokeCap = SKStrokeCap.Round     // Скруглённые концы линий
            };

            // Заливка (рисуем поверх обводки)
            using var fillPaint = new SKPaint
            {
                Color = fillColor,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center,
                Style = SKPaintStyle.Fill         // Обычная заливка
            };

            // Сначала обводка, потом текст — иначе обводка перекроет края текста
            canvas.DrawText(text, x, y, font, strokePaint);
            canvas.DrawText(text, x, y, font, fillPaint);
        }

        /// <summary>
        /// Рисует многострочный текст с автоматическим переносом по словам.
        /// Разбивает текст на строки, если он не влезает в maxWidth.
        /// Поддерживает обводку и настраиваемый межстрочный интервал.
        /// </summary>
        private void DrawWrappedText(SKCanvas canvas, string text, float x, float y,
            float fontSize, SKFontStyle fontStyle, SKColor fillColor, SKColor strokeColor,
            float strokeWidth, float maxWidth, float lineSpacing = 1.2f)
        {
            using var typeface = SKTypeface.FromFamilyName(null, fontStyle);
            using var font = new SKFont(typeface, fontSize);

            // Разбиваем текст на строки, которые влезают в maxWidth
            var lines = WrapTextToLines(text, font, maxWidth);

            // Подготавливаем "кисти"" для обводки и заливки
            using var strokePaint = new SKPaint
            {
                Color = strokeColor,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = strokeWidth,
                StrokeJoin = SKStrokeJoin.Round,
                StrokeCap = SKStrokeCap.Round
            };

            using var fillPaint = new SKPaint
            {
                Color = fillColor,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center,
                Style = SKPaintStyle.Fill
            };

            // Расстояние между базовыми линиями строк
            float lineHeight = fontSize * lineSpacing;

            // Рисуем каждую строку
            for (int i = 0; i < lines.Count; i++)
            {
                float lineY = y + (i * lineHeight);

                // Сначала обводка, потом заливка
                canvas.DrawText(lines[i], x, lineY, font, strokePaint);
                canvas.DrawText(lines[i], x, lineY, font, fillPaint);
            }
        }

        /// <summary>
        /// Разбивает длинный текст на строки по ширине.
        /// Учитывает ширину каждого слова т.е. не рвёт их посередине.
        /// </summary>
        private List<string> WrapTextToLines(string text, SKFont font, float maxWidth)
        {
            var lines = new List<string>();
            var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var currentLine = new System.Text.StringBuilder();

            foreach (var word in words)
            {
                string testLine = currentLine.Length > 0
                    ? currentLine + " " + word
                    : word;

                // Измеряем ширину тестовой строки
                font.MeasureText(testLine, out SKRect bounds);

                if (bounds.Width > maxWidth && currentLine.Length > 0)
                {
                    // Текущее слово не влезает — сохраняем строку и начинаем новую
                    lines.Add(currentLine.ToString());
                    currentLine.Clear();
                    currentLine.Append(word);
                }
                else
                {
                    // Влезает — добавляем слово к текущей строке
                    currentLine.Clear();
                    currentLine.Append(testLine);
                }
            }

            // Добавляем остаток
            if (currentLine.Length > 0)
                lines.Add(currentLine.ToString());

            // Если текст пустой или одно слово слишком длинное — все равно добавляем
            if (lines.Count == 0 && words.Length > 0)
                lines.Add(words[0]);

            return lines;
        }
    }
}
