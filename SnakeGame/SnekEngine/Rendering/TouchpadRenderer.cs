using SkiaSharp;
using SnakeGame.SnekEngine.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SnakeGame.SnekEngine.Rendering
{
    public class TouchpadRenderer
    {
        private readonly List<WaterRipple> _ripples = new();
        private readonly List<(SKPoint point, DateTime time)> _swipeTrail = new();
        private readonly TimeSpan _trailDuration = TimeSpan.FromMilliseconds(200);

        public void AddRipple(SKPoint p)
        {
            _ripples.Add(new WaterRipple { Center = p, Time = 0, Duration = 0.5f });
        }

        public void AddSwipePoint(SKPoint p)
        {
            _swipeTrail.Add((p, DateTime.UtcNow));
        }

        public void ClearSwipeTrail()
        {
            _swipeTrail.Clear();
        }
    }
}
