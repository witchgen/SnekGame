using static SnakeGame.SnekEngine.Abstractions.GameEnums;
using System.Collections.Generic;

namespace SnakeGame.SnekEngine.Abstractions.Models
{
    public class InitialSettings
    {
        public int? Seed { get; set; }
        public int Rows { get; set; }
        public int Cols { get; set; }
        public (int i, int j) SnakeSpawnPoint { get; set; }
        public Direction FirstDirection { get; set; }
        public int BombsCount { get; set; } = 0;
        public bool CustomWalls { get; set; } = false;
        public HashSet<(int i, int j)> Walls { get; set; } = new();
    }
}
