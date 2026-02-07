using System;
using UnityEngine;

namespace uDefend.AntiCheat
{
    [Serializable]
    public struct ObscuredDecimal : IEquatable<ObscuredDecimal>, IComparable<ObscuredDecimal>, IFormattable
    {
        private const int ChecksumSalt = 0x4D8E_B2C7;

        public static event Action OnCheatingDetected;

        [SerializeField] private int _encrypted0;
        [SerializeField] private int _encrypted1;
        [SerializeField] private int _encrypted2;
        [SerializeField] private int _encrypted3;
        [SerializeField] private int _key0;
        [SerializeField] private int _key1;
        [SerializeField] private int _key2;
        [SerializeField] private int _key3;
        [SerializeField] private int _checksum;

        private ObscuredDecimal(decimal value)
        {
            int[] bits = decimal.GetBits(value);
            _key0 = ObscuredRandom.Next();
            _key1 = ObscuredRandom.Next();
            _key2 = ObscuredRandom.Next();
            _key3 = ObscuredRandom.Next();
            _encrypted0 = bits[0] ^ _key0;
            _encrypted1 = bits[1] ^ _key1;
            _encrypted2 = bits[2] ^ _key2;
            _encrypted3 = bits[3] ^ _key3;
            _checksum = ComputeChecksum(bits);
        }

        private static int ComputeChecksum(int[] bits)
        {
            return bits[0] ^ bits[1] ^ bits[2] ^ bits[3] ^ ChecksumSalt;
        }

        private bool IsDefault() =>
            _key0 == 0 && _key1 == 0 && _key2 == 0 && _key3 == 0 &&
            _encrypted0 == 0 && _encrypted1 == 0 && _encrypted2 == 0 && _encrypted3 == 0 &&
            _checksum == 0;

        private decimal GetDecrypted()
        {
            if (IsDefault()) return 0m;

            int[] bits = new int[4];
            bits[0] = _encrypted0 ^ _key0;
            bits[1] = _encrypted1 ^ _key1;
            bits[2] = _encrypted2 ^ _key2;
            bits[3] = _encrypted3 ^ _key3;

            if (ComputeChecksum(bits) != _checksum)
            {
                OnCheatingDetected?.Invoke();
            }
            return new decimal(bits);
        }

        private void SetEncrypted(decimal value)
        {
            int[] bits = decimal.GetBits(value);
            _key0 = ObscuredRandom.Next();
            _key1 = ObscuredRandom.Next();
            _key2 = ObscuredRandom.Next();
            _key3 = ObscuredRandom.Next();
            _encrypted0 = bits[0] ^ _key0;
            _encrypted1 = bits[1] ^ _key1;
            _encrypted2 = bits[2] ^ _key2;
            _encrypted3 = bits[3] ^ _key3;
            _checksum = ComputeChecksum(bits);
        }

        // Implicit conversions
        public static implicit operator ObscuredDecimal(decimal value) => new ObscuredDecimal(value);
        public static implicit operator decimal(ObscuredDecimal value) => value.GetDecrypted();

        // Arithmetic operators
        public static ObscuredDecimal operator +(ObscuredDecimal a, ObscuredDecimal b) => new ObscuredDecimal(a.GetDecrypted() + b.GetDecrypted());
        public static ObscuredDecimal operator -(ObscuredDecimal a, ObscuredDecimal b) => new ObscuredDecimal(a.GetDecrypted() - b.GetDecrypted());
        public static ObscuredDecimal operator *(ObscuredDecimal a, ObscuredDecimal b) => new ObscuredDecimal(a.GetDecrypted() * b.GetDecrypted());
        public static ObscuredDecimal operator /(ObscuredDecimal a, ObscuredDecimal b) => new ObscuredDecimal(a.GetDecrypted() / b.GetDecrypted());
        public static ObscuredDecimal operator %(ObscuredDecimal a, ObscuredDecimal b) => new ObscuredDecimal(a.GetDecrypted() % b.GetDecrypted());

        public static ObscuredDecimal operator ++(ObscuredDecimal a)
        {
            decimal val = a.GetDecrypted() + 1;
            a.SetEncrypted(val);
            return a;
        }

        public static ObscuredDecimal operator --(ObscuredDecimal a)
        {
            decimal val = a.GetDecrypted() - 1;
            a.SetEncrypted(val);
            return a;
        }

        // Comparison operators
        public static bool operator ==(ObscuredDecimal a, ObscuredDecimal b) => a.GetDecrypted() == b.GetDecrypted();
        public static bool operator !=(ObscuredDecimal a, ObscuredDecimal b) => a.GetDecrypted() != b.GetDecrypted();
        public static bool operator <(ObscuredDecimal a, ObscuredDecimal b) => a.GetDecrypted() < b.GetDecrypted();
        public static bool operator >(ObscuredDecimal a, ObscuredDecimal b) => a.GetDecrypted() > b.GetDecrypted();
        public static bool operator <=(ObscuredDecimal a, ObscuredDecimal b) => a.GetDecrypted() <= b.GetDecrypted();
        public static bool operator >=(ObscuredDecimal a, ObscuredDecimal b) => a.GetDecrypted() >= b.GetDecrypted();

        // IEquatable<ObscuredDecimal>
        public bool Equals(ObscuredDecimal other) => GetDecrypted() == other.GetDecrypted();

        // IComparable<ObscuredDecimal>
        public int CompareTo(ObscuredDecimal other) => GetDecrypted().CompareTo(other.GetDecrypted());

        // IFormattable
        public string ToString(string format, IFormatProvider formatProvider) => GetDecrypted().ToString(format, formatProvider);

        public override string ToString() => GetDecrypted().ToString();
        public override int GetHashCode() => GetDecrypted().GetHashCode();
        public override bool Equals(object obj) => obj is ObscuredDecimal other && Equals(other);
    }
}
