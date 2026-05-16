using SkiaSharp;
using SnakeGame.SnekEngine.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SnakeGame.SnekEngine.Rendering
{
    /// <summary>
    /// Рендерер - "рисователь", отображает игровую графику по имеющемуся снимку логики (snapshot)
    /// Позволяет грузить нужные изображения и тайлы из встроенных ресурсов приложения
    /// Если ресурсов нет / не находит - рисует графические примитивы на их месте
    /// </summary>
    internal class GameRenderer
    {
        private readonly FieldRenderer _fieldR;
        private readonly SnakeRenderer _snakeR;
        private readonly float _cellSize;
        private List<(int i, int j)> _wallsCache = new();
        private float _pauseAnimTime = 0f; // Для анимации паузы
        private static readonly (int di, int dj)[] Directions =
        {
            (0, 1),   // Направо
            (1, 0),   // Вниз
            (0, -1),  // Налево
            (-1, 0)   // Вверх
        };

        public GameRenderer(int rows, int cols, float cellSize)
        {
            _cellSize = cellSize;
            _fieldR = new FieldRenderer(rows, cols, cellSize);
            _snakeR = new SnakeRenderer(_cellSize);
        }

        /// <summary>
        /// Отрисовка раунда (статика)
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="snapshot"></param>
        public void DrawStatic(SKCanvas canvas, GameSnapshot snapshot, bool isPaused)
        {
            canvas.Clear(SKColors.Transparent);
            _wallsCache.Clear();

            var cellImage = GetImageFromGameResource("hedge_cell.png");
            if (cellImage == null)
            {
                _fieldR.DrawField(canvas);
                _fieldR.DrawCells(canvas);
            }
            else
            {
                _fieldR.DrawFieldCells(canvas, cellImage);
            }
            CacheWalls(snapshot.Field);
            DrawWallsGraphic(canvas, snapshot.Field);
            DrawAppleGraphic(canvas, snapshot.Apple);

            if (snapshot.Bombs != null)
                DrawBombGraphic(canvas, snapshot.Bombs);

            // Если игра на паузе, рисуем змею по последним позициям на поле
            if (isPaused)
                _snakeR.DrawStaticPause(canvas, snapshot.Snake);
            // Если новая игра, рисуем стартовую точку змеи
            else
                _snakeR.DrawStatic(canvas, snapshot.Snake);
        }

        /// <summary>
        /// Отрисовка раунда (в движении, геймплей)
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="previous"></param>
        /// <param name="current"></param>
        /// <param name="t">Дельта времени</param>
        public void Draw(SKCanvas canvas, GameSnapshot previous, GameSnapshot current, float t)
        {
            _wallsCache.Clear();
            canvas.Clear(SKColors.Transparent);
            var cellImage = GetImageFromGameResource("hedge_cell.png");
            if(cellImage == null)
            {
                _fieldR.DrawField(canvas);
                _fieldR.DrawCells(canvas);
            }
            else
            {
                _fieldR.DrawFieldCells(canvas, cellImage);
            }
            CacheWalls(current.Field);
            DrawWallsGraphic(canvas, current.Field);
            DrawAppleGraphic(canvas, current.Apple);
            if (current.Bombs != null)
                DrawBombGraphic(canvas, current.Bombs);

            _snakeR.Draw(canvas, previous.Snake, current.Snake, t);
        }

        // TODO: разместить это и геймовер в отдельном рендерере (OverlayRenderer)
        public void RenderPauseOverlay(SKCanvas canvas, float width, float height)
        {
            UpdatePauseAnimation();

            // Затемнение
            using var overlay = new SKPaint
            {
                Color = new SKColor(0, 0, 0, 110)
            };
            canvas.DrawRect(0, 0, width, height, overlay);

            // Анимация текста
            float scale = 1.0f + 0.1f * (float)Math.Sin(_pauseAnimTime * 3.5f);
            float fontSize = 84f * scale;

            using var font = new SKFont(SKTypeface.FromFamilyName(null, SKFontStyle.Bold), fontSize);
            using var textPaint = new SKPaint
            {
                Color = SKColors.PaleGoldenrod,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center,
                StrokeJoin = SKStrokeJoin.Round,  // Скруглённые углы
                StrokeCap = SKStrokeCap.Round     // Скруглённые концы линий
            };

            float x = width / 2f;
            float y = height / 2f + fontSize * 0.35f; // baseline fix

            canvas.DrawText("[ ПАУЗА ]", x, y, font, textPaint);
        }
                
        public void UpdatePauseAnimation()
        {
            _pauseAnimTime += 0.016f; // ~60 FPS
        }

        public void DrawResultsScreen(SKCanvas canvas, float width, float height, PlayInfo results)
        {
            var endgame = new ResultsRenderer(results);
            endgame.RenderEndScreen(canvas, width, height);
        }

        /// <summary>
        /// Упрощенное рисование стен (графический примитив-заглушка)
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="field"></param>
        private void DrawWallsBasic(SKCanvas canvas, int[,] field)
        {
            using var paint = new SKPaint
            {
                Color = new SKColor(80, 80, 80),
                IsAntialias = false,
                Style = SKPaintStyle.Fill
            };

            foreach (var wall in _wallsCache)
            {
                var path = new SKPath();
                path.AddRect(new SKRect(
                    wall.j * _cellSize,
                    wall.i * _cellSize,
                    (wall.j + 1) * _cellSize,
                    (wall.i + 1) * _cellSize
                ));

                canvas.DrawPath(path, paint);
            }
        }

        /// <summary>
        /// Рисуем стены вокруг поля
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="field"></param>
        private void DrawWallsGraphic(SKCanvas canvas, int[,] field)
        {
            var wallCorner = GetImageFromGameResource("hedge_wall_corner.png");
            var wallStraight = GetImageFromGameResource("hedge_wall_straight.png");
            if(wallCorner == null || wallStraight == null) // Если не находим ресурсов стены, рисуем заглушки
            {
                DrawWallsBasic(canvas, field);
            }
            else
            {
                using var paint = new SKPaint { IsAntialias = false };

                // 1. Собираем периметр
                var walls = _wallsCache.ToHashSet();
                var loop = TraceWallLoop(walls);

                // 2. Находим реальные углы по координатам
                var minI = loop.Min(w => w.i);
                var maxI = loop.Max(w => w.i);
                var minJ = loop.Min(w => w.j);
                var maxJ = loop.Max(w => w.j);

                foreach (var cell in loop)
                {
                    var rect = new SKRect(
                        cell.j * _cellSize,
                        cell.i * _cellSize,
                        (cell.j + 1) * _cellSize,
                        (cell.i + 1) * _cellSize
                    );

                    // 3. Определяем, является ли клетка углом
                    int cornerIndex = 0;

                    if (cell.i == minI && cell.j == minJ) cornerIndex = 1; // Левый верхний
                    else if (cell.i == minI && cell.j == maxJ) cornerIndex = 2; // Правый верхний
                    else if (cell.i == maxI && cell.j == maxJ) cornerIndex = 3; // Правый нижний
                    else if (cell.i == maxI && cell.j == minJ) cornerIndex = 4; // Левый нижний

                    if (cornerIndex != 0)
                    {
                        // 4. Применяем отражения по таблице
                        switch (cornerIndex)
                        {
                            case 1: // Левый верхний — как есть
                                DrawFlipped(canvas, wallCorner, rect, false, false);
                                break;

                            case 2: // Правый верхний — flipX
                                DrawFlipped(canvas, wallCorner, rect, true, false);
                                break;

                            case 3: // Правый нижний — flipX + flipY
                                DrawFlipped(canvas, wallCorner, rect, true, true);
                                break;

                            case 4: // Левый нижний — flipY
                                DrawFlipped(canvas, wallCorner, rect, false, true);
                                break;
                        }
                    }
                    else
                    {
                        // 5. Прямой сегмент — определяем ориентацию
                        var (prev, next) = GetNeighbors(loop, cell);
                        int dir = GetDirection(cell, next);

                        int rotation = dir switch
                        {
                            0 => 0,    // вправо
                            1 => 90,   // вниз
                            2 => 180,  // влево
                            3 => 270,  // вверх
                            _ => 0
                        };

                        DrawRotated(canvas, wallStraight, rect, rotation);
                    }
                }
            }
        }

        private ((int i, int j) prev, (int i, int j) next) GetNeighbors(List<(int i, int j)> loop, (int i, int j) cur)
        {
            int idx = loop.IndexOf(cur);
            var prev = loop[(idx - 1 + loop.Count) % loop.Count];
            var next = loop[(idx + 1) % loop.Count];
            return (prev, next);
        }

        /// <summary>
        /// Метод для отрисовки углов поля (крутим-вертим одно изображение угла по четырем направлениям)
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="img"></param>
        /// <param name="rect"></param>
        /// <param name="flipX"></param>
        /// <param name="flipY"></param>
        private void DrawFlipped(SKCanvas canvas, SKImage img, SKRect rect, bool flipX, bool flipY)
        {
            canvas.Save();

            canvas.Translate(rect.MidX, rect.MidY);
            canvas.Scale(flipX ? -1 : 1, flipY ? -1 : 1);
            canvas.Translate(-rect.MidX, -rect.MidY);

            canvas.DrawImage(img, rect);

            canvas.Restore();
        }

        /// <summary>
        /// Отрисовка тайлов стен с поворотом по направлению
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="img"></param>
        /// <param name="rect"></param>
        /// <param name="rotation"></param>
        private void DrawRotated(SKCanvas canvas, SKImage img, SKRect rect, int rotation)
        {
            canvas.Save();
            canvas.Translate(rect.MidX, rect.MidY);
            canvas.RotateDegrees(rotation);
            canvas.Translate(-rect.MidX, -rect.MidY);
            canvas.DrawImage(img, rect);
            canvas.Restore();
        }

        private List<(int i, int j)> TraceWallLoop(HashSet<(int i, int j)> walls)
        {
            var loop = new List<(int i, int j)>();

            var start = walls.OrderBy(w => w.i).ThenBy(w => w.j).First();
            var current = start;
            var prevDir = 0; // начинаем "вправо"

            do
            {
                loop.Add(current);

                bool moved = false;

                // пробуем повернуть направо > прямо > налево > назад
                for (int k = 0; k < 4; k++)
                {
                    int dir = (prevDir + k) % 4;
                    var (di, dj) = Directions[dir];
                    var next = (current.i + di, current.j + dj);

                    if (walls.Contains(next))
                    {
                        prevDir = dir;
                        current = next;
                        moved = true;
                        break;
                    }
                }

                if (!moved)
                    break;

            } while (current != start);

            return loop;
        }

        /// <summary>
        /// Вспомогательный метод получения направления (нужен для отрисовки тайлов стен)
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        private int GetDirection((int i, int j) from, (int i, int j) to)
        {
            int di = to.i - from.i;
            int dj = to.j - from.j;

            for (int d = 0; d < 4; d++)
                if (Directions[d] == (di, dj))
                    return d;

            return 0;
        }

        /// <summary>
        /// Берем нужную картинку по названию файла из ресурсов игры
        /// </summary>
        /// <param name="resourceName"></param>
        /// <returns></returns>
        private SKImage? GetImageFromGameResource(string resourceName)
        {
            var asm = typeof(App).Assembly;

            var imageResource = asm.GetManifestResourceNames()
                .Where(n => n.Contains("Game") && n.Contains(resourceName))
                .FirstOrDefault();

            using var stream = asm.GetManifestResourceStream(imageResource);
            if (stream == null)
                return null;

            var image = SKImage.FromEncodedData(stream);

            return image;
        }

        private void CacheWalls(int[,] field)
        {
            for (int i = 0; i < field.GetLength(0); i++)
            {
                for (int j = 0; j < field.GetLength(1); j++)
                {
                    if (field[i, j] == 1)
                        _wallsCache.Add((i, j));
                }
            }
        }

        /// <summary>
        /// Рисуем яблочко на поле
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="apple"></param>
        public void DrawAppleGraphic(SKCanvas canvas, (int i, int j) apple)
        {
            var appleImage = GetImageFromGameResource("apple.png");
            if(appleImage == null)
            {
                DrawAppleBasic(canvas, apple);
            }
            else
            {
                float scale = 1.3f; // яблоко на 30% больше клетки
                float appleSize = _cellSize * scale;
                float offset = (appleSize - _cellSize) / 2f;

                using var paint = new SKPaint
                {
                    IsAntialias = false
                };

                var rect = new SKRect(
                    apple.j * _cellSize - offset,
                    apple.i * _cellSize - offset,
                    apple.j * _cellSize - offset + appleSize,
                    apple.i * _cellSize - offset + appleSize
                );

                canvas.DrawImage(appleImage, rect, paint);
            }
        }

        /// <summary>
        /// Примитив-заглушка яблока
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="apple"></param>
        private void DrawAppleBasic(SKCanvas canvas, (int i, int j) apple)
        {
            var paint = new SKPaint
            {
                Color = SKColors.IndianRed,
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };

            float cx = apple.j * _cellSize + _cellSize / 2;
            float cy = apple.i * _cellSize + _cellSize / 2;
            float radius = _cellSize * 0.35f;

            var path = new SKPath();
            path.AddCircle(cx, cy, radius);

            canvas.DrawPath(path, paint);
        }

        /// <summary>
        /// Рисуем бомбы на поле
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="bombs"></param>
        private void DrawBombGraphic(SKCanvas canvas, HashSet<(int i, int j)> bombs)
        {
            var bombImage = GetImageFromGameResource("bomb.png");
            if (bombImage == null)
            {
                DrawBombBasic(canvas, bombs);
            }
            else
            {
                float scale = 1.3f; // на 30% больше клетки
                float bombSize = _cellSize * scale;
                float offset = (bombSize - _cellSize) / 2f;

                using var paint = new SKPaint
                {
                    IsAntialias = false
                };

                foreach(var bomb in bombs)
                {
                    var rect = new SKRect(
                    bomb.j * _cellSize - offset,
                    bomb.i * _cellSize - offset,
                    bomb.j * _cellSize - offset + bombSize,
                    bomb.i * _cellSize - offset + bombSize
                );

                    canvas.DrawImage(bombImage, rect, paint);
                }
            }
        }

        /// <summary>
        /// Примитив-заглушка для бомб
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="bombs"></param>
        private void DrawBombBasic(SKCanvas canvas, HashSet<(int i, int j)> bombs)
        {
            using var paint = new SKPaint
            {
                Color = SKColors.Black,
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };

            foreach(var bomb in bombs)
            {
                float cx = bomb.j * _cellSize + _cellSize / 2;
                float cy = bomb.i * _cellSize + _cellSize / 2;
                float r = _cellSize * 0.35f;

                var path = new SKPath();
                path.AddCircle(cx, cy, r);

                canvas.DrawPath(path, paint);
            }
        }
    }
}
