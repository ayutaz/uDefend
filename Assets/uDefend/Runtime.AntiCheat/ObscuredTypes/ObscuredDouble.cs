using System;
using UnityEngine;

namespace uDefend.AntiCheat
{
    [Serializable]
    public struct ObscuredDouble : IEquatable<ObscuredDouble>, IComparable<ObscuredDouble>, IFormattable
    {
        private const long ChecksumSalt = 0x6C4E_B3A7_D9F2_851EL;

        public static event Action OnCheatingDetected;

        [SerializeField] private long _encryptedValue;
        [SerializeField] private long _key;
        [SerializeField] private long _checksum;
        [SerializeField] private long _fakeValue;
        [SerializeField] private long _decoyKey;

        private ObscuredDouble(double value)
        {
            long bits = BitConverter.DoubleToInt64Bits(value);
            _key = ObscuredRandom.NextLong();
            _encryptedValue = bits ^ _key;
            _checksum = bits ^ ChecksumSalt;
            _decoyKey = ObscuredRandom.NextLong();
            _fakeValue = bits ^ _decoyKey;
        }

        private bool IsDefault() => _key == 0 && _encryptedValue == 0 && _checksum == 0;

        private double GetDecrypted()
        {
            if (IsDefault()) return 0d;

            long bits = _encryptedValue ^ _key;

            if ((bits ^ ChecksumSalt) != _checksum || (_fakeValue ^ _decoyKey) != bits)
            {
                OnCheatingDetected?.Invoke();
            }

            long newKey = ObscuredRandom.NextLong();
            _encryptedValue = bits ^ newKey;
            _key = newKey;

            return BitConverter.Int64BitsToDouble(bits);
        }

        private void SetEncrypted(double value)
        {
            long bits = BitConverter.DoubleToInt64Bits(value);
            _key = ObscuredRandom.NextLong();
            _encryptedValue = bits ^ _key;
            _checksum = bits ^ ChecksumSalt;
            _decoyKey = ObscuredRandom.NextLong();
            _fakeValue = bits ^ _decoyKey;
        }

        // Implicit conversions
        public static implicit operator ObscuredDouble(double value) => new ObscuredDouble(value);
        public static implicit operator double(ObscuredDouble value) => value.GetDecrypted();

        // Arithmetic operators
        public static ObscuredDouble operator +(ObscuredDouble a, ObscuredDouble b) => new ObscuredDouble(a.GetDecrypted() + b.GetDecrypted());
        public static ObscuredDouble operator -(ObscuredDouble a, ObscuredDouble b) => new ObscuredDouble(a.GetDecrypted() - b.GetDecrypted());
        public static ObscuredDouble operator *(ObscuredDouble a, ObscuredDouble b) => new ObscuredDouble(a.GetDecrypted() * b.GetDecrypted());
        public static ObscuredDouble operator /(ObscuredDouble a, ObscuredDouble b) => new ObscuredDouble(a.GetDecrypted() / b.GetDecrypted());

        // Comparison operators
        public static bool operator ==(ObscuredDouble a, ObscuredDouble b) => a.GetDecrypted() == b.GetDecrypted();
        public static bool operator !=(ObscuredDouble a, ObscuredDouble b) => a.GetDecrypted() != b.GetDecrypted();
        public static bool operator <(ObscuredDouble a, ObscuredDouble b) => a.GetDecrypted() < b.GetDecrypted();
        public static bool operator >(ObscuredDouble a, ObscuredDouble b) => a.GetDecrypted() > b.GetDecrypted();
        public static bool operator <=(ObscuredDouble a, ObscuredDouble b) => a.GetDecrypted() <= b.GetDecrypted();
        public static bool operator >=(ObscuredDouble a, ObscuredDouble b) => a.GetDecrypted() >= b.GetDecrypted();

        // IEquatable<ObscuredDouble>
        public bool Equals(ObscuredDouble other) => GetDecrypted() == other.GetDecrypted();

        // IComparable<ObscuredDouble>
        public int CompareTo(ObscuredDouble other) => GetDecrypted().CompareTo(other.GetDecrypted());

        // IFormattable
        public string ToString(string format, IFormatProvider formatProvider) => GetDecrypted().ToString(format, formatProvider);

        public override string ToString() => GetDecrypted().ToString();
        public override int GetHashCode() => GetDecrypted().GetHashCode();
        public override bool Equals(object obj) => obj is ObscuredDouble other && Equals(other);
    }
}
