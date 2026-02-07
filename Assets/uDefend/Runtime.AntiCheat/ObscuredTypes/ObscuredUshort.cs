using System;
using UnityEngine;

namespace uDefend.AntiCheat
{
    [Serializable]
    public struct ObscuredUshort : IEquatable<ObscuredUshort>, IComparable<ObscuredUshort>, IFormattable
    {
        private const int ChecksumSalt = 0x48BD_E6A3;

        public static event Action OnCheatingDetected;

        [SerializeField] private int _encryptedValue;
        [SerializeField] private int _key;
        [SerializeField] private int _checksum;
        [SerializeField] private int _fakeValue;

        private ObscuredUshort(ushort value)
        {
            _key = ObscuredRandom.Next();
            _encryptedValue = value ^ (ushort)_key;
            _checksum = value ^ ChecksumSalt;
            _fakeValue = value;
        }

        private bool IsDefault() => _key == 0 && _encryptedValue == 0 && _checksum == 0;

        private ushort GetDecrypted()
        {
            if (IsDefault()) return 0;

            ushort decrypted = (ushort)(_encryptedValue ^ (ushort)_key);

            if ((decrypted ^ ChecksumSalt) != _checksum || _fakeValue != decrypted)
            {
                OnCheatingDetected?.Invoke();
            }

            int newKey = ObscuredRandom.Next();
            _encryptedValue = decrypted ^ (ushort)newKey;
            _key = newKey;

            return decrypted;
        }

        private void SetEncrypted(ushort value)
        {
            _key = ObscuredRandom.Next();
            _encryptedValue = value ^ (ushort)_key;
            _checksum = value ^ ChecksumSalt;
            _fakeValue = value;
        }

        // Implicit conversions
        public static implicit operator ObscuredUshort(ushort value) => new ObscuredUshort(value);
        public static implicit operator ushort(ObscuredUshort value) => value.GetDecrypted();

        // Arithmetic operators
        public static ObscuredUshort operator +(ObscuredUshort a, ObscuredUshort b) => new ObscuredUshort((ushort)(a.GetDecrypted() + b.GetDecrypted()));
        public static ObscuredUshort operator -(ObscuredUshort a, ObscuredUshort b) => new ObscuredUshort((ushort)(a.GetDecrypted() - b.GetDecrypted()));
        public static ObscuredUshort operator *(ObscuredUshort a, ObscuredUshort b) => new ObscuredUshort((ushort)(a.GetDecrypted() * b.GetDecrypted()));
        public static ObscuredUshort operator /(ObscuredUshort a, ObscuredUshort b) => new ObscuredUshort((ushort)(a.GetDecrypted() / b.GetDecrypted()));
        public static ObscuredUshort operator %(ObscuredUshort a, ObscuredUshort b) => new ObscuredUshort((ushort)(a.GetDecrypted() % b.GetDecrypted()));

        public static ObscuredUshort operator ++(ObscuredUshort a)
        {
            ushort val = (ushort)(a.GetDecrypted() + 1);
            a.SetEncrypted(val);
            return a;
        }

        public static ObscuredUshort operator --(ObscuredUshort a)
        {
            ushort val = (ushort)(a.GetDecrypted() - 1);
            a.SetEncrypted(val);
            return a;
        }

        // Comparison operators
        public static bool operator ==(ObscuredUshort a, ObscuredUshort b) => a.GetDecrypted() == b.GetDecrypted();
        public static bool operator !=(ObscuredUshort a, ObscuredUshort b) => a.GetDecrypted() != b.GetDecrypted();
        public static bool operator <(ObscuredUshort a, ObscuredUshort b) => a.GetDecrypted() < b.GetDecrypted();
        public static bool operator >(ObscuredUshort a, ObscuredUshort b) => a.GetDecrypted() > b.GetDecrypted();
        public static bool operator <=(ObscuredUshort a, ObscuredUshort b) => a.GetDecrypted() <= b.GetDecrypted();
        public static bool operator >=(ObscuredUshort a, ObscuredUshort b) => a.GetDecrypted() >= b.GetDecrypted();

        // IEquatable<ObscuredUshort>
        public bool Equals(ObscuredUshort other) => GetDecrypted() == other.GetDecrypted();

        // IComparable<ObscuredUshort>
        public int CompareTo(ObscuredUshort other) => GetDecrypted().CompareTo(other.GetDecrypted());

        // IFormattable
        public string ToString(string format, IFormatProvider formatProvider) => GetDecrypted().ToString(format, formatProvider);

        public override string ToString() => GetDecrypted().ToString();
        public override int GetHashCode() => GetDecrypted().GetHashCode();
        public override bool Equals(object obj) => obj is ObscuredUshort other && Equals(other);
    }
}
