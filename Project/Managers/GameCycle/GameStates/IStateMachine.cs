using System;

namespace Project.Managers.Game.GameStates
{
    public interface IStateMachine
    {
        public void StartState<TType>(TType type) where TType : Enum;
        public void StartState<TType, TParam>(TType type, TParam param) where TType : Enum where TParam : GameStateParams;
        public (Enum Type, IGameState State) GetCurrentState();
        public (Enum Type, IGameState State) GetPreviousState();
    }
}
