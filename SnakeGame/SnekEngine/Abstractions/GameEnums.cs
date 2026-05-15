namespace SnakeGame.SnekEngine.Abstractions
{
    public class GameEnums
    {
        /// <summary>
        /// Состояние экрана перед началом игры (в зависимости от него рисуем окно настроек и кнопку старта, например)
        /// </summary>
        public enum GameScreenState
        {
            Idle,       // Начальное положение: видна нижняя панель кнопок (перегенерировать, настройки)
            Setup,      // Настройки видны, игра не начата
            Ready,      // Поле сгенерировано, можно стартовать
            Playing,    // Игра идёт
            Paused,     // Игра на паузе
            GameOver    // Игра закончилась, показываем результат + кнопку "Настройки"
        }

        /// <summary>
        /// Направление движения змеи
        /// </summary>
        public enum Direction
        {
            Up,
            Down,
            Left,
            Right
        }

        /// <summary>
        /// Статус игрового сеанса
        /// </summary>
        public enum GameStatus
        {
            Idle,           // Бездействует (этап настроек)
            Initialized,    // Проинициализирован, готов к запуску игрового цикла
            Running,        // Игровой процесс
            Paused,         // Пауза
            Ended           // Завершение сеанса
        }

        /// <summary>
        /// Пресеты сложности
        /// </summary>
        public enum Difficulty
        {
            Easy,
            Medium,
            Hard
        }

        /// <summary>
        /// Содержимое доступной для змеи клетки
        /// </summary>
        public enum CellContent
        {
            Empty,      // Пустая (можно сюда идти)
            Wall,       // Стена 
            Snake,      // Тело самой змеи
            Apple,      // Еда (ням-ням!)
            Bomb        // Бомба
        }

        /// <summary>
        /// Причина завершения игры
        /// </summary>
        public enum GameOverReason
        {
            Wall,       // Врезались в стену
            BitTail,    // Врезались сами в себя
            Bomb,       // Подорвались на бомбе
            Victory     // Ура, победа! (съели все возможные "яблоки")
        }

        /// <summary>
        /// Опции отладки
        /// </summary>
        public enum DebugOption
        {
            ToggleBombSpawnAreaHighlight,
            DrawAIpath
        }
    }
}
