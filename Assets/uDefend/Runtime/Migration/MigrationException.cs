using System;

namespace uDefend.Migration
{
    public class MigrationException : Exception
    {
        public MigrationException()
            : base("A migration operation failed.") { }

        public MigrationException(string message)
            : base(message) { }

        public MigrationException(string message, Exception innerException)
            : base(message, innerException) { }
    }

    public class UnsupportedVersionException : MigrationException
    {
        public ushort FileVersion { get; }
        public ushort MinSupportedVersion { get; }

        public UnsupportedVersionException(ushort fileVersion, ushort minSupportedVersion)
            : base($"Save file version {fileVersion} is below the minimum supported version {minSupportedVersion}.")
        {
            FileVersion = fileVersion;
            MinSupportedVersion = minSupportedVersion;
        }
    }

    public class MissingMigrationException : MigrationException
    {
        public ushort FromVersion { get; }
        public ushort TargetVersion { get; }

        public MissingMigrationException(ushort fromVersion, ushort targetVersion)
            : base($"No migration registered for version {fromVersion} to {targetVersion}.")
        {
            FromVersion = fromVersion;
            TargetVersion = targetVersion;
        }
    }
}
