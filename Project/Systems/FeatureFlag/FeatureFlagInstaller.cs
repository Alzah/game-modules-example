using Project.Helpers;
using Project.Helpers.Attributes;
using Project.Systems.FeatureFlag.Signals;
using System;
using UnityEngine;
using Zenject;

namespace Project.Systems.FeatureFlag
{
    [CreateAssetMenu(fileName = "FeatureFlagInstaller", menuName = "Project/Installers/Systems/FeatureFlag")]
    public sealed class FeatureFlagInstaller : ScriptableObjectInstaller
    {
        [NamespacePath("Project")]
        public InspectableType<Enum> FlagEnumType;

        public override void InstallBindings()
        {
            Type type = FlagEnumType;
            if (type == null)
            {
                throw new ArgumentNullException("FlagEnumType is null");
            }
            Container.BindInterfacesAndSelfTo<FeatureFlagSystem>().AsSingle().NonLazy();
            Container.BindInstance(type).WhenInjectedInto<FeatureFlagSystem>();

            Container.DeclareSignal<FeatureFlagUpdatedSignal>();
        }
    }
}