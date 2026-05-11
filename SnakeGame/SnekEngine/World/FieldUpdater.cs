using SnakeGame.SnekEngine.Abstractions.Interfaces;
using SnakeGame.SnekEngine.Abstractions.Models;
using System;
using System.Collections.Generic;
using static SnakeGame.Custom.CustomExceptions;
using static SnakeGame.SnekEngine.Abstractions.GameEnums;

namespace SnakeGame.SnekEngine.World
{
    internal class FieldUpdater : IFieldUpdater
    {
        public FieldUpdater() { }

        public GameSnapshot UpdateField(GameSnapshot snapshot, Direction direction, Random rnd)
        {
            var field = snapshot.Field;
            var snake = snapshot.Snake;
            var head = snake.Body.First.Value;

            // 1. Новая позиция головы
            (int i, int j) newHead = (0, 0);
            CellContent nextCell = CellContent.Empty;
            try
            {
                newHead = MoveHead(head, direction);

                nextCell = CheckAhead(field, newHead);
            }
            catch // Ловим лицом границу поля?
            {
                nextCell = CellContent.Wall;
            }

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
                snapshot.EndReason = GameOverReason.Bomb;
                return snapshot;
            }

            bool ateApple = nextCell == CellContent.Apple;

            // 3. Перемещаем змею
            snake.Move(newHead, ateApple);

            if (ateApple)
            {
                snapshot.Score += 100;
                snapshot.Apple = PlaceApple(field, rnd); // новое яблоко
            }

            // 4. Бомба
            if (ateApple && snapshot.Bombs != null)
                snapshot.Bombs = PlaceBomb(field, snapshot.Bombs.Count, rnd);

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
            //Array.Clear(field, 0, field.Length);

            // СТЕНЫ РЕАЛИЗОВАТЬ НОРМАЛЬНО БЛЭТ
            for (int i = 0; i < field.GetLength(0); i++)
                for (int j = 0; j < field.GetLength(1); j++)
                    if (field[i, j] != 1) // не стена
                        field[i, j] = 0;

            foreach (var seg in snake.Body)
                field[seg.i, seg.j] = 2;

            field[apple.i, apple.j] = 3;

            if (bombs != null)
                foreach (var bomb in bombs)
                    field[bomb.i, bomb.j] = 4;
        }

        private (int i, int j) PlaceApple(int[,] field, Random rnd)
        {
            int rows = field.GetLength(0);
            int cols = field.GetLength(1);

            for (int attempt = 0; attempt < 1500; attempt++)
            {
                int i = rnd.Next(1, rows - 1);
                int j = rnd.Next(1, cols - 1);

                if (field[i, j] == 0)
                {
                    field[i, j] = 3;
                    return (i, j);
                }
            }

            throw new CantPlaceItemsException();
        }

        private HashSet<(int i, int j)>? PlaceBomb(int[,] field, int amount, Random rnd)
        {
            if (amount < 0 || amount > 15) return null;

            int rows = field.GetLength(0);
            int cols = field.GetLength(1);
            var bombs = new HashSet<(int i, int j)>();
            var maxAttempts = field.Length * 10;

            for (int attempt = 0; attempt <= maxAttempts; attempt++)
            {
                int i = rnd.Next(1, rows - 1);
                int j = rnd.Next(1, cols - 1);

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
