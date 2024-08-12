using Project.Managers.Game.GameStates;
using Project.Managers.Game.Interfaces;
using Project.Managers.Game.Signals;
using System;
using Zenject;

namespace Project.Managers.Game
{
    public class GameManager : IPauseManager
    {
        private readonly SignalBus _signalBus;
        private readonly IStateMachine _stateMachine;

        private bool _isPaused;

        public GameManager(SignalBus signalBus, IStateMachine stateMachine)
        {
            _signalBus = signalBus;
            _stateMachine = stateMachine;
        }

        public Enum GetCurrentState()
        {
            return _stateMachine.GetCurrentState().Type;
        }

        public Enum GetPreviousState()
        {
            return _stateMachine.GetPreviousState().Type;
        }

        public void SwitchGameState<TType>(TType newState, GameStateParams param = null) where TType : Enum
        {
            param ??= new GameStateParams();

            _stateMachine.StartState(newState, param);
        }

        public void PauseToggle(bool isPaused)
        {
            _isPaused = isPaused;

            _signalBus?.Fire(new GamePauseChangedSignal { IsPaused = _isPaused });
        }

        public bool IsCurrentStateStarted => _stateMachine.GetCurrentState().State != null && _stateMachine.GetCurrentState().State.IsEntered;

        public bool IsPaused => _isPaused;
    }
}