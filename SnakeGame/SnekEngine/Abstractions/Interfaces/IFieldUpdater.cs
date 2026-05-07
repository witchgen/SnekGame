using SnakeGame.SnekEngine.Abstractions.Models;
using static SnakeGame.SnekEngine.Abstractions.GameEnums;

namespace SnakeGame.SnekEngine.Abstractions.Interfaces
{
    internal interface IFieldUpdater
    {
        public GameSnapshot UpdateField(GameSnapshot prev, Direction direction);
    }
}
