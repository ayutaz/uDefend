using System;
using System.Collections.Generic;
using System.IO;

namespace uDefend.Core
{
    public class BackupManager
    {
        public void CreateBackup(string filePath, int maxBackups)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));
            if (maxBackups <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxBackups), "Must be positive.");
            if (!File.Exists(filePath))
                return;

            // Rotate existing backups: .bak.3 → delete, .bak.2 → .bak.3, .bak.1 → .bak.2
            for (int i = maxBackups; i >= 1; i--)
            {
                string backupPath = GetBackupPath(filePath, i);
                if (i == maxBackups)
                {
                    if (File.Exists(backupPath))
                        File.Delete(backupPath);
                }
                else
                {
                    string nextPath = GetBackupPath(filePath, i + 1);
                    if (File.Exists(backupPath))
                    {
                        if (File.Exists(nextPath))
                            File.Delete(nextPath);
                        File.Move(backupPath, nextPath);
                    }
                }
            }

            // Copy current file to .bak.1
            File.Copy(filePath, GetBackupPath(filePath, 1));
        }

        public bool RestoreLatestBackup(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            // Find the lowest numbered backup that exists
            for (int i = 1; ; i++)
            {
                string backupPath = GetBackupPath(filePath, i);
                if (!File.Exists(backupPath))
                    return false;

                // Try to restore from this backup
                try
                {
                    if (File.Exists(filePath))
                        File.Delete(filePath);
                    File.Copy(backupPath, filePath);
                    return true;
                }
                catch
                {
                    // If this backup is corrupted, try next
                    continue;
                }
            }
        }

        public string[] GetBackupPaths(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            var paths = new List<string>();
            for (int i = 1; ; i++)
            {
                string backupPath = GetBackupPath(filePath, i);
                if (!File.Exists(backupPath))
                    break;
                paths.Add(backupPath);
            }
            return paths.ToArray();
        }

        private static string GetBackupPath(string filePath, int index)
        {
            return filePath + ".bak." + index;
        }
    }
}
