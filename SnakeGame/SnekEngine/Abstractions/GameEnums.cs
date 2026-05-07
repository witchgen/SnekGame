namespace SnakeGame.SnekEngine.Abstractions
{
    public class GameEnums
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
            Idle,
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

        public enum CellContent
        {
            Empty,
            Wall,
            Snake,
            Apple,
            Bomb
        }

        public enum GameOverReason
        {
            Wall,
            BitTail,
            Bomb,
            Victory
        }

        public enum DebugOption
        {
            ToggleBombSpawnAreaHighlight,
            DrawAIpath
        }
    }
}
