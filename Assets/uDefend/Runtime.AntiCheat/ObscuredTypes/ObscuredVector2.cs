using System;
using UnityEngine;

namespace uDefend.AntiCheat
{
    [Serializable]
    public struct ObscuredVector2 : IEquatable<ObscuredVector2>
    {
        private const int ChecksumSalt = 0x2B7F_C4D8;

        public static event Action OnCheatingDetected;

        [SerializeField] private int _encryptedX;
        [SerializeField] private int _encryptedY;
        [SerializeField] private int _keyX;
        [SerializeField] private int _keyY;
        [SerializeField] private int _checksum;

        private ObscuredVector2(Vector2 value)
        {
            int bitsX = BitConverter.SingleToInt32Bits(value.x);
            int bitsY = BitConverter.SingleToInt32Bits(value.y);
            _keyX = ObscuredRandom.Next();
            _keyY = ObscuredRandom.Next();
            _encryptedX = bitsX ^ _keyX;
            _encryptedY = bitsY ^ _keyY;
            _checksum = bitsX ^ bitsY ^ ChecksumSalt;
        }

        private bool IsDefault() =>
            _keyX == 0 && _keyY == 0 &&
            _encryptedX == 0 && _encryptedY == 0 &&
            _checksum == 0;

        private Vector2 GetDecrypted()
        {
            if (IsDefault()) return Vector2.zero;

            int bitsX = _encryptedX ^ _keyX;
            int bitsY = _encryptedY ^ _keyY;
            if ((bitsX ^ bitsY ^ ChecksumSalt) != _checksum)
            {
                OnCheatingDetected?.Invoke();
            }
            return new Vector2(
                BitConverter.Int32BitsToSingle(bitsX),
                BitConverter.Int32BitsToSingle(bitsY));
        }

        private void SetEncrypted(Vector2 value)
        {
            int bitsX = BitConverter.SingleToInt32Bits(value.x);
            int bitsY = BitConverter.SingleToInt32Bits(value.y);
            _keyX = ObscuredRandom.Next();
            _keyY = ObscuredRandom.Next();
            _encryptedX = bitsX ^ _keyX;
            _encryptedY = bitsY ^ _keyY;
            _checksum = bitsX ^ bitsY ^ ChecksumSalt;
        }

        // Implicit conversions
        public static implicit operator ObscuredVector2(Vector2 value) => new ObscuredVector2(value);
        public static implicit operator Vector2(ObscuredVector2 value) => value.GetDecrypted();

        // Arithmetic operators
        public static ObscuredVector2 operator +(ObscuredVector2 a, ObscuredVector2 b) => new ObscuredVector2(a.GetDecrypted() + b.GetDecrypted());
        public static ObscuredVector2 operator -(ObscuredVector2 a, ObscuredVector2 b) => new ObscuredVector2(a.GetDecrypted() - b.GetDecrypted());
        public static ObscuredVector2 operator *(ObscuredVector2 a, float d) => new ObscuredVector2(a.GetDecrypted() * d);
        public static ObscuredVector2 operator *(float d, ObscuredVector2 a) => new ObscuredVector2(d * a.GetDecrypted());
        public static ObscuredVector2 operator /(ObscuredVector2 a, float d) => new ObscuredVector2(a.GetDecrypted() / d);

        // Equality operators
        public static bool operator ==(ObscuredVector2 a, ObscuredVector2 b) => a.GetDecrypted() == b.GetDecrypted();
        public static bool operator !=(ObscuredVector2 a, ObscuredVector2 b) => a.GetDecrypted() != b.GetDecrypted();

        // IEquatable<ObscuredVector2>
        public bool Equals(ObscuredVector2 other) => GetDecrypted() == other.GetDecrypted();

        public override string ToString() => GetDecrypted().ToString();
        public override int GetHashCode() => GetDecrypted().GetHashCode();
        public override bool Equals(object obj) => obj is ObscuredVector2 other && Equals(other);
    }
}
