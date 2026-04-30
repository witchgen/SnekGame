namespace SnakeGame.Models.GameInfo
{
    public class Enums
    {

        public enum Direction
        {
            Up,
            Down,
            Left,
            Right
        }

        public enum GameStatus
        {
            Initialized,
            Running,
            Paused,
            Ended
        }

        public enum Difficulty
        {
            Easy,
            Medium,
            Hard
        }

        public enum GameOverReason
        {
            Wall,
            BitTail,
            Bomb,
            Starvation
        }
    }
}