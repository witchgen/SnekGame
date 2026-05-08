using SnakeGame.SnekEngine.Abstractions.Interfaces;
using SnakeGame.SnekEngine.Abstractions.Models;
using System;
using System.Collections.Generic;
using static SnakeGame.Custom.CustomExceptions;

namespace SnakeGame.SnekEngine.World
{
    public class FieldInitializer : IFieldInitializer
    {
        private Random _rnd;
        public FieldInitializer() { }

        public GameSnapshot InitializeField(InitialSettings settings, Random rnd)
        {
            int rows = settings.Rows;
            int cols = settings.Cols;

            _rnd = rnd;

            int[,] field = new int[rows, cols];

            // 1. Стены
            if (settings.CustomWalls)
                SetCustomWalls(field, settings.Walls);
            else
                SetBasicWalls(field);

            // 2. Змея
            var snake = new Snake(new Snake.SnakeSegment((settings.SnakeSpawnPointI, settings.SnakeSpawnPointJ)));
            field[settings.SnakeSpawnPointI, settings.SnakeSpawnPointJ] = 2;

            // 3. Яблоко
            var apple = PlaceApple(field);

            HashSet<(int i, int j)> bombs = null;

            // 4. Бомба (если включено)
            if (settings.BombsCount > 0)
                bombs = PlaceInitialBombs(field, settings.BombsCount);

            return new GameSnapshot
            {
                Field = field,
                CurrentSnake = snake,
                Apple = apple,
                Bombs = bombs,
                CurrentScore = 0
            };
        }

        private void SetBasicWalls(int[,] field)
        {
            int rows = field.GetLength(0);
            int cols = field.GetLength(1);

            for (int i = 0; i < rows; i++)
            {
                field[i, 0] = 1;
                field[i, cols - 1] = 1;
            }

            for (int j = 0; j < cols; j++)
            {
                field[0, j] = 1;
                field[rows - 1, j] = 1;
            }
        }

        private void SetCustomWalls(int[,] field, HashSet<(int i, int j)> walls)
        {
            foreach (var (i, j) in walls)
                field[i, j] = 1;
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

        private HashSet<(int i, int j)>? PlaceInitialBombs(int[,] field, int amount)
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
