using SnakeGame.SnekEngine.Abstractions.Models;

namespace SnakeGame.SnekEngine.Abstractions.Interfaces
{
    public interface IFieldInitializer
    {
        /// <summary>
        /// Инициализируем поле для новой игры
        /// </summary>
        /// <param name="settings">Стартовая конфигурация</param>
        /// <returns>Первый снимок раунда</returns>
        GameSnapshot InitializeField(InitialSettings settings);
    }
}
