using SnakeGame.SnekEngine.Abstractions.Interfaces;
using SnakeGame.SnekEngine.Abstractions.Models;
using System;
using System.Collections.Generic;
using static SnakeGame.Custom.CustomExceptions;
using static SnakeGame.SnekEngine.Abstractions.GameEnums;

namespace SnakeGame.SnekEngine.Core.Services
{
    internal class FieldUpdater : IFieldUpdater
    {
        private readonly Random _rnd;

        public FieldUpdater(Random rnd)
        {
            _rnd = rnd;
        }

        public GameSnapshot UpdateField(GameSnapshot snapshot, Direction direction)
        {
            var field = snapshot.Field;
            var snake = snapshot.CurrentSnake;
            var head = snake.Body.First.Value;

            // 1. Новая позиция головы
            var newHead = MoveHead(head, direction);

            var nextCell = CheckAhead(field, newHead);

            // 2. Проверка столкновений
            if (nextCell == CellContent.Wall)
            {
                snapshot.EndReason = GameOverReason.Wall;
                return snapshot;
            }
            else if(nextCell == CellContent.Snake)
            {
                snapshot.EndReason = GameOverReason.BitTail;
                return snapshot;
            }
            else if(nextCell == CellContent.Bomb)
            {
                snapshot.EndReason |= GameOverReason.Bomb;
                return snapshot;
            }

            bool ateApple = nextCell == CellContent.Apple;

            // 3. Перемещаем змею
            snake.Move(newHead, ateApple);

            if (ateApple)
                snapshot.Apple = PlaceApple(field); // новое яблоко

            // 4. Бомба
            if (ateApple && snapshot.Bombs != null)
                snapshot.Bombs = PlaceBomb(field, snapshot.Bombs.Count);

            // 5. Обновляем поле
            UpdateFieldArray(field, snake, snapshot.Apple, snapshot.Bombs);

            return snapshot;
        }

        private (int i, int j) MoveHead(Snake.SnakeSegment head, Direction dir)
        {
            return dir switch
            {
                Direction.Up => (head.i - 1, head.j),
                Direction.Down => (head.i + 1, head.j),
                Direction.Left => (head.i, head.j - 1),
                Direction.Right => (head.i, head.j + 1),
                _ => (head.i, head.j)
            };
        }

        private CellContent CheckAhead(int[,] field, (int i, int j) pos)
        {
            int cell = field[pos.i, pos.j];
            switch (cell)
            {
                case 0: return CellContent.Empty;
                case 1: return CellContent.Wall;
                case 2: return CellContent.Snake;
                case 3: return CellContent.Apple;
                case 4: return CellContent.Bomb;
                default: return CellContent.Wall;
            };
        }

        private void UpdateFieldArray(int[,] field, Snake snake, (int i, int j) apple, HashSet<(int i, int j)>? bombs)
        {
            Array.Clear(field, 0, field.Length);

            foreach (var seg in snake.Body)
                field[seg.i, seg.j] = 2;

            field[apple.i, apple.j] = 3;

            if (bombs != null)
                foreach (var bomb in bombs)
                    field[bomb.i, bomb.j] = 4;
        }

        private (int i, int j) PlaceApple(int[,] field)
        {
            int rows = field.GetLength(0);
            int cols = field.GetLength(1);

            for (int attempt = 0; attempt < 1500; attempt++)
            {
                int i = _rnd.Next(1, rows - 1);
                int j = _rnd.Next(1, cols - 1);

                if (field[i, j] == 0)
                {
                    field[i, j] = 3;
                    return (i, j);
                }
            }

            throw new CantPlaceItemsException();
        }

        private HashSet<(int i, int j)>? PlaceBomb(int[,] field, int amount)
        {
            if (amount < 0 || amount > 15) return null;

            int rows = field.GetLength(0);
            int cols = field.GetLength(1);
            var bombs = new HashSet<(int i, int j)>();
            var maxAttempts = field.Length * 10;

            for (int attempt = 0; attempt <= maxAttempts; attempt++)
            {
                int i = _rnd.Next(1, rows - 1);
                int j = _rnd.Next(1, cols - 1);

                if (bombs.Count == amount) return bombs;

                if (field[i, j] == 0)
                {
                    field[i, j] = 4;
                    bombs.Add((i, j));
                }
            }
            throw new CantPlaceItemsException();
        }
    }
}
