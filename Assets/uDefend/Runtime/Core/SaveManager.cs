using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using uDefend.Encryption;
using uDefend.KeyManagement;
using uDefend.Migration;
using uDefend.Serialization;

namespace uDefend.Core
{
    public class SaveManager
    {
        private readonly SaveSettings _settings;
        private readonly ISerializer _serializer;
        private readonly IEncryptionProvider _encryptionProvider;
        private readonly IKeyProvider _keyProvider;
        private readonly BackupManager _backupManager;
        private readonly MigrationRunner _migrationRunner;
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _slotLocks = new ConcurrentDictionary<string, SemaphoreSlim>();

        public SaveManager(SaveSettings settings, IKeyProvider keyProvider)
            : this(settings, keyProvider, null) { }

        public SaveManager(SaveSettings settings, IKeyProvider keyProvider, MigrationRunner migrationRunner)
        {
            _settings = settings != null ? settings : throw new ArgumentNullException(nameof(settings));
            _keyProvider = keyProvider ?? throw new ArgumentNullException(nameof(keyProvider));

            _serializer = CreateSerializer(settings.Serializer);
            _encryptionProvider = CreateEncryptionProvider(settings.Encryption);
            _backupManager = new BackupManager();
            _migrationRunner = migrationRunner ?? new MigrationRunner(settings.CurrentVersion, settings.MinSupportedVersion);
        }

        public async Task SaveAsync<T>(string slotId, T data)
        {
            if (string.IsNullOrEmpty(slotId)) throw new ArgumentNullException(nameof(slotId));

            var slot = new SaveSlot(slotId, _settings.GetSavePath());
            var semaphore = _slotLocks.GetOrAdd(slotId, _ => new SemaphoreSlim(1, 1));

            await semaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                byte[] serialized = _serializer.Serialize(data);

                byte[] masterKey = _keyProvider.GetMasterKey();
                KeyDerivation.DeriveKeys(masterKey, out byte[] encKey, out byte[] hmacKey);
                CryptoUtility.SecureClear(masterKey);

                try
                {
                    await SaveFile.WriteAsync(slot.FilePath, serialized, encKey, hmacKey,
                        _settings.CurrentVersion, _encryptionProvider, _settings.Encryption)
                        .ConfigureAwait(false);

                    if (_settings.AutoBackup)
                        _backupManager.CreateBackup(slot.FilePath, _settings.MaxBackupCount);
                }
                finally
                {
                    CryptoUtility.SecureClear(encKey);
                    CryptoUtility.SecureClear(hmacKey);
                    CryptoUtility.SecureClear(serialized);
                }
            }
            finally
            {
                semaphore.Release();
            }
        }

        public async Task<T> LoadAsync<T>(string slotId)
        {
            if (string.IsNullOrEmpty(slotId)) throw new ArgumentNullException(nameof(slotId));

            var slot = new SaveSlot(slotId, _settings.GetSavePath());
            var semaphore = _slotLocks.GetOrAdd(slotId, _ => new SemaphoreSlim(1, 1));

            await semaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                byte[] masterKey = _keyProvider.GetMasterKey();
                KeyDerivation.DeriveKeys(masterKey, out byte[] encKey, out byte[] hmacKey);
                CryptoUtility.SecureClear(masterKey);

                byte[] data;
                ushort fileVersion;
                try
                {
                    (data, fileVersion) = await SaveFile.ReadAsync(slot.FilePath, encKey, hmacKey, _encryptionProvider)
                        .ConfigureAwait(false);
                }
                finally
                {
                    CryptoUtility.SecureClear(encKey);
                    CryptoUtility.SecureClear(hmacKey);
                }

                // Apply migrations if the file version is older than current
                if (_migrationRunner.NeedsMigration(fileVersion))
                    data = _migrationRunner.Run(data, fileVersion);

                T result = _serializer.Deserialize<T>(data);
                CryptoUtility.SecureClear(data);
                return result;
            }
            finally
            {
                semaphore.Release();
            }
        }

        public bool Exists(string slotId)
        {
            if (string.IsNullOrEmpty(slotId)) throw new ArgumentNullException(nameof(slotId));
            var slot = new SaveSlot(slotId, _settings.GetSavePath());
            return slot.Exists;
        }

        public void Delete(string slotId)
        {
            if (string.IsNullOrEmpty(slotId)) throw new ArgumentNullException(nameof(slotId));
            var slot = new SaveSlot(slotId, _settings.GetSavePath());
            if (File.Exists(slot.FilePath))
                File.Delete(slot.FilePath);
        }

        public string[] GetAllSlotIds()
        {
            string savePath = _settings.GetSavePath();
            if (!Directory.Exists(savePath))
                return Array.Empty<string>();

            return Directory.GetFiles(savePath, "*.sav")
                .Select(f => Path.GetFileNameWithoutExtension(f))
                .ToArray();
        }

        private static ISerializer CreateSerializer(SerializerType type)
        {
            switch (type)
            {
                case SerializerType.Json:
                    return new JsonUtilitySerializer();
                case SerializerType.MessagePack:
                    throw new NotSupportedException(
                        "MessagePack serializer requires external package setup. Use SerializerType.Json or provide a custom ISerializer.");
                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
        }

        private static IEncryptionProvider CreateEncryptionProvider(EncryptionType type)
        {
            switch (type)
            {
                case EncryptionType.AesCbcHmac:
                    return new AesCbcHmacProvider();
                case EncryptionType.AesGcm:
                    return new AesGcmProvider();
#if UNITY_EDITOR
                case EncryptionType.None:
#pragma warning disable CS0618 // Obsolete warning expected for NullEncryptionProvider
                    return new NullEncryptionProvider();
#pragma warning restore CS0618
#endif
                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
        }
    }
}
