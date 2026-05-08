using static SnakeGame.SnekEngine.Abstractions.GameEnums;
namespace SnakeGame.SnekEngine.Custom
{
    internal static class FieldExtensions
    {
        /// <summary>
        /// Вспомогательный метод для пресечения возможности задать обратное движение для змеи
        /// </summary>
        /// <param name="direction">Целевое направление движения</param>
        /// <returns>Обратное направление</returns>
        internal static Direction ToOpposite(this Direction direction)
            => direction switch
            {
                Direction.Up => Direction.Down,
                Direction.Down => Direction.Up,
                Direction.Left => Direction.Right,
                Direction.Right => Direction.Left,
                _ => direction
            };

        internal static string AsString(this Direction dir)
            => dir switch
            {
                Direction.Up => "Вверх",
                Direction.Down => "Вниз",
                Direction.Left => "Влево",
                Direction.Right => "Вправо",
                _ => "Неизвестное направление"
            };
    }
}
