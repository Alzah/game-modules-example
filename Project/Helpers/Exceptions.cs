using System;
using System.Threading.Tasks;
using ProjectVersion = Project.Systems.Versioning.Version;

namespace Project.Helpers.Exceptions
{
    public sealed class FileSystemWriteException : Exception
    {
        public FileSystemWriteException(Task task) : base($"Write to storage failed with status: {task.Status}")
        {
        }
    }

    public sealed class FileSystemReadException : Exception
    {
        public FileSystemReadException(Task task) : base($"Read from storage failed with status: {task.Status}")
        {
        }
    }

    public sealed class FileSystemCancelTaskException : Exception
    {
    }

    public sealed class LevelNoFoundException : Exception
    {
        public LevelNoFoundException(int level, string type) : base($"No found #{level} for {type}")
        {
        }
    }

    public sealed class LevelNoFoundException<LevelEnumType> : Exception
    {
        public LevelNoFoundException(int level, LevelEnumType type) : base($"No found #{level} for {type}")
        {
        }
    }

    public sealed class UndefinedLevelTypeException : Exception
    {
    }

    public sealed class ConfigNoFoundException : Exception
    {
    }

    public sealed class SpanVersionException : Exception
    {
        public SpanVersionException(string span) : base($"Incorrect span template {span}. Correct templates: >|>=|<|<=|=|==x.x.x.x")
        {
        }

        public SpanVersionException(ProjectVersion left, ProjectVersion right) : base($"Incorrect interval left {left} > right {right}")
        {
        }
    }

    public sealed class VersionParseException : Exception
    {
        public VersionParseException(string version) : base($"Version {version} no match with x.x.x.x, x.x.x, x.x, x")
        {
        }
    }
}
