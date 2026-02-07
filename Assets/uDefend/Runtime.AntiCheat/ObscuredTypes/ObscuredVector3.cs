using System;
using UnityEngine;

namespace uDefend.AntiCheat
{
    [Serializable]
    public struct ObscuredVector3 : IEquatable<ObscuredVector3>
    {
        private const int ChecksumSalt = unchecked((int)0x8A3D_E5F2);

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
        [SerializeField] private int _decoyKey;

        private ObscuredVector3(Vector3 value)
        {
            int bitsX = BitConverter.SingleToInt32Bits(value.x);
            int bitsY = BitConverter.SingleToInt32Bits(value.y);
            int bitsZ = BitConverter.SingleToInt32Bits(value.z);
            _keyX = ObscuredRandom.Next();
            _keyY = ObscuredRandom.Next();
            _keyZ = ObscuredRandom.Next();
            _encryptedX = bitsX ^ _keyX;
            _encryptedY = bitsY ^ _keyY;
            _encryptedZ = bitsZ ^ _keyZ;
            _checksum = bitsX ^ bitsY ^ bitsZ ^ ChecksumSalt;
            _decoyKey = ObscuredRandom.Next();
            _fakeX = bitsX ^ _decoyKey;
            _fakeY = bitsY ^ _decoyKey;
            _fakeZ = bitsZ ^ _decoyKey;
        }

        private bool IsDefault() =>
            _keyX == 0 && _keyY == 0 && _keyZ == 0 &&
            _encryptedX == 0 && _encryptedY == 0 && _encryptedZ == 0 &&
            _checksum == 0;

        private Vector3 GetDecrypted()
        {
            if (IsDefault()) return Vector3.zero;

            int bitsX = _encryptedX ^ _keyX;
            int bitsY = _encryptedY ^ _keyY;
            int bitsZ = _encryptedZ ^ _keyZ;

            bool checksumFailed = (bitsX ^ bitsY ^ bitsZ ^ ChecksumSalt) != _checksum;
            bool decoyTampered = (_fakeX ^ _decoyKey) != bitsX
                              || (_fakeY ^ _decoyKey) != bitsY
                              || (_fakeZ ^ _decoyKey) != bitsZ;

            if (checksumFailed || decoyTampered)
            {
                OnCheatingDetected?.Invoke();
            }

            int newKeyX = ObscuredRandom.Next();
            int newKeyY = ObscuredRandom.Next();
            int newKeyZ = ObscuredRandom.Next();
            _encryptedX = bitsX ^ newKeyX;
            _encryptedY = bitsY ^ newKeyY;
            _encryptedZ = bitsZ ^ newKeyZ;
            _keyX = newKeyX;
            _keyY = newKeyY;
            _keyZ = newKeyZ;

            return new Vector3(
                BitConverter.Int32BitsToSingle(bitsX),
                BitConverter.Int32BitsToSingle(bitsY),
                BitConverter.Int32BitsToSingle(bitsZ));
        }

        private void SetEncrypted(Vector3 value)
        {
            int bitsX = BitConverter.SingleToInt32Bits(value.x);
            int bitsY = BitConverter.SingleToInt32Bits(value.y);
            int bitsZ = BitConverter.SingleToInt32Bits(value.z);
            _keyX = ObscuredRandom.Next();
            _keyY = ObscuredRandom.Next();
            _keyZ = ObscuredRandom.Next();
            _encryptedX = bitsX ^ _keyX;
            _encryptedY = bitsY ^ _keyY;
            _encryptedZ = bitsZ ^ _keyZ;
            _checksum = bitsX ^ bitsY ^ bitsZ ^ ChecksumSalt;
            _decoyKey = ObscuredRandom.Next();
            _fakeX = bitsX ^ _decoyKey;
            _fakeY = bitsY ^ _decoyKey;
            _fakeZ = bitsZ ^ _decoyKey;
        }

        // Implicit conversions
        public static implicit operator ObscuredVector3(Vector3 value) => new ObscuredVector3(value);
        public static implicit operator Vector3(ObscuredVector3 value) => value.GetDecrypted();

        // Arithmetic operators
        public static ObscuredVector3 operator +(ObscuredVector3 a, ObscuredVector3 b) => new ObscuredVector3(a.GetDecrypted() + b.GetDecrypted());
        public static ObscuredVector3 operator -(ObscuredVector3 a, ObscuredVector3 b) => new ObscuredVector3(a.GetDecrypted() - b.GetDecrypted());
        public static ObscuredVector3 operator *(ObscuredVector3 a, float d) => new ObscuredVector3(a.GetDecrypted() * d);
        public static ObscuredVector3 operator *(float d, ObscuredVector3 a) => new ObscuredVector3(d * a.GetDecrypted());
        public static ObscuredVector3 operator /(ObscuredVector3 a, float d) => new ObscuredVector3(a.GetDecrypted() / d);

        // Equality operators
        public static bool operator ==(ObscuredVector3 a, ObscuredVector3 b) => a.GetDecrypted() == b.GetDecrypted();
        public static bool operator !=(ObscuredVector3 a, ObscuredVector3 b) => a.GetDecrypted() != b.GetDecrypted();

        // IEquatable<ObscuredVector3>
        public bool Equals(ObscuredVector3 other) => GetDecrypted() == other.GetDecrypted();

        public override string ToString() => GetDecrypted().ToString();
        public override int GetHashCode() => GetDecrypted().GetHashCode();
        public override bool Equals(object obj) => obj is ObscuredVector3 other && Equals(other);
    }
}
