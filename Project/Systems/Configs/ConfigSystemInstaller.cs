using UnityEngine;
using Zenject;

namespace Project.Systems.Configs
{
    [CreateAssetMenu(fileName = "ConfigSystemInstaller", menuName = "Project/Installers/Systems/Configs")]
    public sealed class ConfigSystemInstaller : ScriptableObjectInstaller
    {
        public string ConfigsDirectory = "configs";

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<ConfigSystem>().AsSingle().NonLazy();
            Container.BindInstance(ConfigsDirectory).WhenInjectedInto<ConfigSystem>();
        }
    }
}
