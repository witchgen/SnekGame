using ShimSkiaSharp;
using SnakeGame.Models.LegacyGame.GameInfo;
using System;
using System.Collections.Generic;

namespace SnakeGame.SnekEngine.Abstractions.Models
{
    /// <summary>
    /// Наша змея. Состоит из связанных сегментов, при копировании состояние предпочтительно использовать метод <see cref="GetAsClone()"/>
    /// </summary>
    public class Snake : ICloneable
    {
        public LinkedList<SnakeSegment> Body = new();

        private Snake() { }

        public Snake(SnakeSegment head)
        {
            for (int x = 0; x < 3; x++) // У стартовой змеюки три сегмента
            {
                Body.AddFirst(head);
            }
        }

        /// <summary>
        /// Возвращаем новый объект змеи, идентичный оригиналукопия оригинала
        /// <returns></returns>
        public object Clone()
        {
            var clone = new Snake();
            foreach (var segment in Body)
            {
                clone.Body.AddLast(segment.Clone());
            }
            return clone;
        }

        /// <summary>
        /// Обертка для удобства
        /// </summary>
        /// <returns></returns>
        public Snek GetAsClone() => (Snek)Clone();

        /// <summary>
        /// Сдвинуть змею на новые кооординаты головы
        /// </summary>
        /// <param name="headPosition">Новая позиция головы</param>
        /// <param name="isChomp">Ела ли змея "яблоко"</param>
        public void Move((int i, int j) headPosition, bool isChomp)
        {
            Body.AddFirst(new SnakeSegment(headPosition));
            if (!isChomp) Body.RemoveLast(); // Если змея съела "яблоко", подчищаем на один сегмент меньше
        }

        /// <summary>
        /// Отдельный сегмент тела змеи, имеет координаты высота-широта
        /// </summary>
        public class SnakeSegment
        {
            // Высота (строка)
            public int i;
            // Ширина (столбец)
            public int j;

            public SnakeSegment((int i, int j) coords) { i = coords.i; j = coords.j; }
            public SnakeSegment(SnakeSegment snake) { i = snake.i; j = snake.j; }

            public SnakeSegment Clone() => new SnakeSegment((i, j));
        }
    }
}
