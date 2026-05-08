using Microsoft.Maui.Controls;
using SkiaSharp;
using SnakeGame.ViewModels;
using System;

namespace SnakeGame.Views;

public partial class GamePage : ContentPage
{
	private readonly GameViewModel _gvm;
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

    private void OnGameScreenPaintSurface(object sender, SkiaSharp.Views.Maui.SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;

        float canvasWidth = e.Info.Width;
		float canvasHeight = e.Info.Height;

        float radius = 20f;

        // создаём скруглённый прямоугольник
        using var clipPath = new SKPath();
        clipPath.AddRoundRect(new SKRect(0, 0, canvasWidth, canvasHeight), radius, radius);

        // обрезаем всё по форме
        canvas.ClipPath(clipPath, antialias: true);

        // рисуем рамку
        using (var borderPaint = new SKPaint
        {
            Color = SKColors.Gray,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 3,
            IsAntialias = true
        })
        {
            canvas.DrawPath(clipPath, borderPaint);
        }

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

        canvas.Translate(offsetX, offsetY);

        _gvm.UpdateCanvasSize(canvasWidth, canvasHeight);
        _gvm.Render(canvas);
    }
}