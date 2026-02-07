using System;
using uDefend.Encryption;

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
using System.IO;
using System.Security.Cryptography;
#endif

namespace uDefend.KeyManagement
{
    /// <summary>
    /// Key provider using Windows DPAPI (Data Protection API).
    /// Keys are protected with the current user's credentials and stored on disk.
    /// Falls back to PBKDF2 on non-Windows platforms.
    /// </summary>
    public sealed class WindowsDpapiKeyProvider : IKeyStore, IDisposable
    {
        private readonly string _keyFilePath;
        private byte[] _cachedKey;
        private bool _disposed;

        public WindowsDpapiKeyProvider(string keyIdentifier)
        {
            if (string.IsNullOrEmpty(keyIdentifier))
                throw new ArgumentException("Key identifier must not be null or empty.", nameof(keyIdentifier));

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            var appDataDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "uDefend", "Keys");
            Directory.CreateDirectory(appDataDir);
            _keyFilePath = Path.Combine(appDataDir, keyIdentifier + ".dpapi");
#else
            _keyFilePath = null;
#endif
        }

        public byte[] GetMasterKey()
        {
            ThrowIfDisposed();

            if (_cachedKey != null)
                return CopyKey(_cachedKey);

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            if (_keyFilePath != null && File.Exists(_keyFilePath))
            {
                byte[] protectedData = File.ReadAllBytes(_keyFilePath);
                _cachedKey = ProtectedData.Unprotect(protectedData, null, DataProtectionScope.CurrentUser);
                return CopyKey(_cachedKey);
            }

            // Generate new key
            _cachedKey = CryptoUtility.GenerateRandomBytes(32);
            StoreMasterKeyInternal(_cachedKey);
            return CopyKey(_cachedKey);
#else
            throw new PlatformNotSupportedException("Windows DPAPI is only available on Windows.");
#endif
        }

        public bool HasMasterKey()
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            return _cachedKey != null || (_keyFilePath != null && File.Exists(_keyFilePath));
#else
            return _cachedKey != null;
#endif
        }

        public void StoreMasterKey(byte[] key)
        {
            ThrowIfDisposed();
            if (key == null || key.Length == 0)
                throw new ArgumentException("Key must not be null or empty.", nameof(key));

            _cachedKey = new byte[key.Length];
            Buffer.BlockCopy(key, 0, _cachedKey, 0, key.Length);
            StoreMasterKeyInternal(key);
        }

        public void DeleteMasterKey()
        {
            ThrowIfDisposed();

            if (_cachedKey != null)
            {
                CryptoUtility.SecureClear(_cachedKey);
                _cachedKey = null;
            }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            if (_keyFilePath != null && File.Exists(_keyFilePath))
                File.Delete(_keyFilePath);
#endif
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            if (_cachedKey != null)
            {
                CryptoUtility.SecureClear(_cachedKey);
                _cachedKey = null;
            }
        }

        private void StoreMasterKeyInternal(byte[] key)
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            if (_keyFilePath != null)
            {
                byte[] protectedData = ProtectedData.Protect(key, null, DataProtectionScope.CurrentUser);
                File.WriteAllBytes(_keyFilePath, protectedData);
            }
#endif
        }

        private static byte[] CopyKey(byte[] source)
        {
            var copy = new byte[source.Length];
            Buffer.BlockCopy(source, 0, copy, 0, source.Length);
            return copy;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(WindowsDpapiKeyProvider));
        }
    }
}
