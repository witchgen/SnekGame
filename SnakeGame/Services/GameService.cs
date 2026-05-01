//using Android.OS;
using SnakeGame.Custom;
using SnakeGame.Models.GameInfo;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SnakeGame.Custom.CustomExceptions;
using static SnakeGame.Models.GameInfo.Enums;

namespace SnakeGame.Services;

public interface IGameService
{
    event EventHandler<string>? FieldUpdated;
    /// <summary>
    /// Состояние игрового процесса
    /// </summary>
    GameStatus Status { get; }
    /// <summary>
    /// Инициализировать новую игру по размеру поля
    /// </summary>
    /// <param name="size">Размер поля (без учета границ)</param>
    void InitializeNewGame(int size);
    /// <summary>
    /// Выставить сложность
    /// </summary>
    /// <param name="lvl">Желаемая сложность</param>
    void SetDifficulty(Difficulty lvl);
    /// <summary>
    /// Начать игру в змейку
    /// </summary>
    /// <param name="state">Начальное состояние, нужно задать имя игрока</param>
    /// <returns>Статус состояния игры</returns>
    Task<GameStatus> StartNewGame(GameState state);
    /// <summary>
    /// Продолжить игру с паузы
    /// </summary>
    /// <param name="state">Хранимое состояние</param>
    /// <returns>Статус состояния игры</returns>
    Task<GameStatus> ContinueGame(GameState state);
    /// <summary>
    /// Сменить направление змеи
    /// </summary>
    void ChangeDirection(Direction dir);
    /// <summary>
    /// Получить текущий счет игрока
    /// </summary>
    /// <returns></returns>
    string GetCurrentScore();
    /// <summary>
    /// Сбросить игру
    /// </summary>
    void ResetGame();
    /// <summary>
    /// Поставить игру на паузу
    /// </summary>
    void PauseGame();
    /// <summary>
    /// Получить сохраненное состояние игрового сеанса
    /// </summary>
    /// <returns></returns>
    GameState GetGameState();
    /// <summary>
    /// Переключаем опции отладки во время игрового процесса
    /// </summary>
    /// <param name="option">Нужная опция <see cref="DebugOption"/> (будет переключена)</param>
    void ToggleDebugOption(DebugOption option);
}

public class GameService : IGameService
{
    private IFieldGenerator _generator = new FieldGenerator();

    private int _score = 0; // Счет
    private int _size; // Размер поля (без границ)
    private SnekSegment _actualHead; // Хранимая позиция головы змеи
    private Random _rnd = new(); // Бог рандома
    private Direction _direction; // Хранимое направление
    private Direction? _pendingDirection = null; // Последнее полученное от игрока направление
    public int[,] _field; // Хранимое состояние игрового поля
    public (int, int) _nextBomb; // Следующая позиция бомбы
    private bool _boom = false; // Взорвались?
    private bool _canPlaceBombs = true; // Можем ли спавнить бомбы (если закончилось место)

    // Режим отладки (флаги):
    private bool _highlightBombSpawnArea = false;
    public bool _snakeAiControlled = false;
    // Путь ИИ:
    private Queue<(int y, int x)> _currentAIPath = new();

    private GameState _gameState;

    // Событие для передачи обновлений поля
    public event EventHandler<string>? FieldUpdated;
    public GameStatus Status { get; private set; } = GameStatus.Initialized;

    private int _speedMs = 500; // Скорость игры (медленная по дефолту)
    private int _diffMultiplier = 1; // Хранимая сложность (низкая по дефолту)

    // Рисуем игровое поле
    private string DrawField()
    {
        var sb = new StringBuilder();

        for (int i = 0; i <= _size; i++)
        {
            for (int j = 0; j <= _size; j++)
            {
                var symbol = _field[i, j].ToEmoji();
                sb.Append(symbol);
            }
            sb.Append(Environment.NewLine);
        }

        return sb.ToString();
    }

    public void SetDifficulty(Difficulty lvl)
    {
        switch (lvl)
        {
            case Difficulty.Easy: _speedMs = 400; break;
            case Difficulty.Medium: _speedMs = 300; break;
            case Difficulty.Hard:
                _speedMs = 200; break;
            default: _speedMs = 500; break;
        }

        _diffMultiplier = (int)lvl;
    }

    // Если хотим задать размер поля (не вся текущая логика адаптирована!)
    public void InitializeNewGame(int size)
    {
        _size = size;
        ResetGame();
    }

    // Первичная инициализация
    public void ResetGame()
    {
        _field = _generator.GetNewGameField(_size);
        _currentAIPath.Clear();

        _gameState = new GameState() {
            CurrentGameData = new PlayData()
        };

        _score = 0;
        var initPos = _generator.SetInitialSnakePosition(_size);
        
        

        _actualHead = new SnekSegment(initPos, initPos);
        _nextBomb = (1, 1);
        _boom = false;

        FieldUpdated?.Invoke(this, DrawField());
    }

    public void PauseGame()
    {
        Status = GameStatus.Paused;
    }

    public GameState GetGameState()
    {
        return _gameState;
    }

    // Точка входа для игрового процесса новой игры, задаем новое состояние
    public async Task<GameStatus> StartNewGame(GameState state)
    {
        _gameState = state;

        _gameState.SolidSnake = new Snek(_actualHead);
        _direction = Direction.Up;
        ChangeDirection(_direction);
        _gameState.SnakeHeadPosition = _actualHead;
        _gameState.BombPosition = (1, 1);
        // Ищем на поле первое яблоко
        for (int i = 1; i < _size; i++)
        {
            for (int j = 0; j < _size; j++)
            {
                if (_field[i, j] == 3)
                    _gameState.ApplePosition = (i, j);
            }
        }

        Status = GameStatus.Running;
        return await GameLoop();
    }

    // При продолжении с паузы подгружаем сохраненное состояние
    public async Task<GameStatus> ContinueGame(GameState state)
    {
        _gameState = state;
        Status = GameStatus.Running;
        return await GameLoop();
    }

    // Основной игровой цикл змейки
    private async Task<GameStatus> GameLoop()
    {
        while (Status == GameStatus.Running)
        {
            var result = Tick();

            FieldUpdated?.Invoke(this, DrawField());
            

            if (result == GameStatus.Ended)
            {
                PaintTheTownRed(_gameState.SolidSnake);

                FieldUpdated?.Invoke(this, DrawField());

                _gameState.CurrentGameData = new PlayData
                {
                    PlayerName = string.IsNullOrWhiteSpace(_gameState.CurrentGameData.PlayerName) ? "Анонимус" : _gameState.CurrentGameData.PlayerName,
                    DifficultyLevel = (Difficulty)_diffMultiplier,
                    Score = _score,
                    DeathReason = _snakeAiControlled ? GameOverReason.AIsucker : _gameState.CurrentGameData.DeathReason,
                    MaxSnakeLength = _gameState.SolidSnake.body.Count
                };

                Status = GameStatus.Ended;
                return GameStatus.Ended;
            }

            await Task.Delay(_speedMs);
        }

        return GameStatus.Paused;
    }

    // Один момент игрового процесса
    private GameStatus Tick()
    {
        var nextHead = new SnekSegment(_actualHead);
        _gameState.CurrentGameData.WasSnakeAIControlled = _snakeAiControlled;
        Direction nextDirection = _direction;

        if (!_snakeAiControlled)
        {
            nextDirection = GetPendingSnakeDirection();
            ChangeDirection(nextDirection);

            switch (nextDirection)
            {
                case Direction.Up: nextHead._y--; break;
                case Direction.Down: nextHead._y++; break;
                case Direction.Left: nextHead._x--; break;
                case Direction.Right: nextHead._x++; break;
            }
        }
        else // Если включена отладка, змея ползает сама
        {
            nextHead = MoveSmartSnake();
        }

        if (IsBombAhead(nextHead)) // Напоролись на бомбу
        {
            _boom = true;
            _gameState.CurrentGameData.DeathReason = GameOverReason.Bomb;
            return GameStatus.Ended;
        }

        if (IsOutOfBounds(nextHead)) // Вошли в стену
        {
            _gameState.CurrentGameData.DeathReason = GameOverReason.Wall;
            return GameStatus.Ended;
        }

        if (HasCollidedWithItself(nextHead)) // Съели себя же
        {
            _gameState.CurrentGameData.DeathReason = GameOverReason.BitTail;
            return GameStatus.Ended;
        }

        bool fed = IsMunchyAhead(nextHead); // Скушали "яблочко"

        if (fed)
        {
            _currentAIPath.Clear(); // Сброс пути для ИИ змеи, если "яблоко" съедено

            if (_diffMultiplier > 0) // Если сложность выше легкой, с яблоком генерируется новая бомба...
            {
                _field[_gameState.BombPosition.Item1, _gameState.BombPosition.Item2] = 0;
                _nextBomb = GetBombCoords();
                if(_canPlaceBombs) // ... но только пока есть место
                {
                    _gameState.BombPosition = _nextBomb;
                    _field[_nextBomb.Item1, _nextBomb.Item2] = 7;
                }
            }

            _score += 100 + (_diffMultiplier * 50);

            try
            {
                var newApple = GetAppleCoords();
                _gameState.ApplePosition = newApple;
                _field[newApple.Item1, newApple.Item2] = 3;
            }
            catch
            {
                _gameState.CurrentGameData.DeathReason = GameOverReason.Victory;
                return GameStatus.Ended;
            }
        }

        _direction = nextDirection;

        _actualHead = nextHead;
        _gameState.SnakeHeadPosition = _actualHead;

        _gameState.SolidSnake.Move(_actualHead, fed);

        SyncFieldWithSnake(_gameState.SolidSnake);

        return GameStatus.Running;
    }

    private SnekSegment MoveSmartSnake()
    {
        // Если старый путь ещё актуален — продолжаем по нему
        if (_currentAIPath.Count > 0)
        {
            var nextStep = _currentAIPath.Peek();
            // Проверяем, не стала ли следующая клетка препятствием
            if (IsSafeToMoveHere(nextStep.y, nextStep.x))
            {
                _currentAIPath.Dequeue();

                return new SnekSegment(nextStep.y, nextStep.x);
            }
            // Если путь заблокирован — сбрасываем и ищем новый
            _currentAIPath.Clear();
        }

        // Ищем новый путь
        _currentAIPath = BFSforTarget(_gameState.ApplePosition);

        if (_currentAIPath.Count > 0)
        {
            var step = _currentAIPath.Dequeue();
            return new SnekSegment(step.y, step.x);
        }

        // Запасной вариант — идём к хвосту
        var snekTail = _gameState.SolidSnake.body.Last.Value;
        _currentAIPath = BFSforTarget((snekTail._y, snekTail._x));

        if (_currentAIPath.Count > 6) // Если путь к хвосту слишком короткий, идем рандомными мувами чтобы с меньшей вероятностью врезаться
        {            
            var step = _currentAIPath.Dequeue();
            return new SnekSegment(step.y, step.x);
        }

        //Если и до хвоста не можем найти путь, то делаем случайный шаг в сторону и молимся богу рандома
        var nextHead = GetRandomSafeMove();
        return nextHead;
    }

    private SnekSegment GetRandomSafeMove()
    {
        var safeMoves = new List<SnekSegment>();

        foreach (var (dy, dx) in dirs)
        {
            int ny = _actualHead._y + dy;
            int nx = _actualHead._x + dx;

            if (IsSafeToMoveHere(ny, nx))
                safeMoves.Add(new SnekSegment(ny, nx));
        }

        // Если есть безопасные варианты — выбираем случайный
        if (safeMoves.Count > 0)
            return safeMoves[_rnd.Next(safeMoves.Count)];

        // Если вариантов нет, ползем вперед - на верную гибель
        var (fdy, fdx) = dirs[(int)_direction];
        return new SnekSegment(_actualHead._y + fdy, _actualHead._x + fdx);
    }

    /// <summary>
    /// Проверяем ближайших соседей клекти в четырех направлениях
    /// </summary>
    /// <returns>true если встречено препятствие</returns>
    private bool IsSafeToMoveHere(int y, int x)
    {
        return _field[y, x] is not (1 or 2 or 5 or 7);
    }

    static readonly (int dy, int dx)[] dirs =
    {
        (-1, 0), // верхняя клетка
        (1, 0),  // нижняя клетка
        (0, -1), // левая клетка
        (0, 1)   // правая клетка
    };

    /// <summary>
    /// Ищем путь к указанной цели
    /// </summary>
    /// <param name="desiredCell">Координаты, которые ищем (скорее всего "яблоко" либо хвост)</param>
    /// <returns>Очередь координат-шагов для пути к цели (пустая если пути не найдено - перекрыт, к примеру)</returns>
    private Queue<(int y, int x)> BFSforTarget((int y, int x) desiredCell)
    {
        var moveQueue = new Queue<(int y, int x)>();
        moveQueue.Enqueue( (_actualHead._y, _actualHead._x) ); // Голова змеи как стартовая точка

        var ourTail = _gameState.SolidSnake.body.Last.Value;

        var cameFrom = new Dictionary<(int y, int x), (int y, int x)>();
        cameFrom[(_actualHead._y, _actualHead._x)] = (-1, -1); // без предка (индикатор начала пути)

        while ( moveQueue.Count > 0 )
        {
            var (y, x) = moveQueue.Dequeue();

            if (y == desiredCell.y && x == desiredCell.x) // Нашли путь до нашей цели!
            {
                return RetracePath(cameFrom, (y, x));
            }

            foreach(var (dy, dx) in dirs) // Пока идем к цели, записываем пройденные клетки
            {
                int neighbor_y = y + dy;
                int neighbor_x = x + dx;

                // Проверка границ
                if (neighbor_y < 0 || neighbor_y >= _field.GetLength(0)) continue;
                if (neighbor_x < 0 || neighbor_x >= _field.GetLength(1)) continue;

                var itsTail = neighbor_y == ourTail._y && neighbor_x == ourTail._x;

                if (/*!itsTail && */!IsSafeToMoveHere(neighbor_y, neighbor_x)) continue;

                if (cameFrom.ContainsKey((neighbor_y, neighbor_x))) continue;

                // Добавляем:
                moveQueue.Enqueue((neighbor_y, neighbor_x));
                cameFrom[(neighbor_y, neighbor_x)] = (y, x);
            }
        }
        // Не нашли пути к цели, возвращаем пустой путь:
        return new Queue<(int, int)>();
    }

    /// <summary>
    /// "Отматываем" найденный путь, чтобы змея могла его пройти
    /// </summary>
    /// <param name="path">Найденный путь (от финиша)</param>
    /// <param name="target">Искомая точка (в нашем случае "яблоко")</param>
    /// <returns>Развернутый в обратную сторону набор координат пути</returns>
    private Queue<(int, int)> RetracePath(Dictionary<(int y, int x), (int y, int x)> path, (int y, int x) target)
    {
        var reversePath = new Stack<(int y, int x)>();
        var current = target;

        while( path.ContainsKey(current) )
        {
            var (y, x) = path[current];
            if (y == -1 && x == -1) break; // дошли до стартовой точки
            reversePath.Push(current);
            current = (y, x);
        }

        var result = new Queue<(int, int)>();
        while( reversePath.Count > 0 )
        {
            result.Enqueue(reversePath.Pop());
        }

        return result;
    }

    public void ChangeDirection(Direction dir)
    {
        if( _direction != dir.ToOpposite() )// Змее нельзя поворачивать "в себя"
        {
            _pendingDirection = dir;
        }
    }

    /// <summary>
    /// Устанавливаем направление змеи с учетом последней отправленной команды
    /// </summary>
    private Direction GetPendingSnakeDirection()
    {
        if (_pendingDirection != null)
        {
            return _pendingDirection.Value;
        }
        else return _direction;
    }

    private bool IsOutOfBounds(SnekSegment head) =>
        head._x < 0 || head._x > _size ||
        head._y < 0 || head._y > _size ||
        _field[head._y, head._x] == 1;


    private bool HasCollidedWithItself(SnekSegment head) =>
        _field[head._y, head._x] == 2;

    private bool IsMunchyAhead(SnekSegment head) => _field[head._y, head._x] == 3;

    private bool IsBombAhead(SnekSegment head) => _field[head._y, head._x] == 7;

    private (int y, int x) GetAppleCoords()
    {
        for (int i = 0; i < 1500; i++)
        {
            var x = _rnd.Next(1, _size - 1);
            var y = _rnd.Next(1, _size - 1);

            var cell = _field[y, x];

            if (!IsAppleBlocked(cell) && IsSafeApplePlacement(y, x))
            {
                return (y, x);
            }
        }

        throw new CantPlaceItemsException();
    }

    private bool IsAppleBlocked(int cell)
    {
        return cell is 1 or 2 or 3 or 5 or 7;
    }

    /// <summary>
    /// Проверяем, можем ли разместить "яблоко" так, чтобы к нему было хотя бы два подхода (т.е. не в тупике)
    /// </summary>
    private bool IsSafeApplePlacement(int y, int x)
    {
        int approachDirections = 0;

        foreach (var (dy, dx) in dirs)
        {
            int ny = y + dy;
            int nx = x + dx;

            if (_field[ny, nx] is not (1 or 2 or 5 or 7))
                approachDirections++;
        }

        return approachDirections > 1;
    }

    private (int, int) GetBombCoords()
    {
        for (int i = 0; i < 3000; i++)
        {
            var x = _rnd.Next(1, _size - 1);
            var y = _rnd.Next(1, _size - 1);

            var bombCell = _field[y, x];

            var canPlaceBombHere = CheckWindowForBomb(y, x, spot => spot != 5);

            if (bombCell == 0 && canPlaceBombHere) // Если клетка свободна и не вблизи головы змеи, генерируем там бомбу
                return (y, x);
        }
        // Если не нашли подходящего места для бомбы за i попыток, перестаем их спавнить
        // (К этому моменту игрок заслужил спокойно доесть "яблоки" :)
        _canPlaceBombs = false;
        return (-1, -1);
    }

    /// <summary>
    /// Проверка подмассива вокруг головы змеи
    /// </summary>
    /// <param name="i">Текущая строка головы змеи</param>
    /// <param name="j">Текущий столбец головы змеи</param>
    /// <param name="predicate">Логическая проверка на требуемую клетку</param>
    /// <returns></returns>
    bool CheckWindowForBomb(int y, int x, Func<int, bool> predicate)
    {
        int n = _field.GetLength(0);
        int window = 3; // Размерность подмассива

        int rowStart = Math.Max(0, y - window);
        int rowEnd = Math.Min(n - 1, y + window);

        int colStart = Math.Max(0, x - window);
        int colEnd = Math.Min(n - 1, x + window);

        for (int r = rowStart; r <= rowEnd; r++)
        {
            for (int c = colStart; c <= colEnd; c++)
            {
                if (!predicate(_field[c, r])) // Учитываем, что в двумерном массиве первая координата - высота, вторая - длина
                    return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Финальная отрисовка поля после геймовера
    /// </summary>
    private void PaintTheTownRed(Snek poorSnek)
    {
        // Избавляемся от нашей змеи с особой жестокостью
        foreach (var segment in poorSnek.body)
        {
            _field[segment._y, segment._x] = 4;
        }

        // Взрыв бомбы!
        if (_boom)
            { _field[_nextBomb.Item1, _nextBomb.Item2] = 8; }

        _field[_actualHead._y, _actualHead._x] = 6;
    }

    /// <summary>
    /// Перерисовываем поле с учетом изменения позиций сегментов змеи
    /// </summary>
    /// <param name="snake"></param>
    private void SyncFieldWithSnake(Snek snake)
    {
        // Очищаем старые позиции змеюки (оставляем стены и еду)
        for (int y = 0; y <= _size; y++)
            for (int x = 0; x <= _size; x++)
            {
                var cell = _field[y, x];

                if (cell is 2 or 4 or 5 or 6 or 9)
                    _field[y, x] = 0;
            }

        // Рисуем змею заново
        foreach (var segment in snake.body)
        {
            _field[segment._y, segment._x] = 2;
        }

        // Голова отдельно
        _field[_actualHead._y, _actualHead._x] = 5;

        if( _highlightBombSpawnArea && _diffMultiplier > 0) // Отладка расположения бомбы
        {
            int n = _field.GetLength(0);
            int window = 2;

            int rowStart = Math.Max(0, _actualHead._x - window);
            int rowEnd = Math.Min(n - 1, _actualHead._x + window);

            int colStart = Math.Max(0, _actualHead._y - window);
            int colEnd = Math.Min(n - 1, _actualHead._y + window);

            // Bomb window debug:

            for (int r = rowStart; r <= rowEnd; r++)
            {
                for (int c = colStart; c <= colEnd; c++)
                {
                    if (!(_field[c, r] is 1 or 2 or 3 or 5 or 7))
                        // Рисуем места, где НЕ ДОЛЖНА заспавниться бомба
                        _field[c, r] = 9;
                }
            }
        }
    }

    public string GetCurrentScore()
    {
        return _score.ToString();
    }

    public void ToggleDebugOption(DebugOption option)
    {
        switch (option)
        {
            case DebugOption.ToggleBombSpawnAreaHighlight:
                _highlightBombSpawnArea = !_highlightBombSpawnArea; break;
            case DebugOption.ToggleSnakeAi:
                _snakeAiControlled = !_snakeAiControlled; break;
            default: break;
        }
    }
}
