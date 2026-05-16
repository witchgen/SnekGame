using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using CommunityToolkit.Maui.Converters;
using static SnakeGame.Models.LegacyGame.GameInfo.Enums;
using static SnakeGame.SnekEngine.Abstractions.GameEnums;

namespace SnakeGame.Custom
{
    // ========== LEGACY: ============
    public class DifficultyToColorConverter : BaseConverterOneWay<LegacyDifficulty, Brush>
    {
        public override Brush ConvertFrom(LegacyDifficulty value, CultureInfo? culture) => value switch
        {
            LegacyDifficulty.Easy => new SolidColorBrush(Color.FromArgb("#4CAF50")),
            LegacyDifficulty.Medium => new SolidColorBrush(Color.FromArgb("#FF9800")),
            LegacyDifficulty.Hard => new SolidColorBrush(Color.FromArgb("#F44336")),
            _ => new SolidColorBrush(Colors.LightGray),
        };

        public override Brush DefaultConvertReturnValue { get; set; } = new SolidColorBrush(Colors.Gray);
    }

    public class LegacyDifficultyToBackgroundConverter : BaseConverterOneWay<LegacyDifficulty, Color>
    {
        public override Color ConvertFrom(LegacyDifficulty value, CultureInfo? culture) => value switch
        {
            LegacyDifficulty.Easy => Color.FromArgb("#4CAF50"),
            LegacyDifficulty.Medium => Color.FromArgb("#d1ae2e"),
            LegacyDifficulty.Hard => Color.FromArgb("#c20e0e"),
            _ => Color.FromArgb("#A9E9E9E"),
        };

        public override Color DefaultConvertReturnValue { get; set; } = Color.FromArgb("#1A808080");
    }

    public class DeathReasonToMessageConverter : BaseConverterOneWay<LegacyGameOverReason, string>
    {
        public override string ConvertFrom(LegacyGameOverReason value, CultureInfo? culture) => value switch
        {
            LegacyGameOverReason.Wall => "🧱 Не справился с управлением",
            LegacyGameOverReason.BitTail => "🐍 Укусил себя за хвост",
            LegacyGameOverReason.Bomb => "💥 Бабахнул на славу",
            LegacyGameOverReason.Victory => "😇 Преисполнился в своём познании 😇",
            LegacyGameOverReason.AIsucker => "🤖 Доверил свою жизнь ИИ 🤖",
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

    // ========== ACTUAL: ============

    public class BoolToColorConverter : IValueConverter
    {
        public Color TrueColor { get; set; } = Colors.DarkSlateGray;
        public Color FalseColor { get; set; } = Colors.Transparent;

        public object Convert(object value, Type targetType, object parameter, CultureInfo? culture)
        {
            if (value is bool b)
                return b ? TrueColor : FalseColor;

            return FalseColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class GameOverReasonToMessageConverter : BaseConverterOneWay<GameOverReason, string>
    {
        public override string ConvertFrom(GameOverReason value, CultureInfo? culture) => value switch
        {
            GameOverReason.Wall => "🧱 Не справился с управлением",
            GameOverReason.BitTail => "🐍 Укусил себя за хвост",
            GameOverReason.Bomb => "💥 Бабахнул на славу",
            GameOverReason.Victory => "😇 Преисполнился в своём познании 😇",
            _ => "❓ Вознесся на новый уровень бытия"
        };

        public override string DefaultConvertReturnValue { get; set; } = "❓ Неизвестно";
    }

    public class RankToColorConverter : BaseConverterOneWay<int, Color>
    {
        public override Color ConvertFrom(int value, CultureInfo? culture) => (value) switch
        {
            1 => Colors.Gold,
            2 => Colors.AliceBlue,
            3 => Colors.DarkOrange,
            _ => Colors.Gray
        };

        public override Color DefaultConvertReturnValue { get; set; } = Colors.Gray;
    }

    public class RankToBrushConverter : BaseConverterOneWay<int, Brush>
    {
        public override Brush ConvertFrom(int value, CultureInfo? culture)
        {
            var color = value switch
            {
                1 => Colors.Gold,
                2 => Colors.AliceBlue,
                3 => Colors.DarkOrange,
                _ => Colors.Gray
            };
            return new SolidColorBrush(color);
        }

        public override Brush DefaultConvertReturnValue { get; set; } = new SolidColorBrush(Colors.Gray);
    }
}



