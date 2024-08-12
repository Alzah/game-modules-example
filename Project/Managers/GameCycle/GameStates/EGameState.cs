using Project.Helpers.Attributes;

namespace Project.Managers.Game.GameStates
{
    public enum EGameState
    {
        Init = 0,
        Menu = 1,
        [Gameplay]
        Level = 2,
        [Victory]
        Win = 3,
        Fail = 4,
    }
}
