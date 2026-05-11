namespace SnakeGame.SnekEngine.Abstractions
{
    public class GameEnums
    {
        public enum GameScreenState
        {
            Setup,      // Настройки видны, игра не начата
            Ready,      // Поле сгенерировано, можно стартовать
            Playing,    // Игра идёт
            GameOver    // Игра закончилась, показываем результат + кнопку "Настройки"
        }

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
