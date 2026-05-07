using SnakeGame.SnekEngine.Abstractions.Models;

namespace SnakeGame.SnekEngine.Abstractions.Interfaces
{
    public interface IGameplayService
    {
        /// <summary>
        /// Инициализация игрового раунда
        /// </summary>
        /// <returns>Первичный снимок игры, по нему будет "ориентироваться" рендер</returns>
        GameSnapshot InitializeLevel();
    }
}
