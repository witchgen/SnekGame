using SnakeGame.Models.GameInfo;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static SnakeGame.Custom.CustomExceptions;
using static SnakeGame.Models.GameInfo.Enums;

namespace SnakeGame.Services
{
    public interface IBigThinkSnakeService
    {
        Direction CalculateNextMove(GameState state);
    }

    internal class BigThinkSnakeService : IBigThinkSnakeService
    {
        GameState _state;
        private readonly Random _rnd = new();

        static readonly (int dy, int dx)[] dirs =
        {
            (-1, 0), // вверх
            (1, 0),  // вниз
            (0, -1), // налево
            (0, 1)   // направо
        };

        public Direction CalculateNextMove(GameState state)
        {
            _state = state;
            // считаем следующую клетку для головы
            var head = state.SnakeHeadPosition;
            var nextCell = MoveSmartSnake(); // координаты следующей клетки

            if (nextCell.y == head._y && nextCell.x == head._x)
                return state.CurrentDirection;

            return GetDirection(head._y, head._x, nextCell.y, nextCell.x);
        }

        /// <summary>
        /// Полная логика ИИ, как раньше: всё в координатах
        /// </summary>
        private (int y, int x) MoveSmartSnake()
        {
            // 1. BFS к яблоку
            var pathToApple = BFSforTarget(_state.ApplePosition, toApple: true);
            if (pathToApple.Count > 0)
                return pathToApple.Dequeue();

            // 2. BFS к хвосту (выжидание)
            var tail = _state.SolidSnake.body.Last.Value;
            var pathToTail = BFSforTarget((tail._y, tail._x), toApple: false);
            if (pathToTail.Count > 0)
                return pathToTail.Dequeue();

            // 3. DFS fallback — самый длинный безопасный путь
            var survivalPath = JustStayAliveDFS();
            if (survivalPath.Count > 0)
                return survivalPath.Dequeue();

            // 4. Рандомный безопасный ход
            return GetRandomSafeMove();

            //var head = _state.SnakeHeadPosition;

            //Debug.WriteLine("ИИ пересчитал направление!");

            //// 1. Если старый путь ещё актуален — продолжаем по нему
            //if (_currentAIPath.Count > 0)
            //{
            //    var nextStep = _currentAIPath.Peek();
            //    if (IsSafeToMoveHere(_state, nextStep.y, nextStep.x) &&
            //        IsMoveSafeLongTerm(_state, nextStep.y, nextStep.x))
            //    {
            //        _currentAIPath.Dequeue();
            //        return nextStep;
            //    }

            //    _currentAIPath.Clear();
            //}

            //// 2. BFS к яблоку
            //_currentAIPath = BFSforTarget(_state, _state.ApplePosition, true);
            //if (_currentAIPath.Count > 0)
            //    return _currentAIPath.Dequeue();

            //// 3. BFS к хвосту
            //var tail = _state.SolidSnake.body.Last.Value;
            //_currentAIPath = BFSforTarget(_state, (tail._y, tail._x), false);
            //if (_currentAIPath.Count > 0)
            //    return _currentAIPath.Dequeue();

            //// 4. DFS fallback — ищем самый длинный безопасный путь
            //_currentAIPath = JustStayAliveDFS(_state);
            //if (_currentAIPath.Count > 0)
            //    return _currentAIPath.Dequeue();

            //// 5. Рандомный безопасный ход
            //return GetRandomSafeMove(_state);
        }

        private bool IsMoveSafeLongTerm(int ny, int nx)
        {
            return FloodFillCount(ny, nx) >= _state.SolidSnake.body.Count;
        }

        private bool IsMoveSafeWhenGoingForApple(int ny, int nx)
        {
            // +1: после поедания яблока змея вырастет на 1 сегмент
            return FloodFillCount(ny, nx) >= _state.SolidSnake.body.Count + 1;
        }

        private int FloodFillCount(int sy, int sx)
        {
            var q = new Queue<(int y, int x)>();
            var visited = new HashSet<(int y, int x)>();
            var field = _state.GameField;

            if (!Inside(sy, sx) || !IsSafeToMoveHere(sy, sx))
                return 0;

            q.Enqueue((sy, sx));
            visited.Add((sy, sx));

            while (q.Count > 0)
            {
                var (y, x) = q.Dequeue();

                foreach (var (dy, dx) in dirs)
                {
                    int ny = y + dy;
                    int nx = x + dx;

                    if (!Inside(ny, nx)) continue;
                    if (!IsSafeToMoveHere(ny, nx)) continue;
                    if (visited.Contains((ny, nx))) continue;

                    visited.Add((ny, nx));
                    q.Enqueue((ny, nx));
                }
            }

            return visited.Count;
        }

        private Queue<(int y, int x)> BFSforTarget((int y, int x) desiredCell, bool toApple)
        {
            var moveQueue = new Queue<(int y, int x)>();
            var head = _state.SnakeHeadPosition;

            moveQueue.Enqueue((head._y, head._x));

            var cameFrom = new Dictionary<(int y, int x), (int y, int x)>();
            cameFrom[(head._y, head._x)] = (-1, -1);

            var field = _state.GameField;

            while (moveQueue.Count > 0)
            {
                var (y, x) = moveQueue.Dequeue();

                if (y == desiredCell.y && x == desiredCell.x)
                    return RetracePath(cameFrom, (y, x));

                foreach (var (dy, dx) in dirs)
                {
                    int ny = y + dy;
                    int nx = x + dx;

                    if (!Inside(ny, nx)) continue;
                    if (!IsSafeToMoveHere(ny, nx)) continue;
                    if (toApple)
                    {
                        // К яблоку — проверяем, что после поедания змее будет куда расти
                        if (!IsMoveSafeWhenGoingForApple(ny, nx)) continue;
                    }
                    else
                    {
                        // К хвосту — проверяем, что путь не ведёт в мгновенный тупик
                        if (!IsMoveSafeLongTerm(ny, nx)) continue;
                    }
                    if (cameFrom.ContainsKey((ny, nx))) continue;

                    cameFrom[(ny, nx)] = (y, x);
                    moveQueue.Enqueue((ny, nx));
                }
            }

            return new Queue<(int, int)>();
        }

        private Queue<(int, int)> RetracePath(
            Dictionary<(int y, int x), (int y, int x)> path,
            (int y, int x) target)
        {
            var reversePath = new Stack<(int y, int x)>();
            var current = target;

            while (path.ContainsKey(current))
            {
                var (y, x) = path[current];
                if (y == -1 && x == -1) break;
                reversePath.Push(current);
                current = (y, x);
            }

            var result = new Queue<(int, int)>();
            while (reversePath.Count > 0)
                result.Enqueue(reversePath.Pop());

            return result;
        }

        private Queue<(int y, int x)> JustStayAliveDFS(int maxDepth = 8)
        {
            var best = new List<(int y, int x)>();
            var visited = new HashSet<(int y, int x)>();

            var head = _state.SnakeHeadPosition;
            var field = _state.GameField;

            void dfs((int y, int x) pos, List<(int y, int x)> path, int depth)
            {
                // Ограничение глубины
                if (depth >= maxDepth)
                {
                    if (path.Count > best.Count)
                        best = new List<(int y, int x)>(path);
                    return;
                }

                visited.Add(pos);
                path.Add(pos);

                bool deadEnd = true;

                foreach (var (dy, dx) in dirs)
                {
                    int ny = pos.y + dy;
                    int nx = pos.x + dx;

                    if (!Inside(ny, nx)) continue;
                    if (!IsSafeToMoveHere(ny, nx)) continue;
                    if (visited.Contains((ny, nx))) continue;

                    deadEnd = false;
                    dfs((ny, nx), path, depth+1); // Увеличиваем глубину до максимального значения
                }

                if (deadEnd && path.Count > best.Count)
                    best = new List<(int y, int x)>(path);

                path.RemoveAt(path.Count - 1);
                visited.Remove(pos);
            }

            dfs((head._y, head._x), new List<(int y, int x)>(), 0);

            return new Queue<(int y, int x)>(best.Skip(1));
        }

        private (int y, int x) GetRandomSafeMove()
        {
            var head = _state.SnakeHeadPosition;
            var candidates = new List<(int y, int x)>();

            foreach (var (dy, dx) in dirs)
            {
                int ny = head._y + dy;
                int nx = head._x + dx;

                if (!Inside(ny, nx)) continue;
                if (!IsSafeToMoveHere(ny, nx)) continue;

                candidates.Add((ny, nx));
            }

            if (candidates.Count == 0)
            {
                // Безвыходная ситуация: идём вперёд по текущему курсу до гибели
                var (dy, dx) = _state.CurrentDirection switch
                {
                    Direction.Up => (-1, 0),
                    Direction.Down => (1, 0),
                    Direction.Left => (0, -1),
                    Direction.Right => (0, 1),
                    _ => (0, 0)
                };
                return (head._y + dy, head._x + dx);
            }

            return candidates[_rnd.Next(candidates.Count)];
        }

        private bool Inside(int y, int x)
        {
            var f = _state.GameField;
            return y >= 0 && y < f.GetLength(0) && x >= 0 && x < f.GetLength(1);
        }

        private bool IsSafeToMoveHere(int y, int x)
        {
            var snakeTail = _state.SolidSnake.body.Last.Value;
            if (y == snakeTail._y && x == snakeTail._x)
                return true;

            return _state.GameField[y, x] is not (1 or 2 or 5 or 7);
        }

        private static Direction GetDirection(int y, int x, int ny, int nx)
        {
            if (ny == y - 1 && nx == x) return Direction.Up;
            if (ny == y + 1 && nx == x) return Direction.Down;
            if (ny == y && nx == x - 1) return Direction.Left;
            if (ny == y && nx == x + 1) return Direction.Right;

            throw new CanNotSetDirectionException("Invalid direction for AI path");
        }
    }
}
