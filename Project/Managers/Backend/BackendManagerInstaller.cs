using UnityEngine;
using Zenject;

namespace Project.Managers
{
    [CreateAssetMenu(fileName = "BackendManagerInstaller", menuName = "Project/Installers/Managers/Backend")]
    public sealed class BackendManagerInstaller : ScriptableObjectInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<BackendManager>().AsSingle().NonLazy();
        }
    }
}