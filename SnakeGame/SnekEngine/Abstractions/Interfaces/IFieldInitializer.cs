using SnakeGame.SnekEngine.Abstractions.Models;
using System;

namespace SnakeGame.SnekEngine.Abstractions.Interfaces
{
    public interface IFieldInitializer
    {
        /// <summary>
        /// Инициализируем поле для новой игры
        /// </summary>
        /// <param name="settings">Стартовая конфигурация</param>
        /// <returns>Первый снимок раунда</returns>
        GameSnapshot InitializeField(InitialSettings settings, Random rnd);
    }
}
