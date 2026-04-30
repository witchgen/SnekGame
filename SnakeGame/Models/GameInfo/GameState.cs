using static SnakeGame.Models.GameInfo.Enums;

namespace SnakeGame.Models.GameInfo
{
    public class GameState
    {
        /// <summary>
        /// Индикатор, была ли игра продлолжена с паузы
        /// </summary>
        public bool IsNewGame { get; set; }
        /// <summary>
        /// Состояние игрового поля
        /// </summary>
        public int[,] GameField { get; set; }
        /// <summary>
        /// Состояние змеи
        /// </summary>
        public Snek SolidSnake { get; set; }
        /// <summary>
        /// Положение головы змеи
        /// </summary>
        public SnekSegment SnakeHeadPosition { get; set; }
        /// <summary>
        /// Положение "яблока"
        /// </summary>
        public (int, int) ApplePosition { get; set; }
        /// <summary>
        /// Положение бомбы
        /// </summary>
        public (int, int) BombPosition { get; set; }
        /// <summary>
        /// Последнее заданное направление (изначально ВВЕРХ)
        /// </summary>
        public Direction CurrentDirection { get; set; }
        /// <summary>
        /// Итоги раунда + имя игрока
        /// </summary>
        public PlayData CurrentGameData { get; set; }
    }
}
