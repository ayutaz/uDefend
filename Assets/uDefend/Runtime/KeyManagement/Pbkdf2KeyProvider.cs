using System;
using System.Security.Cryptography;
using uDefend.Encryption;

namespace uDefend.KeyManagement
{
    public sealed class Pbkdf2KeyProvider : IKeyProvider, IDisposable
    {
        private const int Iterations = 600_000;
        private const int SaltSize = 16;
        private const int DerivedKeySize = 32;
        private static readonly HashAlgorithmName HashAlgorithm = HashAlgorithmName.SHA256;

        private byte[] _passphrase;
        private byte[] _salt;
        private byte[] _cachedKey;
        private bool _disposed;

        public Pbkdf2KeyProvider(byte[] passphrase) : this(passphrase, null) { }

        public Pbkdf2KeyProvider(byte[] passphrase, byte[] salt)
        {
            if (passphrase == null || passphrase.Length == 0)
                throw new ArgumentException("Passphrase must not be null or empty.", nameof(passphrase));

            _passphrase = new byte[passphrase.Length];
            Buffer.BlockCopy(passphrase, 0, _passphrase, 0, passphrase.Length);

            if (salt != null)
            {
                _salt = new byte[salt.Length];
                Buffer.BlockCopy(salt, 0, _salt, 0, salt.Length);
            }
        }

        public byte[] GetMasterKey()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(Pbkdf2KeyProvider));

            if (_cachedKey == null)
            {
                if (_salt == null)
                    _salt = CryptoUtility.GenerateRandomBytes(SaltSize);

                using (var pbkdf2 = new Rfc2898DeriveBytes(_passphrase, _salt, Iterations, HashAlgorithm))
                {
                    _cachedKey = pbkdf2.GetBytes(DerivedKeySize);
                }
            }

            // Return defensive copy so callers cannot corrupt the cached key
            var copy = new byte[_cachedKey.Length];
            Buffer.BlockCopy(_cachedKey, 0, copy, 0, _cachedKey.Length);
            return copy;
        }

        public bool HasMasterKey()
        {
            return _cachedKey != null || _salt != null;
        }

        public byte[] GetSalt()
        {
            if (_salt == null)
                _salt = CryptoUtility.GenerateRandomBytes(SaltSize);

            var copy = new byte[_salt.Length];
            Buffer.BlockCopy(_salt, 0, copy, 0, _salt.Length);
            return copy;
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
            if (_passphrase != null)
            {
                CryptoUtility.SecureClear(_passphrase);
                _passphrase = null;
            }
            if (_salt != null)
            {
                CryptoUtility.SecureClear(_salt);
                _salt = null;
            }
        }
    }
}
