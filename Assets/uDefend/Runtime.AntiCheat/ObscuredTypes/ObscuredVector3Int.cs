using System;
using UnityEngine;

namespace uDefend.AntiCheat
{
    [Serializable]
    public struct ObscuredVector3Int : IEquatable<ObscuredVector3Int>
    {
        private const int ChecksumSalt = 0x3F6B_D9A1;

        public static event Action OnCheatingDetected;

        [SerializeField] private int _encryptedX;
        [SerializeField] private int _encryptedY;
        [SerializeField] private int _encryptedZ;
        [SerializeField] private int _keyX;
        [SerializeField] private int _keyY;
        [SerializeField] private int _keyZ;
        [SerializeField] private int _checksum;
        [SerializeField] private int _fakeX;
        [SerializeField] private int _fakeY;
        [SerializeField] private int _fakeZ;

        private ObscuredVector3Int(Vector3Int value)
        {
            _keyX = ObscuredRandom.Next();
            _keyY = ObscuredRandom.Next();
            _keyZ = ObscuredRandom.Next();
            _encryptedX = value.x ^ _keyX;
            _encryptedY = value.y ^ _keyY;
            _encryptedZ = value.z ^ _keyZ;
            _checksum = value.x ^ value.y ^ value.z ^ ChecksumSalt;
            _fakeX = value.x;
            _fakeY = value.y;
            _fakeZ = value.z;
        }

        private bool IsDefault() =>
            _keyX == 0 && _keyY == 0 && _keyZ == 0 &&
            _encryptedX == 0 && _encryptedY == 0 && _encryptedZ == 0 &&
            _checksum == 0;

        private Vector3Int GetDecrypted()
        {
            if (IsDefault()) return Vector3Int.zero;

            int x = _encryptedX ^ _keyX;
            int y = _encryptedY ^ _keyY;
            int z = _encryptedZ ^ _keyZ;

            bool checksumFailed = (x ^ y ^ z ^ ChecksumSalt) != _checksum;
            bool decoyTampered = _fakeX != x || _fakeY != y || _fakeZ != z;

            if (checksumFailed || decoyTampered)
            {
                OnCheatingDetected?.Invoke();
            }

            int newKeyX = ObscuredRandom.Next();
            int newKeyY = ObscuredRandom.Next();
            int newKeyZ = ObscuredRandom.Next();
            _encryptedX = x ^ newKeyX;
            _encryptedY = y ^ newKeyY;
            _encryptedZ = z ^ newKeyZ;
            _keyX = newKeyX;
            _keyY = newKeyY;
            _keyZ = newKeyZ;

            return new Vector3Int(x, y, z);
        }

        private void SetEncrypted(Vector3Int value)
        {
            _keyX = ObscuredRandom.Next();
            _keyY = ObscuredRandom.Next();
            _keyZ = ObscuredRandom.Next();
            _encryptedX = value.x ^ _keyX;
            _encryptedY = value.y ^ _keyY;
            _encryptedZ = value.z ^ _keyZ;
            _checksum = value.x ^ value.y ^ value.z ^ ChecksumSalt;
            _fakeX = value.x;
            _fakeY = value.y;
            _fakeZ = value.z;
        }

        // Implicit conversions
        public static implicit operator ObscuredVector3Int(Vector3Int value) => new ObscuredVector3Int(value);
        public static implicit operator Vector3Int(ObscuredVector3Int value) => value.GetDecrypted();

        // Arithmetic operators
        public static ObscuredVector3Int operator +(ObscuredVector3Int a, ObscuredVector3Int b) => new ObscuredVector3Int(a.GetDecrypted() + b.GetDecrypted());
        public static ObscuredVector3Int operator -(ObscuredVector3Int a, ObscuredVector3Int b) => new ObscuredVector3Int(a.GetDecrypted() - b.GetDecrypted());
        public static ObscuredVector3Int operator *(ObscuredVector3Int a, int d) => new ObscuredVector3Int(a.GetDecrypted() * d);
        public static ObscuredVector3Int operator *(int d, ObscuredVector3Int a) => new ObscuredVector3Int(d * a.GetDecrypted());
        public static ObscuredVector3Int operator /(ObscuredVector3Int a, int d) => new ObscuredVector3Int(a.GetDecrypted() / d);

        // Equality operators
        public static bool operator ==(ObscuredVector3Int a, ObscuredVector3Int b) => a.GetDecrypted() == b.GetDecrypted();
        public static bool operator !=(ObscuredVector3Int a, ObscuredVector3Int b) => a.GetDecrypted() != b.GetDecrypted();

        // IEquatable<ObscuredVector3Int>
        public bool Equals(ObscuredVector3Int other) => GetDecrypted() == other.GetDecrypted();

        public override string ToString() => GetDecrypted().ToString();
        public override int GetHashCode() => GetDecrypted().GetHashCode();
        public override bool Equals(object obj) => obj is ObscuredVector3Int other && Equals(other);
    }
}
