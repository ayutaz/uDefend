using System;
using UnityEngine;

namespace uDefend.AntiCheat
{
    [Serializable]
    public struct ObscuredVector2Int : IEquatable<ObscuredVector2Int>
    {
        private const int ChecksumSalt = 0x1C4A_F7D3;

        public static event Action OnCheatingDetected;

        [SerializeField] private int _encryptedX;
        [SerializeField] private int _encryptedY;
        [SerializeField] private int _keyX;
        [SerializeField] private int _keyY;
        [SerializeField] private int _checksum;
        [SerializeField] private int _fakeX;
        [SerializeField] private int _fakeY;
        [SerializeField] private int _decoyKey;

        private ObscuredVector2Int(Vector2Int value)
        {
            _keyX = ObscuredRandom.Next();
            _keyY = ObscuredRandom.Next();
            _encryptedX = value.x ^ _keyX;
            _encryptedY = value.y ^ _keyY;
            _checksum = value.x ^ value.y ^ ChecksumSalt;
            _decoyKey = ObscuredRandom.Next();
            _fakeX = value.x ^ _decoyKey;
            _fakeY = value.y ^ _decoyKey;
        }

        private bool IsDefault() =>
            _keyX == 0 && _keyY == 0 &&
            _encryptedX == 0 && _encryptedY == 0 &&
            _checksum == 0;

        private Vector2Int GetDecrypted()
        {
            if (IsDefault()) return Vector2Int.zero;

            int x = _encryptedX ^ _keyX;
            int y = _encryptedY ^ _keyY;

            bool checksumFailed = (x ^ y ^ ChecksumSalt) != _checksum;
            bool decoyTampered = (_fakeX ^ _decoyKey) != x || (_fakeY ^ _decoyKey) != y;

            if (checksumFailed || decoyTampered)
            {
                OnCheatingDetected?.Invoke();
            }

            int newKeyX = ObscuredRandom.Next();
            int newKeyY = ObscuredRandom.Next();
            _encryptedX = x ^ newKeyX;
            _encryptedY = y ^ newKeyY;
            _keyX = newKeyX;
            _keyY = newKeyY;

            return new Vector2Int(x, y);
        }

        private void SetEncrypted(Vector2Int value)
        {
            _keyX = ObscuredRandom.Next();
            _keyY = ObscuredRandom.Next();
            _encryptedX = value.x ^ _keyX;
            _encryptedY = value.y ^ _keyY;
            _checksum = value.x ^ value.y ^ ChecksumSalt;
            _decoyKey = ObscuredRandom.Next();
            _fakeX = value.x ^ _decoyKey;
            _fakeY = value.y ^ _decoyKey;
        }

        // Implicit conversions
        public static implicit operator ObscuredVector2Int(Vector2Int value) => new ObscuredVector2Int(value);
        public static implicit operator Vector2Int(ObscuredVector2Int value) => value.GetDecrypted();

        // Arithmetic operators
        public static ObscuredVector2Int operator +(ObscuredVector2Int a, ObscuredVector2Int b) => new ObscuredVector2Int(a.GetDecrypted() + b.GetDecrypted());
        public static ObscuredVector2Int operator -(ObscuredVector2Int a, ObscuredVector2Int b) => new ObscuredVector2Int(a.GetDecrypted() - b.GetDecrypted());
        public static ObscuredVector2Int operator *(ObscuredVector2Int a, int d) => new ObscuredVector2Int(a.GetDecrypted() * d);
        public static ObscuredVector2Int operator *(int d, ObscuredVector2Int a) => new ObscuredVector2Int(d * a.GetDecrypted());
        public static ObscuredVector2Int operator /(ObscuredVector2Int a, int d) => new ObscuredVector2Int(a.GetDecrypted() / d);

        // Equality operators
        public static bool operator ==(ObscuredVector2Int a, ObscuredVector2Int b) => a.GetDecrypted() == b.GetDecrypted();
        public static bool operator !=(ObscuredVector2Int a, ObscuredVector2Int b) => a.GetDecrypted() != b.GetDecrypted();

        // IEquatable<ObscuredVector2Int>
        public bool Equals(ObscuredVector2Int other) => GetDecrypted() == other.GetDecrypted();

        public override string ToString() => GetDecrypted().ToString();
        public override int GetHashCode() => GetDecrypted().GetHashCode();
        public override bool Equals(object obj) => obj is ObscuredVector2Int other && Equals(other);
    }
}
