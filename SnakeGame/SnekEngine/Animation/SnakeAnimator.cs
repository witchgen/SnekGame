
using SkiaSharp;
using SnakeGame.SnekEngine.Abstractions.Models;
using System;
using System.Collections.Generic;

namespace SnakeGame.SnekEngine.Animation
{
    public class SnakeAnimator
    {
        private readonly float _cellSize;

        public SnakeAnimator(float cellSize)
        {
            _cellSize = cellSize;
        }

        // WIP
        //public void Interpolate(GameSnapshot oldState, GameSnapshot newState, float t)
        //{
        //    t = Math.Clamp(t, 0f, 1f);

        //    var points = new List<SKPoint>();

        //    for (int i = 0; i < newState.CurrentSnake.Body.Count; i++)
        //    {
        //        var (ox, oy) = oldState.[i];
        //        var (nx, ny) = newState.SnakeCells[i];

        //        float px = Lerp(ox * _cellSize, nx * _cellSize, t);
        //        float py = Lerp(oy * _cellSize, ny * _cellSize, t);

        //        points.Add(new SKPoint(px, py));
        //    }

        //    var apple = new SKPoint(
        //        newState.Apple.X * _cellSize + _cellSize / 2,
        //        newState.Apple.Y * _cellSize + _cellSize / 2
        //    );

        //    return new InterpolatedState
        //    {
        //        SnakePoints = points,
        //        Apple = apple
        //    };
        //}
    }
}
