using Project.Helpers.Exceptions;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Project.Systems.Versioning
{
    [Serializable]
    public class Version : IEquatable<Version>, IComparable<Version>
    {
        [SerializeField]
        protected int _major;
        [SerializeField]
        protected int _minor;
        [SerializeField]
        protected int _build;
        [SerializeField]
        protected int _revision;

        private Regex _mask = new Regex(@"[0-9]+(\.[0-9]+(\.[0-9]+(\.[0-9]+)?)?)?");

        public int Major => _major;
        public int Minor => _minor;
        public int Build => _build;
        public int Revision => _revision;

        public string FullVersion => $"{_major}.{_minor}.{_build}.{_revision}";

        public Version() : this(0, 0, 1, 0) { }

        public Version(string version)
        {
            if (!_mask.IsMatch(version))
            {
                throw new VersionParseException(version);
            }

            var vers = version.Split('.').Select(int.Parse).ToArray();
            Array.Resize(ref vers, 4);

            _major = vers[0];
            _minor = vers[1];
            _build = vers[2];
            _revision = vers[3];
        }

        public Version(int major, int minor, int build, int revision)
        {
            _major = major;
            _minor = minor;
            _build = build;
            _revision = revision;
        }

        public Version(Version version) : this(version.Major, version.Minor, version.Build, version.Revision) { }

        public void IncreaseMajor()
        {
            Increase(EVersionType.Major);
        }

        public void IncreaseMinor()
        {
            Increase(EVersionType.Minor);
        }

        public void IncreaseBuild()
        {
            Increase(EVersionType.Build);
        }

        public void IncreaseRevision()
        {
            Increase(EVersionType.Revision);
        }

#if UNITY_EDITOR
        public void Increase(EVersionType type)
#else
        private void Increase(EVersionType type)
#endif
        {
            switch (type)
            {
                case EVersionType.Major:
                    _major++;
                    break;
                case EVersionType.Minor:
                    _minor++;
                    break;
                case EVersionType.Build:
                    _build++;
                    break;
                case EVersionType.Revision:
                    _revision++;
                    break;
            }
        }

#if UNITY_EDITOR

        public void DecreaseMajor()
        {
            Decrease(EVersionType.Major);
        }

        public void DecreaseMinor()
        {
            Decrease(EVersionType.Minor);
        }

        public void DecreaseBuild()
        {
            Decrease(EVersionType.Build);
        }

        public void DecreaseRevision()
        {
            Decrease(EVersionType.Revision);
        }

        public void Decrease(EVersionType type)
        {
            switch (type)
            {
                case EVersionType.Major:
                    if (_major > 0) _major--;
                    break;
                case EVersionType.Minor:
                    if (_minor > 0) _minor--;
                    break;
                case EVersionType.Build:
                    if (_build > 0) _build--;
                    break;
                case EVersionType.Revision:
                    if (_revision > 0) _revision--;
                    break;
            }
        }

#endif

        public bool Equals(Version other)
        {
            return other is null
                ? false
                : _major == other.Major
                && _minor == other.Minor
                && _build == other.Build
                && _revision == other.Revision;
        }

        public override bool Equals(System.Object obj)
        {
            if (obj == null)
                return false;

            Version versionObj = obj as Version;
            return versionObj == null ? false : Equals(versionObj);
        }

        public override string ToString()
        {
            return FullVersion;
        }

        public override int GetHashCode()
        {
            return (_major * 1000000) + (_minor * 10000) + (_build * 100) + _revision;
        }

        public int CompareTo(Version other)
        {
            if (other == null) return 1;

            var majorCompare = _major.CompareTo(other.Major);
            if (majorCompare != 0)
            {
                return majorCompare;
            }

            var minorCompare = _minor.CompareTo(other.Minor);
            if (minorCompare != 0)
            {
                return minorCompare;
            }

            var buildCompare = _build.CompareTo(other.Build);
            return buildCompare != 0 ? buildCompare : _revision.CompareTo(other.Revision);
        }

        #region Static

        private static Version _minVersion = new Version(0, 0, 0, 0);
        private static Version _maxVersion = new Version(100, 100, 100, 100);

        public static Version MaxVersion { get { return _maxVersion; } }
        public static Version MinVersion { get { return _minVersion; } }

        public static bool operator ==(Version version1, Version version2)
        {
            return ((object)version1) == null || ((object)version2) == null
                ? System.Object.Equals(version1, version2)
                : version1.Equals(version2);
        }

        public static bool operator !=(Version version1, Version version2)
        {
            return ((object)version1) == null || ((object)version2) == null
                ? !System.Object.Equals(version1, version2)
                : !version1.Equals(version2);
        }

        public static bool operator >(Version operand1, Version operand2)
        {
            return operand1.CompareTo(operand2) > 0;
        }

        public static bool operator <(Version operand1, Version operand2)
        {
            return operand1.CompareTo(operand2) < 0;
        }

        public static bool operator >=(Version operand1, Version operand2)
        {
            return operand1.CompareTo(operand2) >= 0;
        }

        public static bool operator <=(Version operand1, Version operand2)
        {
            return operand1.CompareTo(operand2) <= 0;
        }

        #endregion
    }

    public enum EVersionType
    {
        Major = 0,
        Minor = 1,
        Build = 2,
        Revision = 3
    }
}
