using UnityEngine;
using Zenject;

namespace Project.Systems.Versioning
{
    [CreateAssetMenu(fileName = "VersionManagerInstaller", menuName = "Project/Installers/Systems/Version")]
    public sealed class VersionManagerInstaller : ScriptableObjectInstaller
    {
        public GameVersion Version;

        public override void InstallBindings()
        {
            Container.BindInstance(Version);
        }
    }
}
