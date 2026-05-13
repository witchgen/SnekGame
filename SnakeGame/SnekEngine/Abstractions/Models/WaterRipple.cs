using SkiaSharp;

namespace SnakeGame.SnekEngine.Abstractions.Models
{
    public class WaterRipple
    {
        public SKPoint Center;
        public float Time;      // сколько времени прошло
        public float Duration;  // сколько длится эффект
    }
}
