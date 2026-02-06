using System;
using UnityEngine;

namespace uDefend.AntiCheat
{
    [Serializable]
    public struct ObscuredInt : IEquatable<ObscuredInt>, IComparable<ObscuredInt>, IFormattable
    {
        private const int ChecksumSalt = 0x5A3E_F1C7;

        public static event Action OnCheatingDetected;

        [SerializeField] private int _encryptedValue;
        [SerializeField] private int _key;
        [SerializeField] private int _checksum;

        private ObscuredInt(int value)
        {
            _key = ObscuredRandom.Next();
            _encryptedValue = value ^ _key;
            _checksum = value ^ ChecksumSalt;
        }

        private bool IsDefault() => _key == 0 && _encryptedValue == 0 && _checksum == 0;

        private int GetDecrypted()
        {
            if (IsDefault()) return 0;

            int decrypted = _encryptedValue ^ _key;
            if ((decrypted ^ ChecksumSalt) != _checksum)
            {
                OnCheatingDetected?.Invoke();
            }
            return decrypted;
        }

        private void SetEncrypted(int value)
        {
            _key = ObscuredRandom.Next();
            _encryptedValue = value ^ _key;
            _checksum = value ^ ChecksumSalt;
        }

        // Implicit conversions
        public static implicit operator ObscuredInt(int value) => new ObscuredInt(value);
        public static implicit operator int(ObscuredInt value) => value.GetDecrypted();

        // Arithmetic operators
        public static ObscuredInt operator +(ObscuredInt a, ObscuredInt b) => new ObscuredInt(a.GetDecrypted() + b.GetDecrypted());
        public static ObscuredInt operator -(ObscuredInt a, ObscuredInt b) => new ObscuredInt(a.GetDecrypted() - b.GetDecrypted());
        public static ObscuredInt operator *(ObscuredInt a, ObscuredInt b) => new ObscuredInt(a.GetDecrypted() * b.GetDecrypted());
        public static ObscuredInt operator /(ObscuredInt a, ObscuredInt b) => new ObscuredInt(a.GetDecrypted() / b.GetDecrypted());
        public static ObscuredInt operator %(ObscuredInt a, ObscuredInt b) => new ObscuredInt(a.GetDecrypted() % b.GetDecrypted());

        public static ObscuredInt operator ++(ObscuredInt a)
        {
            int val = a.GetDecrypted() + 1;
            a.SetEncrypted(val);
            return a;
        }

        public static ObscuredInt operator --(ObscuredInt a)
        {
            int val = a.GetDecrypted() - 1;
            a.SetEncrypted(val);
            return a;
        }

        // Comparison operators
        public static bool operator ==(ObscuredInt a, ObscuredInt b) => a.GetDecrypted() == b.GetDecrypted();
        public static bool operator !=(ObscuredInt a, ObscuredInt b) => a.GetDecrypted() != b.GetDecrypted();
        public static bool operator <(ObscuredInt a, ObscuredInt b) => a.GetDecrypted() < b.GetDecrypted();
        public static bool operator >(ObscuredInt a, ObscuredInt b) => a.GetDecrypted() > b.GetDecrypted();
        public static bool operator <=(ObscuredInt a, ObscuredInt b) => a.GetDecrypted() <= b.GetDecrypted();
        public static bool operator >=(ObscuredInt a, ObscuredInt b) => a.GetDecrypted() >= b.GetDecrypted();

        // IEquatable<ObscuredInt>
        public bool Equals(ObscuredInt other) => GetDecrypted() == other.GetDecrypted();

        // IComparable<ObscuredInt>
        public int CompareTo(ObscuredInt other) => GetDecrypted().CompareTo(other.GetDecrypted());

        // IFormattable
        public string ToString(string format, IFormatProvider formatProvider) => GetDecrypted().ToString(format, formatProvider);

        public override string ToString() => GetDecrypted().ToString();
        public override int GetHashCode() => GetDecrypted().GetHashCode();
        public override bool Equals(object obj) => obj is ObscuredInt other && Equals(other);
    }
}
