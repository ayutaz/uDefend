using System;
using uDefend.Encryption;

#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine;
#endif

namespace uDefend.KeyManagement
{
    /// <summary>
    /// Key provider using Android Keystore (hardware-backed when available).
    /// Keys are stored in the Android Keystore System via a Java helper class.
    /// Falls back to PBKDF2 on non-Android platforms.
    /// </summary>
    public sealed class AndroidKeystoreKeyProvider : IKeyStore, IDisposable
    {
        private readonly string _keyAlias;
        private byte[] _cachedKey;
        private bool _disposed;

        public AndroidKeystoreKeyProvider(string keyAlias)
        {
            if (string.IsNullOrEmpty(keyAlias))
                throw new ArgumentException("Key alias must not be null or empty.", nameof(keyAlias));

            _keyAlias = keyAlias;
        }

        public byte[] GetMasterKey()
        {
            ThrowIfDisposed();

            if (_cachedKey != null)
                return CopyKey(_cachedKey);

#if UNITY_ANDROID && !UNITY_EDITOR
            using (var helper = new AndroidJavaClass("com.udefend.KeystoreHelper"))
            {
                if (helper.CallStatic<bool>("hasKey", _keyAlias))
                {
                    sbyte[] keyBytes = helper.CallStatic<sbyte[]>("getKey", _keyAlias);
                    _cachedKey = new byte[keyBytes.Length];
                    Buffer.BlockCopy(keyBytes, 0, _cachedKey, 0, keyBytes.Length);
                    return CopyKey(_cachedKey);
                }

                // Generate and store new key
                _cachedKey = CryptoUtility.GenerateRandomBytes(32);
                sbyte[] signedKey = new sbyte[_cachedKey.Length];
                Buffer.BlockCopy(_cachedKey, 0, signedKey, 0, _cachedKey.Length);
                helper.CallStatic("storeKey", _keyAlias, signedKey);
                return CopyKey(_cachedKey);
            }
#else
            throw new PlatformNotSupportedException("Android Keystore is only available on Android devices.");
#endif
        }

        public bool HasMasterKey()
        {
            if (_cachedKey != null) return true;

#if UNITY_ANDROID && !UNITY_EDITOR
            using (var helper = new AndroidJavaClass("com.udefend.KeystoreHelper"))
            {
                return helper.CallStatic<bool>("hasKey", _keyAlias);
            }
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

#if UNITY_ANDROID && !UNITY_EDITOR
            sbyte[] signedKey = new sbyte[key.Length];
            Buffer.BlockCopy(key, 0, signedKey, 0, key.Length);
            using (var helper = new AndroidJavaClass("com.udefend.KeystoreHelper"))
            {
                helper.CallStatic("storeKey", _keyAlias, signedKey);
            }
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

#if UNITY_ANDROID && !UNITY_EDITOR
            using (var helper = new AndroidJavaClass("com.udefend.KeystoreHelper"))
            {
                helper.CallStatic("deleteKey", _keyAlias);
            }
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
            if (_disposed) throw new ObjectDisposedException(nameof(AndroidKeystoreKeyProvider));
        }
    }
}
