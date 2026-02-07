using System;
using UnityEngine;

namespace uDefend.AntiCheat
{
    [Serializable]
    public struct ObscuredString : IEquatable<ObscuredString>
    {
        private const int ChecksumSalt = 0x4E6A_B8D3;

        public static event Action OnCheatingDetected;

        [SerializeField] private char[] _encryptedChars;
        [SerializeField] private int _key;
        [SerializeField] private int _checksum;
        [SerializeField] private int _fakeHash;
        [SerializeField] private int _decoyKey;

        public int Length => _encryptedChars == null ? 0 : _encryptedChars.Length;

        private ObscuredString(string value)
        {
            if (value == null)
            {
                _encryptedChars = null;
                _key = 0;
                _checksum = ChecksumSalt;
                _fakeHash = ChecksumSalt ^ 0;
                _decoyKey = 0;
                return;
            }

            _key = ObscuredRandom.Next();
            _encryptedChars = new char[value.Length];
            for (int i = 0; i < value.Length; i++)
            {
                _encryptedChars[i] = (char)(value[i] ^ (_key + i));
            }
            _checksum = ComputeChecksum(value);
            _decoyKey = ObscuredRandom.Next();
            _fakeHash = ComputeChecksum(value) ^ _decoyKey;
        }

        private string GetDecrypted()
        {
            if (_encryptedChars == null)
            {
                if (!IsDefault())
                {
                    OnCheatingDetected?.Invoke();
                }
                return null;
            }

            char[] decrypted = new char[_encryptedChars.Length];
            for (int i = 0; i < _encryptedChars.Length; i++)
            {
                decrypted[i] = (char)(_encryptedChars[i] ^ (_key + i));
            }

            string result = new string(decrypted);
            Array.Clear(decrypted, 0, decrypted.Length);

            bool checksumFailed = ComputeChecksum(result) != _checksum;
            bool decoyTampered = (ComputeChecksum(result) ^ _decoyKey) != _fakeHash;

            if (checksumFailed || decoyTampered)
            {
                OnCheatingDetected?.Invoke();
            }

            // Note: re-encryption is NOT safe for ObscuredString because _encryptedChars
            // is a reference type (char[]). Struct copy semantics would cause the shared
            // array to be re-encrypted while the original struct retains the old _key.

            return result;
        }

        private void SetEncrypted(string value)
        {
            if (value == null)
            {
                _encryptedChars = null;
                _key = 0;
                _checksum = ChecksumSalt;
                _fakeHash = ChecksumSalt ^ 0;
                _decoyKey = 0;
                return;
            }

            _key = ObscuredRandom.Next();
            _encryptedChars = new char[value.Length];
            for (int i = 0; i < value.Length; i++)
            {
                _encryptedChars[i] = (char)(value[i] ^ (_key + i));
            }
            _checksum = ComputeChecksum(value);
            _decoyKey = ObscuredRandom.Next();
            _fakeHash = ComputeChecksum(value) ^ _decoyKey;
        }

        private static int ComputeChecksum(string value)
        {
            if (value == null) return ChecksumSalt;
            int hash = ChecksumSalt;
            for (int i = 0; i < value.Length; i++)
            {
                hash = hash * 31 + value[i];
            }
            return hash;
        }

        private bool IsDefault() => _key == 0 && _encryptedChars == null && _checksum == 0;

        // Implicit conversions
        public static implicit operator ObscuredString(string value) => new ObscuredString(value);
        public static implicit operator string(ObscuredString value) => value.GetDecrypted();

        // Concatenation
        public static ObscuredString operator +(ObscuredString a, ObscuredString b) => new ObscuredString(a.GetDecrypted() + b.GetDecrypted());
        public static ObscuredString operator +(ObscuredString a, string b) => new ObscuredString(a.GetDecrypted() + b);
        public static ObscuredString operator +(string a, ObscuredString b) => new ObscuredString(a + b.GetDecrypted());

        // Equality operators
        public static bool operator ==(ObscuredString a, ObscuredString b) => string.Equals(a.GetDecrypted(), b.GetDecrypted());
        public static bool operator !=(ObscuredString a, ObscuredString b) => !string.Equals(a.GetDecrypted(), b.GetDecrypted());

        // IEquatable<ObscuredString>
        public bool Equals(ObscuredString other) => string.Equals(GetDecrypted(), other.GetDecrypted());

        public override string ToString() => GetDecrypted() ?? string.Empty;
        public override int GetHashCode() => GetDecrypted()?.GetHashCode() ?? 0;
        public override bool Equals(object obj) => obj is ObscuredString other && Equals(other);
    }
}
