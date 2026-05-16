using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SnakeGame.ViewModels.Behaviors
{
    public class ScorePulseBehavior : Behavior<Label>
    {
        private Label _label;
        private CancellationTokenSource _animationCts;

        protected override void OnAttachedTo(Label label)
        {
            base.OnAttachedTo(label);
            _label = label;
            label.PropertyChanged += OnPropertyChanged;
        }

        protected override void OnDetachingFrom(Label label)
        {
            label.PropertyChanged -= OnPropertyChanged;
            _animationCts?.Cancel();
            base.OnDetachingFrom(label);
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == Label.TextProperty.PropertyName)
            {
                _animationCts?.Cancel();
                _animationCts = new CancellationTokenSource();
                _ = AnimateAsync(_animationCts.Token);
            }
        }

        private async Task AnimateAsync(CancellationToken ct)
        {
            try
            {
                // Сохраняем оригинальные значения
                var originalScale = _label.Scale;
                var originalTextColor = _label.TextColor;

                // Цвета градиента (можно вынести в BindableProperty)
                var startColor = Colors.Gold;
                var endColor = originalTextColor;

                // Фаза 1: Увеличение + градиент вперёд
                var scaleTask = _label.ScaleToAsync(1.4, 200, Easing.CubicOut);
                var colorTask = ColorAnimation(startColor, 200, ct);
                await Task.WhenAll(scaleTask, colorTask);

                ct.ThrowIfCancellationRequested();

                // Фаза 2: Возврат + градиент назад
                var scaleDownTask = _label.ScaleToAsync(1.0, 300, Easing.CubicInOut);
                var colorBackTask = ColorAnimation(endColor, 300, ct);
                await Task.WhenAll(scaleDownTask, colorBackTask);
            }
            catch (OperationCanceledException)
            {
                // Сброс при отмене
                _label.Scale = 1.0;
                _label.TextColor = Colors.White; // или ваш базовый цвет
            }
        }

        private async Task ColorAnimation(Color targetColor, uint duration, CancellationToken ct)
        {
            // MAUI не имеет встроенной анимации цвета текста, делаем вручную
            var startColor = _label.TextColor;
            var startTime = DateTime.UtcNow;

            while (DateTime.UtcNow - startTime < TimeSpan.FromMilliseconds(duration))
            {
                ct.ThrowIfCancellationRequested();
                var progress = (DateTime.UtcNow - startTime).TotalMilliseconds / duration;
                progress = Math.Min(progress, 1.0);

                _label.TextColor = Color.FromRgba(
                    Lerp(startColor.Red, targetColor.Red, progress),
                    Lerp(startColor.Green, targetColor.Green, progress),
                    Lerp(startColor.Blue, targetColor.Blue, progress),
                    Lerp(startColor.Alpha, targetColor.Alpha, progress)
                );

                await Task.Delay(16, ct); // ~60fps
            }

            _label.TextColor = targetColor;
        }

        private static double Lerp(double start, double end, double t) =>
            start + (end - start) * t;
    }
}
