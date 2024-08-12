namespace Project.Managers.Game.Interfaces
{
    public interface IPauseManager
    {
        public bool IsPaused { get; }
        public void PauseToggle(bool isPaused);
    }
}
