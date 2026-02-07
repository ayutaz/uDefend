using System;
using UnityEngine;

namespace uDefend.AntiCheat
{
    [Serializable]
    public struct ObscuredSbyte : IEquatable<ObscuredSbyte>, IComparable<ObscuredSbyte>, IFormattable
    {
        private const int ChecksumSalt = 0x1D5B_C8F4;

        public static event Action OnCheatingDetected;

        [SerializeField] private int _encryptedValue;
        [SerializeField] private int _key;
        [SerializeField] private int _checksum;

        private ObscuredSbyte(sbyte value)
        {
            _key = ObscuredRandom.Next();
            _encryptedValue = value ^ (sbyte)_key;
            _checksum = value ^ ChecksumSalt;
        }

        private bool IsDefault() => _key == 0 && _encryptedValue == 0 && _checksum == 0;

        private sbyte GetDecrypted()
        {
            if (IsDefault()) return 0;

            sbyte decrypted = (sbyte)(_encryptedValue ^ (sbyte)_key);
            if ((decrypted ^ ChecksumSalt) != _checksum)
            {
                OnCheatingDetected?.Invoke();
            }
            return decrypted;
        }

        private void SetEncrypted(sbyte value)
        {
            _key = ObscuredRandom.Next();
            _encryptedValue = value ^ (sbyte)_key;
            _checksum = value ^ ChecksumSalt;
        }

        // Implicit conversions
        public static implicit operator ObscuredSbyte(sbyte value) => new ObscuredSbyte(value);
        public static implicit operator sbyte(ObscuredSbyte value) => value.GetDecrypted();

        // Arithmetic operators
        public static ObscuredSbyte operator +(ObscuredSbyte a, ObscuredSbyte b) => new ObscuredSbyte((sbyte)(a.GetDecrypted() + b.GetDecrypted()));
        public static ObscuredSbyte operator -(ObscuredSbyte a, ObscuredSbyte b) => new ObscuredSbyte((sbyte)(a.GetDecrypted() - b.GetDecrypted()));
        public static ObscuredSbyte operator *(ObscuredSbyte a, ObscuredSbyte b) => new ObscuredSbyte((sbyte)(a.GetDecrypted() * b.GetDecrypted()));
        public static ObscuredSbyte operator /(ObscuredSbyte a, ObscuredSbyte b) => new ObscuredSbyte((sbyte)(a.GetDecrypted() / b.GetDecrypted()));
        public static ObscuredSbyte operator %(ObscuredSbyte a, ObscuredSbyte b) => new ObscuredSbyte((sbyte)(a.GetDecrypted() % b.GetDecrypted()));

        public static ObscuredSbyte operator ++(ObscuredSbyte a)
        {
            sbyte val = (sbyte)(a.GetDecrypted() + 1);
            a.SetEncrypted(val);
            return a;
        }

        public static ObscuredSbyte operator --(ObscuredSbyte a)
        {
            sbyte val = (sbyte)(a.GetDecrypted() - 1);
            a.SetEncrypted(val);
            return a;
        }

        // Comparison operators
        public static bool operator ==(ObscuredSbyte a, ObscuredSbyte b) => a.GetDecrypted() == b.GetDecrypted();
        public static bool operator !=(ObscuredSbyte a, ObscuredSbyte b) => a.GetDecrypted() != b.GetDecrypted();
        public static bool operator <(ObscuredSbyte a, ObscuredSbyte b) => a.GetDecrypted() < b.GetDecrypted();
        public static bool operator >(ObscuredSbyte a, ObscuredSbyte b) => a.GetDecrypted() > b.GetDecrypted();
        public static bool operator <=(ObscuredSbyte a, ObscuredSbyte b) => a.GetDecrypted() <= b.GetDecrypted();
        public static bool operator >=(ObscuredSbyte a, ObscuredSbyte b) => a.GetDecrypted() >= b.GetDecrypted();

        // IEquatable<ObscuredSbyte>
        public bool Equals(ObscuredSbyte other) => GetDecrypted() == other.GetDecrypted();

        // IComparable<ObscuredSbyte>
        public int CompareTo(ObscuredSbyte other) => GetDecrypted().CompareTo(other.GetDecrypted());

        // IFormattable
        public string ToString(string format, IFormatProvider formatProvider) => GetDecrypted().ToString(format, formatProvider);

        public override string ToString() => GetDecrypted().ToString();
        public override int GetHashCode() => GetDecrypted().GetHashCode();
        public override bool Equals(object obj) => obj is ObscuredSbyte other && Equals(other);
    }
}
