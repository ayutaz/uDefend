using System;
using System.Security.Cryptography;
using uDefend.Encryption;

namespace uDefend.KeyManagement
{
    public sealed class Pbkdf2KeyProvider : IKeyProvider
    {
        private const int Iterations = 600_000;
        private const int SaltSize = 16;
        private const int DerivedKeySize = 32;
        private static readonly HashAlgorithmName HashAlgorithm = HashAlgorithmName.SHA256;

        private readonly byte[] _passphrase;
        private byte[] _salt;
        private byte[] _cachedKey;

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
            if (_cachedKey != null)
                return _cachedKey;

            if (_salt == null)
                _salt = CryptoUtility.GenerateRandomBytes(SaltSize);

            using (var pbkdf2 = new Rfc2898DeriveBytes(_passphrase, _salt, Iterations, HashAlgorithm))
            {
                _cachedKey = pbkdf2.GetBytes(DerivedKeySize);
            }

            return _cachedKey;
        }

        public void StoreMasterKey(byte[] key)
        {
            // PBKDF2 derives keys from passphrase; external keys cannot be stored.
            throw new NotSupportedException(
                "Pbkdf2KeyProvider derives keys from a passphrase. Use a platform key provider to store externally provided keys.");
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
    }
}
