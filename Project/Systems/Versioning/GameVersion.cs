using System;
using UnityEngine;

namespace Project.Systems.Versioning
{
    [Serializable]
    public sealed class GameVersion : Version
    {
        public EBuildType Type = EBuildType.Dev;

        public GameVersion() : base() { }

        public GameVersion(GameVersion origin) : base(origin)
        {
            Type = origin.Type;
        }

        public int GetBundleVersion()
        {
            return GetHashCode();
        }

        public string TypedVersion => $"{FullVersion}-{Type.ToShort()}";

        public override string ToString()
        {
            return TypedVersion;
        }
    }

    public static class BuildTypeExtensions
    {
        public static string ToShort(this EBuildType type)
        {
            return type switch
            {
                EBuildType.Dev => "d",
                EBuildType.Stage => "s",
                EBuildType.Release => "r",
                _ => throw new NotImplementedException("EBuildType new type non implement ToString")
            };
        }
    }

    public enum EBuildType
    {
        Dev,
        Stage,
        Release
    }

    public interface IBuildTypeSettings
    {
        public void OnValidate();

        public EBuildType BuildType { get; }
    }

    [Serializable]
    public class BuildTypeSettings : IBuildTypeSettings
    {
        [SerializeField, HideInInspector] protected string _name = "";

        [SerializeField] private EBuildType _buildType;
        [SerializeField] private bool _isEnabled;

        public virtual void OnValidate()
        {
            _name = _isEnabled ? "" : "[Disabled] ";
            _name += $"{_buildType}";
        }

        public EBuildType BuildType => _buildType;

        public bool IsEnabled => _isEnabled;
    }
}
