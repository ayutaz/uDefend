using System;
using UnityEngine;

namespace uDefend.AntiCheat
{
    [Serializable]
    public struct ObscuredChar : IEquatable<ObscuredChar>, IComparable<ObscuredChar>
    {
        private const int ChecksumSalt = 0x59E3_A1D6;

        public static event Action OnCheatingDetected;

        [SerializeField] private int _encryptedValue;
        [SerializeField] private int _key;
        [SerializeField] private int _checksum;

        private ObscuredChar(char value)
        {
            _key = ObscuredRandom.Next();
            _encryptedValue = value ^ (char)_key;
            _checksum = value ^ ChecksumSalt;
        }

        private bool IsDefault() => _key == 0 && _encryptedValue == 0 && _checksum == 0;

        private char GetDecrypted()
        {
            if (IsDefault()) return '\0';

            char decrypted = (char)(_encryptedValue ^ (char)_key);
            if ((decrypted ^ ChecksumSalt) != _checksum)
            {
                OnCheatingDetected?.Invoke();
            }
            return decrypted;
        }

        private void SetEncrypted(char value)
        {
            _key = ObscuredRandom.Next();
            _encryptedValue = value ^ (char)_key;
            _checksum = value ^ ChecksumSalt;
        }

        // Implicit conversions
        public static implicit operator ObscuredChar(char value) => new ObscuredChar(value);
        public static implicit operator char(ObscuredChar value) => value.GetDecrypted();

        // Increment/decrement operators
        public static ObscuredChar operator ++(ObscuredChar a)
        {
            char val = (char)(a.GetDecrypted() + 1);
            a.SetEncrypted(val);
            return a;
        }

        public static ObscuredChar operator --(ObscuredChar a)
        {
            char val = (char)(a.GetDecrypted() - 1);
            a.SetEncrypted(val);
            return a;
        }

        // Comparison operators
        public static bool operator ==(ObscuredChar a, ObscuredChar b) => a.GetDecrypted() == b.GetDecrypted();
        public static bool operator !=(ObscuredChar a, ObscuredChar b) => a.GetDecrypted() != b.GetDecrypted();
        public static bool operator <(ObscuredChar a, ObscuredChar b) => a.GetDecrypted() < b.GetDecrypted();
        public static bool operator >(ObscuredChar a, ObscuredChar b) => a.GetDecrypted() > b.GetDecrypted();
        public static bool operator <=(ObscuredChar a, ObscuredChar b) => a.GetDecrypted() <= b.GetDecrypted();
        public static bool operator >=(ObscuredChar a, ObscuredChar b) => a.GetDecrypted() >= b.GetDecrypted();

        // IEquatable<ObscuredChar>
        public bool Equals(ObscuredChar other) => GetDecrypted() == other.GetDecrypted();

        // IComparable<ObscuredChar>
        public int CompareTo(ObscuredChar other) => GetDecrypted().CompareTo(other.GetDecrypted());

        public override string ToString() => GetDecrypted().ToString();
        public override int GetHashCode() => GetDecrypted().GetHashCode();
        public override bool Equals(object obj) => obj is ObscuredChar other && Equals(other);
    }
}
