using System.Collections.Generic;

namespace SnakeGame.Models.GameInfo;

/// <summary>
/// Змеюка наша игровая. Инициализируется координатами головы, состоит из сегментов
/// </summary>
public class Snek
{
    public LinkedList<SnekSegment> body = new LinkedList<SnekSegment>();

    public Snek(SnekSegment head)
    {
        for (int x = 0; x < 3; x++) // У стартовой змеюки три сегмента
        {
            body.AddFirst(head);
        }
    }

    /// <summary>
    /// Сдвинуть змею на новые кооординаты головы
    /// </summary>
    /// <param name="headPosition">Новая позиция головы</param>
    /// <param name="isChomp">Ела ли змея "яблоко"</param>
    public void Move(SnekSegment headPosition, bool isChomp)
    {
        body.AddFirst(headPosition);
        if (!isChomp) body.RemoveLast(); // Если змея съела "яблоко", подчищаем на один сегмент меньше
    }
}

// Класс сегмента тела змеи
public class SnekSegment
{
    public int _x { get; set; }
    public int _y { get; set; }

    public SnekSegment(int x, int y) { _x = x; _y = y; }
    public SnekSegment(SnekSegment snek) { _x = snek._x; _y = snek._y; }

    public SnekSegment Clone() => new SnekSegment(_x, _y);
}

