using Project.Extensions;
using Project.Managers.Game.Signals;
using System;
using System.Collections.Generic;
using Zenject;

namespace Project.Managers.Game.GameStates
{
    public class GenericStateMachine : IStateMachine, IInitializable, ITickable
    {
        private (Enum Type, IGameState State) _currentGameState;
        private (Enum Type, IGameState State) _previousGameState;
        private Dictionary<Type, Dictionary<Enum, IGameState>> _states = new();

        private readonly SignalBus _signalBus;
        private readonly GameStateFactory _gameStateFactory;

        public GenericStateMachine(SignalBus signalBus, GameStateFactory gameStateFactory)
        {
            _signalBus = signalBus;
            _gameStateFactory = gameStateFactory;
        }

        protected virtual void CreateStates()
        {
            foreach (var en in EnumExtensions.ToArray<EGameState>())
            {
                RegisterState(_gameStateFactory.Create(en));
            }
        }

        public virtual void Initialize()
        {
            CreateStates();
            StartState(EGameState.Init, new GameStateParams());
        }

        public void Tick()
        {
            _currentGameState.State?.Tick();
        }

        public void StartState<TType>(TType type) where TType : Enum
        {
            StartState(type, new GameStateParams());
        }

        public void StartState<TType, TParam>(TType type, TParam param) where TType : Enum where TParam : GameStateParams
        {
            var state = GetGameState(type);
            if (state == null)
            {
                throw new ArgumentException();
            }

            if (state == _currentGameState.State)
            {
                return;
            }

            _currentGameState.State?.Exit();
            _previousGameState = _currentGameState;
            _currentGameState = (type, state);
            _currentGameState.State.Enter(param);

            _signalBus.Fire(new GameStateChangedSignal(type, _previousGameState.Type));
        }

        public (Enum Type, IGameState State) GetCurrentState()
        {
            return _currentGameState;
        }

        public (Enum Type, IGameState State) GetPreviousState()
        {
            return _previousGameState;
        }

        protected void RegisterState<TType>(GenericGameState<TType> newState) where TType : Enum
        {
            var type = typeof(TType);
            var states = GetStatesForType(type);

            states[newState.StateType] = newState;
        }

        private IGameState GetGameState<TType>(TType stateType) where TType : Enum
        {
            var type = typeof(TType);
            var states = GetStatesForType(type);

            return states[stateType];
        }

        private Dictionary<Enum, IGameState> GetStatesForType(Type type)
        {
            if (_states.TryGetValue(type, out Dictionary<Enum, IGameState> states))
            {
                return states;
            }

            _states[type] = new Dictionary<Enum, IGameState>();

            return _states[type];
        }
    }
}
