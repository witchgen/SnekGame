using SnakeGame.SnekEngine.Abstractions.Models;
using System.Collections.Generic;
using static SnakeGame.SnekEngine.Abstractions.GameEnums;

namespace SnakeGame.SnekEngine.Abstractions.Interfaces
{
    public interface IGameplayService
    {
        /// <summary>
        /// Инициализация игрового раунда
        /// </summary>
        /// <returns>Первичный снимок игры, по нему будет "ориентироваться" рендер</returns>
        GameSnapshot InitializeLevel(InitialSettings setup);
        /// <summary>
        /// Один такт игрового процесса (логика)
        /// </summary>
        /// <param name="currentState"></param>
        /// <returns></returns>
        GameSnapshot Tick(GameSnapshot currentState, Direction buffer);
    }
}
