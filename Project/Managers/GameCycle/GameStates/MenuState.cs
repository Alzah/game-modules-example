using Project.Managers.Scene;
using Zenject;

namespace Project.Managers.Game.GameStates
{
    public class MenuState : GenericGameState<EGameState>
    {
        private readonly SceneManager _sceneManager;

        public MenuState(SceneManager sceneManager)
        {
            _sceneManager = sceneManager;
        }

        public override EGameState StateType => EGameState.Menu;

        public override void Enter<TParams>(TParams param = null)
        {
            _sceneManager.SwitchScene(new SceneDescriptor("Menu", UnityEngine.SceneManagement.LoadSceneMode.Additive))
                .Then(_ =>
                {
                    IsEntered = true;
                });
        }

        public class Factory : PlaceholderFactory<MenuState> { }
    }
}
