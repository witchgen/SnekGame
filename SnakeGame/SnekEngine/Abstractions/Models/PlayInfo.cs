using System;
using static SnakeGame.SnekEngine.Abstractions.GameEnums;

namespace SnakeGame.SnekEngine.Abstractions.Models
{
    public class PlayInfo
    {
        /// <summary>
        /// Ранг в таблице рекордов
        /// </summary>
        public int Rank { get; set; } = 1;
        /// <summary>
        /// Имя игрока
        /// </summary>
        public string PlayerName { get; set; }
        /// <summary>
        /// Уровень сложности ( 0 - 2 )
        /// </summary>
        public Difficulty DifficultyLevel { get; set; }
        /// <summary>
        /// Временная метка создания записи
        /// </summary>
        public DateTime DtEnded { get; set; } = DateTime.UtcNow;
        /// <summary>
        /// Набранные очки
        /// </summary>
        public int FinalScore { get; set; }
        /// <summary>
        /// Последняя длина змеи перед гамовером (включает начальные 3 сегмента)
        /// </summary>
        public int MaxSnakeLength { get; set; }
        /// <summary>
        /// Причина смерти
        /// </summary>
        public GameOverReason DeathReason { get; set; }
        public int Seed { get; set; }
        public GameSnapshot CurrentState { get; set; }
    }
}
