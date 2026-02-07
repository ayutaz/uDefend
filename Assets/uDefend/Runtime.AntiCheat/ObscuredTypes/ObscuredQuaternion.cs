using System;
using UnityEngine;

namespace uDefend.AntiCheat
{
    [Serializable]
    public struct ObscuredQuaternion : IEquatable<ObscuredQuaternion>
    {
        private const int ChecksumSalt = 0x5E2C_A8F6;

        public static event Action OnCheatingDetected;

        [SerializeField] private int _encryptedX;
        [SerializeField] private int _encryptedY;
        [SerializeField] private int _encryptedZ;
        [SerializeField] private int _encryptedW;
        [SerializeField] private int _keyX;
        [SerializeField] private int _keyY;
        [SerializeField] private int _keyZ;
        [SerializeField] private int _keyW;
        [SerializeField] private int _checksum;
        [SerializeField] private int _fakeX;
        [SerializeField] private int _fakeY;
        [SerializeField] private int _fakeZ;
        [SerializeField] private int _fakeW;
        [SerializeField] private int _decoyKey;

        private ObscuredQuaternion(Quaternion value)
        {
            int bitsX = BitConverter.SingleToInt32Bits(value.x);
            int bitsY = BitConverter.SingleToInt32Bits(value.y);
            int bitsZ = BitConverter.SingleToInt32Bits(value.z);
            int bitsW = BitConverter.SingleToInt32Bits(value.w);
            _keyX = ObscuredRandom.Next();
            _keyY = ObscuredRandom.Next();
            _keyZ = ObscuredRandom.Next();
            _keyW = ObscuredRandom.Next();
            _encryptedX = bitsX ^ _keyX;
            _encryptedY = bitsY ^ _keyY;
            _encryptedZ = bitsZ ^ _keyZ;
            _encryptedW = bitsW ^ _keyW;
            _checksum = bitsX ^ bitsY ^ bitsZ ^ bitsW ^ ChecksumSalt;
            _decoyKey = ObscuredRandom.Next();
            _fakeX = bitsX ^ _decoyKey;
            _fakeY = bitsY ^ _decoyKey;
            _fakeZ = bitsZ ^ _decoyKey;
            _fakeW = bitsW ^ _decoyKey;
        }

        private bool IsDefault() =>
            _keyX == 0 && _keyY == 0 && _keyZ == 0 && _keyW == 0 &&
            _encryptedX == 0 && _encryptedY == 0 && _encryptedZ == 0 && _encryptedW == 0 &&
            _checksum == 0;

        private Quaternion GetDecrypted()
        {
            if (IsDefault()) return Quaternion.identity;

            int bitsX = _encryptedX ^ _keyX;
            int bitsY = _encryptedY ^ _keyY;
            int bitsZ = _encryptedZ ^ _keyZ;
            int bitsW = _encryptedW ^ _keyW;

            bool checksumFailed = (bitsX ^ bitsY ^ bitsZ ^ bitsW ^ ChecksumSalt) != _checksum;
            bool decoyTampered = (_fakeX ^ _decoyKey) != bitsX
                              || (_fakeY ^ _decoyKey) != bitsY
                              || (_fakeZ ^ _decoyKey) != bitsZ
                              || (_fakeW ^ _decoyKey) != bitsW;

            if (checksumFailed || decoyTampered)
            {
                OnCheatingDetected?.Invoke();
            }

            int newKeyX = ObscuredRandom.Next();
            int newKeyY = ObscuredRandom.Next();
            int newKeyZ = ObscuredRandom.Next();
            int newKeyW = ObscuredRandom.Next();
            _encryptedX = bitsX ^ newKeyX;
            _encryptedY = bitsY ^ newKeyY;
            _encryptedZ = bitsZ ^ newKeyZ;
            _encryptedW = bitsW ^ newKeyW;
            _keyX = newKeyX;
            _keyY = newKeyY;
            _keyZ = newKeyZ;
            _keyW = newKeyW;

            return new Quaternion(
                BitConverter.Int32BitsToSingle(bitsX),
                BitConverter.Int32BitsToSingle(bitsY),
                BitConverter.Int32BitsToSingle(bitsZ),
                BitConverter.Int32BitsToSingle(bitsW));
        }

        private void SetEncrypted(Quaternion value)
        {
            int bitsX = BitConverter.SingleToInt32Bits(value.x);
            int bitsY = BitConverter.SingleToInt32Bits(value.y);
            int bitsZ = BitConverter.SingleToInt32Bits(value.z);
            int bitsW = BitConverter.SingleToInt32Bits(value.w);
            _keyX = ObscuredRandom.Next();
            _keyY = ObscuredRandom.Next();
            _keyZ = ObscuredRandom.Next();
            _keyW = ObscuredRandom.Next();
            _encryptedX = bitsX ^ _keyX;
            _encryptedY = bitsY ^ _keyY;
            _encryptedZ = bitsZ ^ _keyZ;
            _encryptedW = bitsW ^ _keyW;
            _checksum = bitsX ^ bitsY ^ bitsZ ^ bitsW ^ ChecksumSalt;
            _decoyKey = ObscuredRandom.Next();
            _fakeX = bitsX ^ _decoyKey;
            _fakeY = bitsY ^ _decoyKey;
            _fakeZ = bitsZ ^ _decoyKey;
            _fakeW = bitsW ^ _decoyKey;
        }

        // Implicit conversions
        public static implicit operator ObscuredQuaternion(Quaternion value) => new ObscuredQuaternion(value);
        public static implicit operator Quaternion(ObscuredQuaternion value) => value.GetDecrypted();

        // Quaternion multiplication
        public static ObscuredQuaternion operator *(ObscuredQuaternion a, ObscuredQuaternion b) => new ObscuredQuaternion(a.GetDecrypted() * b.GetDecrypted());

        // Equality operators
        public static bool operator ==(ObscuredQuaternion a, ObscuredQuaternion b) => a.GetDecrypted() == b.GetDecrypted();
        public static bool operator !=(ObscuredQuaternion a, ObscuredQuaternion b) => a.GetDecrypted() != b.GetDecrypted();

        // IEquatable<ObscuredQuaternion>
        public bool Equals(ObscuredQuaternion other) => GetDecrypted() == other.GetDecrypted();

        public override string ToString() => GetDecrypted().ToString();
        public override int GetHashCode() => GetDecrypted().GetHashCode();
        public override bool Equals(object obj) => obj is ObscuredQuaternion other && Equals(other);
    }
}
