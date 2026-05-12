using Microsoft.Maui.Controls;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SnakeGame.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using static SnakeGame.SnekEngine.Abstractions.GameEnums;

namespace SnakeGame.Views;

public partial class GamePage : ContentPage
{
	private readonly GameViewModel _gvm;
    Stopwatch _sw = Stopwatch.StartNew();
    long _last = 0;

    private SKPoint _touchStart; // для тачпада
    private SKPoint _lastDirectionPoint;
    private readonly List<SKPoint> _swipePoints = new();
    private readonly float _minMoveThreshold = 25f; // Минимальное смещение для смены направления

    // "Хвост кометы":
    private readonly List<(SKPoint Point, DateTimeOffset Time)> _swipeTrail = new();
    private readonly TimeSpan _trailDuration = TimeSpan.FromMilliseconds(200);

    // FPS
    private int _frameCount;
    private long _lastFpsTime;
    private float _currentFps;

    public GamePage(GameViewModel gvm)
	{
		InitializeComponent();
		BindingContext = gvm;
		_gvm = gvm;

		_gvm.RequestRedraw += () =>
		{
			gameScreen.InvalidateSurface();
		};
	}

    private void OnGameScreenPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        //long now = _sw.ElapsedMilliseconds;
        //float delta = (now - _last) / 1000f;
        //_last = now;

        //_gvm.Update(delta);

        var canvas = e.Surface.Canvas;

        // сброс перед каждым кадром
        canvas.ResetMatrix();
        //canvas.Clear(SKColors.Transparent);

        float canvasWidth = e.Info.Width;
		float canvasHeight = e.Info.Height;

        // размеры поля
        int cols = _gvm.Settings.Cols;
        int rows = _gvm.Settings.Rows;

        // вычисляем размер клетки
        float cellSize = Math.Min(canvasWidth / cols, canvasHeight / rows);

        // реальные размеры поля
        float fieldWidth = cellSize * cols;
        float fieldHeight = cellSize * rows;

        // центрирование
        float offsetX = (canvasWidth - fieldWidth) / 2f;
        float offsetY = (canvasHeight - fieldHeight) / 2f;

        // Сохраняем состояние перед трансформацией
        canvas.Save();
        canvas.Translate(offsetX, offsetY);

        _gvm.UpdateCanvasSize(canvasWidth, canvasHeight);
        _gvm.Render(canvas, canvasWidth, canvasHeight);

        // Восстанавливаем — теперь матрица "чистая"
        canvas.Restore();
    }

    private void OnTouchPanelTouch(object sender, SKTouchEventArgs e)
    {
        switch (e.ActionType)
        {
            case SKTouchAction.Pressed:
                _swipePoints.Clear();
                _touchStart = e.Location;
                _lastDirectionPoint = e.Location;
                //_swipePoints.Add(_touchStart);
                AddTrailPoint(e.Location);
                break;

            case SKTouchAction.Moved:
                AddTrailPoint(e.Location);
                //_swipePoints.Add(e.Location);
                TryChangeDirection(e.Location);
                TouchPanel.InvalidateSurface();
                break;

            case SKTouchAction.Released:
                //DetectSwipe(_touchStart, e.Location);
                _swipeTrail.Clear();
                TouchPanel.InvalidateSurface();
                break;
        }

        e.Handled = true;
    }

    private void DetectSwipe(SKPoint start, SKPoint end)
    {
        float dx = end.X - start.X;
        float dy = end.Y - start.Y;

        if (Math.Abs(dx) > Math.Abs(dy))
        {
            if (dx > 0) _gvm.ChangeDirection(Direction.Right);
            else _gvm.ChangeDirection(Direction.Left);
        }
        else
        {
            if (dy > 0) _gvm.ChangeDirection(Direction.Down);
            else _gvm.ChangeDirection(Direction.Up);
        }
    }

    /// <summary>
    /// Метод для детекта направления тачскрином. Нужен для передачи направления, когда "рулим" пальцем по экрану
    /// </summary>
    /// <param name="currentPoint"></param>
    private void TryChangeDirection(SKPoint currentPoint)
    {
        float dx = currentPoint.X - _lastDirectionPoint.X;
        float dy = currentPoint.Y - _lastDirectionPoint.Y;

        // Ждём, пока палец сдвинется достаточно далеко
        if (Math.Abs(dx) < _minMoveThreshold && Math.Abs(dy) < _minMoveThreshold)
            return;

        Direction? newDirection = null;

        if (Math.Abs(dx) > Math.Abs(dy))
        {
            // Горизонтальное движение
            newDirection = dx > 0 ? Direction.Right : Direction.Left;
        }
        else
        {
            // Вертикальное движение
            newDirection = dy > 0 ? Direction.Down : Direction.Up;
        }

        _gvm.ChangeDirection(newDirection.Value);
        _lastDirectionPoint = currentPoint; // Сбрасываем точку отсчета
    }

    private void AddTrailPoint(SKPoint point)
    {
        var now = DateTimeOffset.UtcNow;
        _swipeTrail.Add((point, now));

        // Убираем точки старше _trailDuration
        while (_swipeTrail.Count > 0 && now - _swipeTrail[0].Time > _trailDuration)
        {
            _swipeTrail.RemoveAt(0);
        }
    }

    /// <summary>
    /// "След кометы" на тач-панели при проведении пальцем (ДОРАБОТАТЬ БЫ ВИЗУАЛ)
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnTouchPanelPaint(object sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        if (_swipeTrail.Count < 2) return;

        var now = DateTimeOffset.UtcNow;
        var paint = new SKPaint
        {
            StrokeWidth = 24,
            Style = SKPaintStyle.Stroke,
            StrokeCap = SKStrokeCap.Round,
            StrokeJoin = SKStrokeJoin.Round,
            IsAntialias = true
        };

        // Рисуем сегменты с градиентом по времени: свежие = яркие, старые = прозрачные
        for (int i = 0; i < _swipeTrail.Count - 1; i++)
        {
            float age1 = (float)(now - _swipeTrail[i].Time).TotalMilliseconds / (float)_trailDuration.TotalMilliseconds;
            float age2 = (float)(now - _swipeTrail[i + 1].Time).TotalMilliseconds / (float)_trailDuration.TotalMilliseconds;

            // age: 0 = сейчас, 1 = исчезает
            byte alpha1 = (byte)(255 * Math.Max(0, 1 - age1));
            byte alpha2 = (byte)(255 * Math.Max(0, 1 - age2));

            // Градиентный шейдер вдоль сегмента
            var colors = new[]
            {
            SKColors.CornflowerBlue.WithAlpha(alpha1),
            SKColors.CornflowerBlue.WithAlpha(alpha2)
        };

            var shader = SKShader.CreateLinearGradient(
                _swipeTrail[i].Point,
                _swipeTrail[i + 1].Point,
                colors,
                null,
                SKShaderTileMode.Clamp);

            paint.Shader = shader;

            var path = new SKPath();
            path.MoveTo(_swipeTrail[i].Point);
            path.LineTo(_swipeTrail[i + 1].Point);

            canvas.DrawPath(path, paint);
        }
    }
}