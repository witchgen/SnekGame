using static SnakeGame.Models.GameInfo.Enums;

namespace SnakeGame.Custom;

public static class FieldExtensions
{
    /// <summary>
    /// Преобразуем клетку в соответствующий эмодзи (максимум 10 типов "графики")
    /// </summary>
    /// <param name="cell">Число</param>
    public static string ToEmoji(this int cell)
    =>
        cell switch
        {
            0 => "⬛", // чистое поле
            1 => "🤡", // граница поля
            2 => "🟩", // тело змеюки
            3 => "🥞", // "яблоко", ням-ням!
            4 => "🔥", // тело змеюки (гейм овер)
            5 => "🟥", // голова змеюки
            6 => "💀", // голова (гейм овер)
            7 => "💣", // бомба (ой-ой!)
            8 => "💥", // БУМ!
            9 => "🔳", // Debug
            _ => cell.ToString()
        };

    /// <summary>
    /// Вспомогательный метод для пресечения возможности задать обратное движение для змеи
    /// </summary>
    /// <param name="direction">Целевое направление движения</param>
    /// <returns>Обратное направление</returns>
    public static Direction ToOpposite(this Direction direction)
        => direction switch
        {
            Direction.Up => Direction.Down,
            Direction.Down => Direction.Up,
            Direction.Left => Direction.Right,
            Direction.Right => Direction.Left,
            _ => direction
        };

}
