using System;

namespace SnakeGame.Services;

public interface IFieldGenerator
{
    /// <summary>
    /// Инициализируем игровое поле
    /// </summary>
    /// <param name="size">Размер поля в клетках (без учета границ)</param>
    /// <returns>Двумерный квадратный массив, инициализированый игровыми элементами, размер size+1</returns>
    int[,] GetNewGameField(int size);

    public (int y, int x) GetFirstApplePosition();
    /// <summary>
    ///  Первичная позиция змеи
    /// </summary>
    /// <param name="size">Размер поля, куда помещаем змеюку</param>
    /// <returns></returns>
    int SetInitialSnakePosition(int size);
}

class FieldGenerator : IFieldGenerator
{
    private static int _size;
    private (int y, int x) _apple = (1, 1);
    private readonly Random _rnd = new();

    public int[,] GetNewGameField(int size)
    {
        _size = size;

        return GenerateFieldArray();
    }

    private int[,] GenerateFieldArray()
    {
        var edge = _size + 1;

        int[,] field = new int[edge, edge];

        for (int i = 0; i < edge; i++)
        {
            for (int j = 0; j < edge; j++)
            {
                if ((i == 0 || j == 0) || (i == _size || j == _size))
                {
                    field[i, j] = 1;
                }
                else field[i, j] = 0;
            }
        }


        var snakeStart = SetInitialSnakePosition(_size);

        field[snakeStart, snakeStart] = 5;

        _apple = SetFirstAppleCoords(field);

        field[_apple.Item1, _apple.Item2] = 3;

        return field;
    }

    private (int, int) SetFirstAppleCoords(int[,] field)
    {
        var x = _rnd.Next(1, _size);
        var y = _rnd.Next(1, _size);

        return field[y, x] == 2 || field[y, x] == 3 ?
        SetFirstAppleCoords(field) : (y, x);
    }

    public (int y, int x) GetFirstApplePosition()
    {
        return _apple;
    }

    public int SetInitialSnakePosition(int size)
    {
        var isEven = size % 2 == 0;

        return isEven ? size / 2 : size / 2 + 1;
    }
}
