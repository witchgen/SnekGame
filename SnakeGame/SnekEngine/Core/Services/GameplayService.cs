using SnakeGame.SnekEngine.Abstractions.Interfaces;
using SnakeGame.SnekEngine.Abstractions.Models;
using SnakeGame.SnekEngine.World;
using System;
using System.Collections.Generic;
using static SnakeGame.SnekEngine.Abstractions.GameEnums;

namespace SnakeGame.SnekEngine.Core.Services
{
    internal class GameplayService : IGameplayService
    {
        private readonly FieldInitializer _initializer;
        private readonly FieldUpdater _updater;
        private Random _rnd;
        private InitialSettings _settings;

        public GameplayService(FieldInitializer initializer,
            FieldUpdater updater) 
        {
            _initializer = initializer;
            _updater = updater;
        }

        public GameSnapshot InitializeLevel(InitialSettings setup)
        {
            _settings = setup;
            _rnd = new Random(setup.Seed);
            return _initializer.InitializeField(_settings, _rnd);
        }

        public GameSnapshot Tick(GameSnapshot currentState, Direction buffer)
        {
            var newState = _updater.UpdateField(currentState, buffer, _rnd);

            return newState;
        }
    }
}
