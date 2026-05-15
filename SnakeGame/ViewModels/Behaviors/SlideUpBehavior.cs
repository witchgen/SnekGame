using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace SnakeGame.ViewModels.Behaviors;

public class SlideUpBehavior : Behavior<VisualElement>
{
    private VisualElement? _element;

    public static readonly BindableProperty IsOpenProperty =
        BindableProperty.Create(
            nameof(IsOpen),
            typeof(bool),
            typeof(SlideUpBehavior),
            false,
            propertyChanged: OnIsOpenChanged);

    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    private static void OnIsOpenChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var behavior = (SlideUpBehavior)bindable;
        behavior.Animate((bool)newValue);
    }

    private async void Animate(bool open)
    {
        if (_element == null) return;

        // Получаем высоту элемента; если не измерена — запасное значение
        var height = _element.Height > 0 ? _element.Height : 500;
        var targetY = open ? 0 : height;

        await _element.TranslateToAsync(0, targetY, 300, Easing.CubicOut);
    }

    protected override void OnAttachedTo(VisualElement bindable)
    {
        base.OnAttachedTo(bindable);
        _element = bindable;
        // Изначально скрыта за пределами экрана
        bindable.TranslationY = 500;
    }

    protected override void OnDetachingFrom(VisualElement bindable)
    {
        base.OnDetachingFrom(bindable);
        _element = null;
    }
}