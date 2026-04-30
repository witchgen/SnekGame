
using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using CommunityToolkit.Maui.Converters;
using static SnakeGame.Models.GameInfo.Enums;

namespace SnakeGame.Custom
{
    public class DifficultyToColorConverter : BaseConverterOneWay<Difficulty, Brush>
    {
        public override Brush ConvertFrom(Difficulty value, CultureInfo? culture) => value switch
        {
            Difficulty.Easy => new SolidColorBrush(Color.FromArgb("#4CAF50")),
            Difficulty.Medium => new SolidColorBrush(Color.FromArgb("#FF9800")),
            Difficulty.Hard => new SolidColorBrush(Color.FromArgb("#F44336")),
            _ => new SolidColorBrush(Colors.LightGray),
        };

        public override Brush DefaultConvertReturnValue { get; set; } = new SolidColorBrush(Colors.Gray);
    }

    public class DifficultyToBackgroundConverter : BaseConverterOneWay<Difficulty, Color>
    {
        public override Color ConvertFrom(Difficulty value, CultureInfo? culture) => value switch
        {
            Difficulty.Easy => Color.FromArgb("#4CAF50"),
            Difficulty.Medium => Color.FromArgb("#d1ae2e"),
            Difficulty.Hard => Color.FromArgb("#c20e0e"),
            _ => Color.FromArgb("#A9E9E9E"),
        };

        public override Color DefaultConvertReturnValue { get; set; } = Color.FromArgb("#1A808080");
    }

    public class DeathReasonToMessageConverter : BaseConverterOneWay<GameOverReason, string>
    {
        public override string ConvertFrom(GameOverReason value, CultureInfo? culture) => value switch
        {
            GameOverReason.Wall => "🧱 Не справился с управлением",
            GameOverReason.BitTail => "🐍 Укусил себя за хвост",
            GameOverReason.Bomb => "💥 Бабахнул на славу",
            _ => "❓ Вознесся на новый уровень бытия"
        };

        public override string DefaultConvertReturnValue { get; set; } = "❓ Неизвестно";
    }

    public class IndexToRankConverter : BaseConverterOneWay<int, string>
    {
        public override string ConvertFrom(int value, CultureInfo? ci) => (value + 1) switch
        {
            1 => "🥇",
            2 => "🥈",
            3 => "🥉",
            var n => $"#{n}"
        };

        public override string DefaultConvertReturnValue { get; set; } = "#?";
    }

    public class DateTimeToShortStringConverter : BaseConverterOneWay<DateTime, string>
    {
        public override string ConvertFrom(DateTime value, CultureInfo? ci) =>
            value.ToLocalTime().ToString("dd.MM HH:mm", CultureInfo.CurrentCulture);

        public override string DefaultConvertReturnValue { get; set; } = string.Empty;
    }

    public class ScoreToFormattedStringConverter : BaseConverterOneWay<int, string>
    {
        public override string ConvertFrom(int value, CultureInfo? ci) => value.ToString("N0", CultureInfo.CurrentCulture);

        public override string DefaultConvertReturnValue { get; set; } = "0";
    }
}



