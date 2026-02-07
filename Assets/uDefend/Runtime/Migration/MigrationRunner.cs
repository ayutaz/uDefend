using System;
using System.Collections.Generic;

namespace uDefend.Migration
{
    public class MigrationRunner
    {
        private readonly Dictionary<ushort, IMigration> _migrations = new Dictionary<ushort, IMigration>();
        private readonly ushort _currentVersion;
        private readonly ushort _minSupportedVersion;

        public MigrationRunner(ushort currentVersion, ushort minSupportedVersion)
        {
            if (minSupportedVersion > currentVersion)
                throw new ArgumentException(
                    "MinSupportedVersion cannot be greater than CurrentVersion.",
                    nameof(minSupportedVersion));

            _currentVersion = currentVersion;
            _minSupportedVersion = minSupportedVersion;
        }

        public void Register(IMigration migration)
        {
            if (migration == null) throw new ArgumentNullException(nameof(migration));

            if (migration.ToVersion != (ushort)(migration.FromVersion + 1))
                throw new ArgumentException(
                    $"Migration must be sequential: expected ToVersion={migration.FromVersion + 1}, got {migration.ToVersion}.");

            if (_migrations.ContainsKey(migration.FromVersion))
                throw new ArgumentException(
                    $"A migration from version {migration.FromVersion} is already registered.");

            _migrations[migration.FromVersion] = migration;
        }

        public bool NeedsMigration(ushort fileVersion)
        {
            return fileVersion < _currentVersion;
        }

        public byte[] Run(byte[] data, ushort fileVersion)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            if (fileVersion < _minSupportedVersion)
                throw new UnsupportedVersionException(fileVersion, _minSupportedVersion);

            if (fileVersion > _currentVersion)
                throw new MigrationException(
                    $"Save file version {fileVersion} is newer than the current version {_currentVersion}.");

            if (fileVersion == _currentVersion)
                return data;

            byte[] current = data;
            for (ushort v = fileVersion; v < _currentVersion; v++)
            {
                if (!_migrations.TryGetValue(v, out IMigration migration))
                    throw new MissingMigrationException(v, (ushort)(v + 1));

                try
                {
                    current = migration.Migrate(current);
                }
                catch (MigrationException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new MigrationException(
                        $"Migration from version {v} to {v + 1} failed.", ex);
                }
            }

            return current;
        }
    }
}
