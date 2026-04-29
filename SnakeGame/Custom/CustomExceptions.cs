using System;

namespace SnakeGame.Custom
{
    public static class CustomExceptions
    {
        public class CantPlaceItemsException : Exception
        {
            public CantPlaceItemsException()
            {

            }

            public CantPlaceItemsException(string msg) : base(msg)
            {

            }
        }
    }
}
