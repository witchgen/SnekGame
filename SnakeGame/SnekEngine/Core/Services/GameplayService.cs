using SnakeGame.SnekEngine.Abstractions.Interfaces;
using SnakeGame.SnekEngine.Abstractions.Models;
using System;

namespace SnakeGame.SnekEngine.Core.Services
{
    internal class GameplayService : IGameplayService
    {
        private readonly FieldInitializer _initializer;
        private readonly FieldUpdater _updater;
        private readonly Random _rnd;
        private InitialSettings _settings;

        public GameplayService(InitialSettings setup) 
        {
            _rnd = new Random(setup.Seed ?? Environment.TickCount);
            _settings = setup;
        }

        public GameSnapshot InitializeLevel() => _initializer.InitializeField(_settings);
    }
}
