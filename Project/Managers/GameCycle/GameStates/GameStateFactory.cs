using System;

namespace Project.Managers.Game.GameStates
{
    public class GameStateFactory
    {
        private readonly InitState.Factory _initStateFactory;
        private readonly MenuState.Factory _menuStateFactory;
        private readonly LevelState.Factory _levelStateFactory;
        private readonly FailState.Factory _failStateFactory;
        private readonly WinState.Factory _winStateFactory;

        public GameStateFactory(InitState.Factory initStateFactory, MenuState.Factory menuStateFactory,
            LevelState.Factory levelStateFactory, FailState.Factory failStateFactory, WinState.Factory winStateFactory)
        {
            _initStateFactory = initStateFactory;
            _menuStateFactory = menuStateFactory;
            _levelStateFactory = levelStateFactory;
            _failStateFactory = failStateFactory;
            _winStateFactory = winStateFactory;
        }

        public GenericGameState<EGameState> Create(EGameState state)
        {
            switch (state)
            {
                case EGameState.Init:
                    return _initStateFactory.Create();
                case EGameState.Menu:
                    return _menuStateFactory.Create();
                case EGameState.Level:
                    return _levelStateFactory.Create();
                case EGameState.Win:
                    return _winStateFactory.Create();
                case EGameState.Fail:
                    return _failStateFactory.Create();
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
