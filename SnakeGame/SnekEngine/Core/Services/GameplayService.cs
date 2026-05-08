using SnakeGame.SnekEngine.Abstractions.Interfaces;
using SnakeGame.SnekEngine.Abstractions.Models;
using System;
using static SnakeGame.SnekEngine.Abstractions.GameEnums;

namespace SnakeGame.SnekEngine.Core.Services
{
    internal class GameplayService : IGameplayService
    {
        private readonly FieldInitializer _initializer;
        private readonly FieldUpdater _updater;
        private Random _rnd;
        private InitialSettings _settings;
        private Direction _currentDir;

        public GameplayService(FieldInitializer initializer,
            FieldUpdater updater) 
        {
            _initializer = initializer;
            _updater = updater;
        }

        public GameSnapshot InitializeLevel(InitialSettings setup)
        {
            _settings = setup;
            _currentDir = setup.FirstDirection;
            _rnd = new Random(setup.Seed ?? Environment.TickCount);
            return _initializer.InitializeField(_settings, _rnd);
        }
    }
}
