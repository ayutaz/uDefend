using System;
using UnityEngine;

namespace uDefend.AntiCheat
{
    [Serializable]
    public struct ObscuredBool : IEquatable<ObscuredBool>
    {
        private const int ChecksumSalt = 0x3C8F_D62B;

        public static event Action OnCheatingDetected;

        [SerializeField] private int _encryptedValue;
        [SerializeField] private int _key;
        [SerializeField] private int _checksum;

        private ObscuredBool(bool value)
        {
            int intVal = value ? 1 : 0;
            _key = ObscuredRandom.Next();
            _encryptedValue = intVal ^ _key;
            _checksum = intVal ^ ChecksumSalt;
        }

        private bool IsDefault() => _key == 0 && _encryptedValue == 0 && _checksum == 0;

        private bool GetDecrypted()
        {
            if (IsDefault()) return false;

            int intVal = _encryptedValue ^ _key;
            if ((intVal ^ ChecksumSalt) != _checksum)
            {
                OnCheatingDetected?.Invoke();
            }
            return intVal != 0;
        }

        private void SetEncrypted(bool value)
        {
            int intVal = value ? 1 : 0;
            _key = ObscuredRandom.Next();
            _encryptedValue = intVal ^ _key;
            _checksum = intVal ^ ChecksumSalt;
        }

        // Implicit conversions
        public static implicit operator ObscuredBool(bool value) => new ObscuredBool(value);
        public static implicit operator bool(ObscuredBool value) => value.GetDecrypted();

        // Logical operators
        public static bool operator !(ObscuredBool a) => !a.GetDecrypted();
        public static ObscuredBool operator &(ObscuredBool a, ObscuredBool b) => new ObscuredBool(a.GetDecrypted() & b.GetDecrypted());
        public static ObscuredBool operator |(ObscuredBool a, ObscuredBool b) => new ObscuredBool(a.GetDecrypted() | b.GetDecrypted());
        public static ObscuredBool operator ^(ObscuredBool a, ObscuredBool b) => new ObscuredBool(a.GetDecrypted() ^ b.GetDecrypted());

        // Equality operators
        public static bool operator ==(ObscuredBool a, ObscuredBool b) => a.GetDecrypted() == b.GetDecrypted();
        public static bool operator !=(ObscuredBool a, ObscuredBool b) => a.GetDecrypted() != b.GetDecrypted();

        // IEquatable<ObscuredBool>
        public bool Equals(ObscuredBool other) => GetDecrypted() == other.GetDecrypted();

        public override string ToString() => GetDecrypted().ToString();
        public override int GetHashCode() => GetDecrypted().GetHashCode();
        public override bool Equals(object obj) => obj is ObscuredBool other && Equals(other);
    }
}
