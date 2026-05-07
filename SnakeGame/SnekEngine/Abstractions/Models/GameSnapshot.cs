using System.Collections.Generic;
using static SnakeGame.SnekEngine.Abstractions.GameEnums;

namespace SnakeGame.SnekEngine.Abstractions.Models
{
    public class GameSnapshot
    {
        public int[,] Field { get; set; }
        public Snake CurrentSnake { get; set; }
        public (int i, int j) Apple { get; set; }
        public HashSet<(int i, int j)>? Bombs { get; set; }
        public int CurrentScore { get; set; }
        public bool HasExploded { get; set; }
        public GameOverReason EndReason { get; set; }
        //public HashSet<(int i, int j)> Walls { get; init; }
    }
}
