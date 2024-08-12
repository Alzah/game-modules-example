using UnityEngine;
using Zenject;

namespace Project.Managers.Lives
{
    [CreateAssetMenu(fileName = "LivesManagerInstaller", menuName = "Project/Installers/Managers/Lives")]
    public sealed class LivesManagerInstaller : ScriptableObjectInstaller
    {
        public LivesManager.Settings Settings;

        public override void InstallBindings()
        {
            Container.DeclareSignal<LivesChangedSignal>();

            Container.BindInterfacesAndSelfTo<LivesManager>().AsSingle().NonLazy();
            Container.BindInstance(Settings).WhenInjectedInto<LivesManager>();
        }
    }
}