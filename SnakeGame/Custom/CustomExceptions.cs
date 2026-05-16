using System;
using static SnakeGame.Models.LegacyGame.GameInfo.Enums;

namespace SnakeGame.Custom
{
    public static class CustomExceptions
    {
        public class CantPlaceItemsException : Exception
        {
            public CantPlaceItemsException() { }

            public CantPlaceItemsException(string msg) : base(msg) { }
        }

        public class CanNotSetDirectionException : Exception
        {
            public CanNotSetDirectionException() { }

            public CanNotSetDirectionException(Direction dir) : base($"Could not set desired direction! Received direction: {dir.AsString()}")
            {
                
            }

            public CanNotSetDirectionException(string msg) : base(msg) { }
        }
    }
}
