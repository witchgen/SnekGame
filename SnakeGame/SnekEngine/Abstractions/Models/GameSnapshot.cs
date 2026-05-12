using System.Collections.Generic;
using static SnakeGame.SnekEngine.Abstractions.GameEnums;

namespace SnakeGame.SnekEngine.Abstractions.Models
{
    // Снимок текущего игрового состояния
    public class GameSnapshot
    {
        public int[,] Field { get; set; }                           // Игровое поле
        public HashSet<Direction> AvailableDirections { get; set; } // Доступные направления
        public Snake Snake { get; set; }                            // Положение змеи  (голова-тело-хвост, все здесь)
        public (int i, int j) Apple { get; set; }                   // Положение еды
        public HashSet<(int i, int j)>? Bombs { get; set; }         // Положение бомб (если null - бомб нету)
        public int Score { get; set; }                              // Текущий счет в рамках игрового тика
        public bool HasExploded { get; set; }                       // Говорит, напоролись ли на бомбу (понадобится для отрисовки доп. графики в дальнейшем)
        public GameOverReason? EndReason { get; set; }              // Причина геймовера (проиграли либо выиграли и как именно)
        //public HashSet<(int i, int j)> Walls { get; init; }

        /// <summary>
        /// Получить идентичную копию текущего состояния (с клонированием змеи и поля)
        /// </summary>
        /// <returns></returns>
        public GameSnapshot Clone()
        {
            return new GameSnapshot
            {
                Field = (int[,])Field.Clone(),
                Snake = (Snake)Snake.Clone(),
                AvailableDirections = [.. AvailableDirections],
                Apple = Apple,
                Bombs = Bombs != null ? new HashSet<(int, int)>(Bombs) : null,
                Score = Score,
                HasExploded = HasExploded,
                EndReason = EndReason
            };
        }
    }
}
