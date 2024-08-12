using Project.Managers.Game.GameStates;
using Project.Managers.Game.Signals;
using UnityEngine;
using Zenject;

namespace Project.Managers.Game
{
    [CreateAssetMenu(fileName = "GameInstaller", menuName = "Project/Installers/Managers/Game")]
    public class GameInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings()
        {
            SignalBusInstaller.Install(Container);

            DeclareSignals();

            InstallStateFactory();
            InstallStateMachine();

            Container.BindInterfacesAndSelfTo<GameManager>().AsSingle().NonLazy();

            Container.BindInterfacesAndSelfTo<ApplicationProvider>().FromNewComponentOnNewGameObject()
                .AsSingle().NonLazy();
        }

        protected virtual void DeclareSignals()
        {
            Container.DeclareSignal<GameStateChangedSignal>();
            Container.DeclareSignal<GamePauseChangedSignal>();
            Container.DeclareSignal<ApplicationFocusSignal>();
            Container.DeclareSignal<ApplicationPauseSignal>();
        }

        private void InstallStateFactory()
        {
            Container.BindFactory<InitState, InitState.Factory>().WhenInjectedInto<GameStateFactory>();
            Container.BindFactory<MenuState, MenuState.Factory>().WhenInjectedInto<GameStateFactory>();
            Container.BindFactory<LevelState, LevelState.Factory>().WhenInjectedInto<GameStateFactory>();
            Container.BindFactory<FailState, FailState.Factory>().WhenInjectedInto<GameStateFactory>();
            Container.BindFactory<WinState, WinState.Factory>().WhenInjectedInto<GameStateFactory>();

            Container.Bind<GameStateFactory>().AsSingle().NonLazy();
        }

        protected virtual void InstallStateMachine()
        {
            Container.BindInterfacesAndSelfTo<GenericStateMachine>().AsSingle().NonLazy();
        }
    }
}