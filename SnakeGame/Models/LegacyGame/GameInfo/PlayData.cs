using System;
using static SnakeGame.Models.LegacyGame.GameInfo.Enums;

namespace SnakeGame.Models.LegacyGame.GameInfo
{
    public class PlayData
    {
        public bool WasSnakeAIControlled { get; set; }
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
        public LegacyDifficulty DifficultyLevel { get; set; }
        /// <summary>
        /// Временная метка создания записи
        /// </summary>
        public DateTime DtSnapshot { get; set; } = DateTime.UtcNow;
        /// <summary>
        /// Набранные очки
        /// </summary>
        public int Score { get; set; }
        /// <summary>
        /// Последняя длина змеи перед гамовером (включает начальные 3 сегмента)
        /// </summary>
        public int MaxSnakeLength { get; set; }
        /// <summary>
        /// Причина смерти
        /// </summary>
        public LegacyGameOverReason DeathReason { get; set; } 
    }
}
