using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using static SnakeGame.SnekEngine.Abstractions.GameEnums;

namespace SnakeGame.SnekEngine.Abstractions.Models
{
    /// <summary>
    /// Начальные настройки игрового раунда
    /// </summary>
    public partial class InitialSettings : ObservableObject
    {
        [ObservableProperty]
        public int _seed;                               // "Зерно" для детерминированного рандома (если оставить пустым, берутся тики в мс с момента запуска приложения)
        [ObservableProperty]
        public int _rows;                            // Ширина поля, одна единица - одна клетка
        [ObservableProperty]
        public int _cols;                          // Высота поля, аналогично
        [ObservableProperty]
        public int _snakeSpawnPointI;                 // Точка старта для змеи, высота
        [ObservableProperty]
        public int _snakeSpawnPointJ;                // Точка старта для змеи, ширина
        [ObservableProperty]
        public Direction _firstDirection;             // Первоначальное направление движения змеи при старте раунда
        [ObservableProperty]
        public int _bombsCount = 0;                    // Количество генериуемых бомб
        [ObservableProperty]
        public bool _customWalls = false;              // Если false, рисуем стены по периметру
        [ObservableProperty]
        public HashSet<(int i, int j)> _walls = new(); // Если стены были расставлены вручную, иначе просто по периметру поля (на будущее)
    }
}
