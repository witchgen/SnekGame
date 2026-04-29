using SnakeGame.Custom;
using SnakeGame.GameInfo;
using System;
using System.Text;
using System.Threading.Tasks;
using static SnakeGame.Custom.CustomExceptions;
using static SnakeGame.GameInfo.Enums;

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
    /// Установить режим отладки (для разработчика)
    /// </summary>
    void SwitchDebugMode();
}

public class GameService : IGameService
{
    private IFieldGenerator _generator = new FieldGenerator();

    private int _score = 0; // Счет
    private int _size; // Размер поля (без границ)
    private SnekSegment _actualHead; // Хранимая позиция головы змеи
    private Random _rnd = new(); // Бог рандома
    private Direction _direction; // Хранимое направление
    public int[,] _field; // Хранимое состояние игрового поля
    public (int, int) _oldMeat; // Старая позиция бомбы
    private bool _boom = false; // Взорвались?

    // Режим отладки:
    private bool _isDebugModeActive = false;

    private bool _gamePaused = false;
    private GameState _gameState = new GameState() { CurrentGameData = new PlayData() };

    // Событие для передачи обновлений поля
    public event EventHandler<string>? FieldUpdated;
    public GameStatus Status { get; private set; } = GameStatus.Initialized;

    private int _speedMs = 500; // Скорость игры (медленная по дефолту)
    private int _diffMultiplier = 1; // Хранимая сложность (низкая по дефолту)

    public void SwitchDebugMode()
    {
        _isDebugModeActive = !_isDebugModeActive;
    }

    public void Pause()
    {
        Status = GameStatus.Paused;
    }

    public void Resume()
    {
        Status = GameStatus.Running;
    }

    public void PauseGame()
    {
        Status = GameStatus.Paused;
        _gamePaused = true;
    }

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
        _gameState = new GameState() { CurrentGameData = new PlayData() };

        _score = 0;
        _field = _generator.GetNewGameField(_size);
        var initPos = _generator.SetInitialSnakePosition(_size);
        _actualHead = new SnekSegment(initPos, initPos);
        _oldMeat = (1, 1);
        _boom = false;

        FieldUpdated?.Invoke(this, DrawField());
    }

    public GameState GetGameState()
    {
        return _gameState;
    }

    // Точка входа для игрового процесса новой игры, задаем новое состояние
    public async Task<GameStatus> StartNewGame(GameState state)
    {
        //ResetGame();
        _gameState = state;

        _gameState.SolidSnake = new Snek(_actualHead);
        _direction = Direction.Up;
        _gameState.SnakeHeadPosition = _actualHead;

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
                    DeathReason = _gameState.CurrentGameData.DeathReason,
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

        var nextDirection = _direction;

        ChangeDirection(nextDirection);

        switch (nextDirection)
        {
            case Direction.Up: nextHead._y--; break;
            case Direction.Down: nextHead._y++; break;
            case Direction.Left: nextHead._x--; break;
            case Direction.Right: nextHead._x++; break;
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
            if (_diffMultiplier > 0) // Если сложность выше легкой, с яблоком генерируется новая бомба
            {
                _field[_oldMeat.Item1, _oldMeat.Item2] = 0;
                _oldMeat = GetMeatCoords();
                _gameState.BombPosition = _oldMeat;
                _field[_oldMeat.Item1, _oldMeat.Item2] = 7;
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
                _gameState.CurrentGameData.DeathReason = GameOverReason.Starvation;
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

    public void ChangeDirection(Direction dir)
    {
        if (_direction != GetOppositeDir(dir)) // Змее нельзя поворачивать "в себя"
            _direction = dir;
    }

    /// <summary>
    /// Вспомогательный метод для пресечения возможности задать обратное движение для змеи
    /// </summary>
    /// <param name="dir">Целевое направление движения</param>
    /// <returns>Обратное направление</returns>
    private Direction GetOppositeDir(Direction dir)
    => dir switch
    {
        Direction.Up => Direction.Down,
        Direction.Down => Direction.Up,
        Direction.Left => Direction.Right,
        Direction.Right => Direction.Left,
        _ => dir,
    };

    private bool IsOutOfBounds(SnekSegment head) =>
        head._x < 0 || head._x > _size ||
        head._y < 0 || head._y > _size ||
        _field[head._y, head._x] == 1;


    private bool HasCollidedWithItself(SnekSegment head) =>
        _field[head._y, head._x] == 2;

    private bool IsMunchyAhead(SnekSegment head) => _field[head._y, head._x] == 3;

    private bool IsBombAhead(SnekSegment head) => _field[head._y, head._x] == 7;


    //private (int, int) GetAppleCoords()
    //{
    //    var x = _rnd.Next(1, _size - 1);
    //    var y = _rnd.Next(1, _size - 1);

    //    var apple = _field[y, x];

    //    for(int i = 0; i <= 3000; i++)
    //    {
    //        if (apple == 1 || apple == 2 || apple == 3 || apple == 5 || apple == 7)
    //        {
    //            GetAppleCoords();
    //            i++;
    //        }
    //        else return (y, x);
    //    }

    //    throw new CantPlaceYummyException();
    //}

    private (int y, int x) GetAppleCoords()
    {
        for (int i = 0; i < 1500; i++)
        {
            var x = _rnd.Next(1, _size - 1);
            var y = _rnd.Next(1, _size - 1);

            var cell = _field[y, x];

            if (!IsBlocked(cell))
                return (y, x);
        }

        throw new CantPlaceItemsException();
    }

    private bool IsBlocked(int cell)
    {
        return cell is 1 or 2 or 3 or 5 or 7;
    }

    private (int, int) GetMeatCoords()
    {
        for (int i = 0; i < 3000; i++)
        {
            var x = _rnd.Next(1, _size - 1);
            var y = _rnd.Next(1, _size - 1);

            var bombCell = _field[y, x];

            var canPlaceBombHere = CheckWindowForBomb(x, y, spot => spot != 5);

            if (bombCell == 0 && canPlaceBombHere) // Если клетка свободна и не вблизи головы змеи, генерируем там бомбу
                return (y, x);
        }
        // Если не нашли подходящего места для бомбы за i попыток, завершаем игру
        // (Доработать: прекратить генерировать новые бомбы если такое происходит)
        throw new CantPlaceItemsException();
    }

    /// <summary>
    /// Проверка подмассива вокруг головы змеи
    /// </summary>
    /// <param name="i">Текущая строка головы змеи</param>
    /// <param name="j">Текущий столбец головы змеи</param>
    /// <param name="predicate">Логическая проверка на требуемую клетку</param>
    /// <returns></returns>
    bool CheckWindowForBomb(int i, int j, Func<int, bool> predicate)
    {
        int n = _field.GetLength(0);
        int window = 3; // Размерность подмассива

        int rowStart = Math.Max(0, i - window);
        int rowEnd = Math.Min(n - 1, i + window);

        int colStart = Math.Max(0, j - window);
        int colEnd = Math.Min(n - 1, j + window);

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
            { _field[_oldMeat.Item1, _oldMeat.Item2] = 8; }

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

        if( _isDebugModeActive && _diffMultiplier > 0) // Отладка расположения бомбы
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
}
