using System;

namespace Project.Managers.Game.Signals
{
    public sealed class GameStateChangedSignal
    {
        public GameStateChangedSignal(Enum currentState, Enum previousState)
        {
            CurrentState = currentState;
            PreviousState = previousState;
        }

        public Enum CurrentState;
        public Enum PreviousState;
    }

    public sealed class GamePauseChangedSignal
    {
        public bool IsPaused;
    }
}
