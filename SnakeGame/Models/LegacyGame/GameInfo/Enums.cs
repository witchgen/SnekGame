namespace SnakeGame.Models.LegacyGame.GameInfo
{
    public class Enums
    {

        public enum Direction
        {
            Up,
            Down,
            Left,
            Right,
            None    // Для управления змеей с ИИ
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
            Victory,
            AIsucker
        }

        public enum DebugOption
        {
            ToggleBombSpawnAreaHighlight,
            ToggleSnakeAi,
            DrawAIpath
        }
    }
}