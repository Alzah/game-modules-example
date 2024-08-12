using System;

namespace Project.Managers.Game.GameStates
{
    public interface IGameState
    {
        public void Enter<TParams>(TParams param = null) where TParams : GameStateParams;
        public void Exit();
        public void Tick();
        public bool IsEntered { get; }
    }

    public abstract class GenericGameState<TType> : IGameState where TType : Enum
    {
        public abstract TType StateType { get; }

        public virtual void Enter<TParams>(TParams param = null) where TParams : GameStateParams
        {
            IsEntered = true;
        }

        public virtual void Exit()
        {
            IsEntered = false;
        }

        public virtual void Tick() { }

        public bool IsEntered { get; protected set; }
    }

    public class GameStateParams
    {
    }
}
