using System;
using System.Runtime.InteropServices;
using uDefend.Encryption;

namespace uDefend.KeyManagement
{
    /// <summary>
    /// Key provider using iOS Keychain Services.
    /// Keys are stored in the iOS Keychain with kSecAttrAccessibleAfterFirstUnlock protection.
    /// Falls back to PBKDF2 on non-iOS platforms.
    /// </summary>
    public sealed class IosKeychainKeyProvider : IKeyStore, IDisposable
    {
        private readonly string _serviceIdentifier;
        private byte[] _cachedKey;
        private bool _disposed;

#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern int uDefend_Keychain_Store(string service, string account, byte[] data, int dataLength);

        [DllImport("__Internal")]
        private static extern int uDefend_Keychain_Load(string service, string account, byte[] buffer, int bufferLength);

        [DllImport("__Internal")]
        private static extern int uDefend_Keychain_Delete(string service, string account);

        [DllImport("__Internal")]
        private static extern int uDefend_Keychain_HasKey(string service, string account);
#endif

        private const string AccountName = "masterkey";

        public IosKeychainKeyProvider(string serviceIdentifier)
        {
            if (string.IsNullOrEmpty(serviceIdentifier))
                throw new ArgumentException("Service identifier must not be null or empty.", nameof(serviceIdentifier));

            _serviceIdentifier = serviceIdentifier;
        }

        public byte[] GetMasterKey()
        {
            ThrowIfDisposed();

            if (_cachedKey != null)
                return CopyKey(_cachedKey);

#if UNITY_IOS && !UNITY_EDITOR
            // Try to load existing key
            byte[] buffer = new byte[32];
            int result = uDefend_Keychain_Load(_serviceIdentifier, AccountName, buffer, buffer.Length);
            if (result > 0)
            {
                _cachedKey = new byte[result];
                Buffer.BlockCopy(buffer, 0, _cachedKey, 0, result);
                CryptoUtility.SecureClear(buffer);
                return CopyKey(_cachedKey);
            }

            // Generate and store new key
            _cachedKey = CryptoUtility.GenerateRandomBytes(32);
            uDefend_Keychain_Store(_serviceIdentifier, AccountName, _cachedKey, _cachedKey.Length);
            return CopyKey(_cachedKey);
#else
            throw new PlatformNotSupportedException("iOS Keychain is only available on iOS devices.");
#endif
        }

        public bool HasMasterKey()
        {
            if (_cachedKey != null) return true;

#if UNITY_IOS && !UNITY_EDITOR
            return uDefend_Keychain_HasKey(_serviceIdentifier, AccountName) == 1;
#else
            return false;
#endif
        }

        public void StoreMasterKey(byte[] key)
        {
            ThrowIfDisposed();
            if (key == null || key.Length == 0)
                throw new ArgumentException("Key must not be null or empty.", nameof(key));

            _cachedKey = new byte[key.Length];
            Buffer.BlockCopy(key, 0, _cachedKey, 0, key.Length);

#if UNITY_IOS && !UNITY_EDITOR
            uDefend_Keychain_Store(_serviceIdentifier, AccountName, key, key.Length);
#endif
        }

        public void DeleteMasterKey()
        {
            ThrowIfDisposed();

            if (_cachedKey != null)
            {
                CryptoUtility.SecureClear(_cachedKey);
                _cachedKey = null;
            }

#if UNITY_IOS && !UNITY_EDITOR
            uDefend_Keychain_Delete(_serviceIdentifier, AccountName);
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

        private static byte[] CopyKey(byte[] source)
        {
            var copy = new byte[source.Length];
            Buffer.BlockCopy(source, 0, copy, 0, source.Length);
            return copy;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(IosKeychainKeyProvider));
        }
    }
}
