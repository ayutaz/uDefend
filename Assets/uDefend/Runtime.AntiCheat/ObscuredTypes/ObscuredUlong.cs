using System;
using UnityEngine;

namespace uDefend.AntiCheat
{
    [Serializable]
    public struct ObscuredUlong : IEquatable<ObscuredUlong>, IComparable<ObscuredUlong>, IFormattable
    {
        private const long ChecksumSalt = 0x7A2F_D8B1_3E6C_94A5L;

        public static event Action OnCheatingDetected;

        [SerializeField] private long _encryptedValue;
        [SerializeField] private long _key;
        [SerializeField] private long _checksum;
        [SerializeField] private long _fakeValue;
        [SerializeField] private long _decoyKey;

        private ObscuredUlong(ulong value)
        {
            _key = ObscuredRandom.NextLong();
            _encryptedValue = (long)(value ^ (ulong)_key);
            _checksum = (long)(value ^ (ulong)ChecksumSalt);
            _decoyKey = ObscuredRandom.NextLong();
            _fakeValue = (long)value ^ _decoyKey;
        }

        private bool IsDefault() => _key == 0 && _encryptedValue == 0 && _checksum == 0;

        private ulong GetDecrypted()
        {
            if (IsDefault()) return 0;

            ulong decrypted = (ulong)_encryptedValue ^ (ulong)_key;

            if ((decrypted ^ (ulong)ChecksumSalt) != (ulong)_checksum || (ulong)(_fakeValue ^ _decoyKey) != decrypted)
            {
                OnCheatingDetected?.Invoke();
            }

            long newKey = ObscuredRandom.NextLong();
            _encryptedValue = (long)(decrypted ^ (ulong)newKey);
            _key = newKey;

            return decrypted;
        }

        private void SetEncrypted(ulong value)
        {
            _key = ObscuredRandom.NextLong();
            _encryptedValue = (long)(value ^ (ulong)_key);
            _checksum = (long)(value ^ (ulong)ChecksumSalt);
            _decoyKey = ObscuredRandom.NextLong();
            _fakeValue = (long)value ^ _decoyKey;
        }

        // Implicit conversions
        public static implicit operator ObscuredUlong(ulong value) => new ObscuredUlong(value);
        public static implicit operator ulong(ObscuredUlong value) => value.GetDecrypted();

        // Arithmetic operators
        public static ObscuredUlong operator +(ObscuredUlong a, ObscuredUlong b) => new ObscuredUlong(a.GetDecrypted() + b.GetDecrypted());
        public static ObscuredUlong operator -(ObscuredUlong a, ObscuredUlong b) => new ObscuredUlong(a.GetDecrypted() - b.GetDecrypted());
        public static ObscuredUlong operator *(ObscuredUlong a, ObscuredUlong b) => new ObscuredUlong(a.GetDecrypted() * b.GetDecrypted());
        public static ObscuredUlong operator /(ObscuredUlong a, ObscuredUlong b) => new ObscuredUlong(a.GetDecrypted() / b.GetDecrypted());
        public static ObscuredUlong operator %(ObscuredUlong a, ObscuredUlong b) => new ObscuredUlong(a.GetDecrypted() % b.GetDecrypted());

        public static ObscuredUlong operator ++(ObscuredUlong a)
        {
            ulong val = a.GetDecrypted() + 1;
            a.SetEncrypted(val);
            return a;
        }

        public static ObscuredUlong operator --(ObscuredUlong a)
        {
            ulong val = a.GetDecrypted() - 1;
            a.SetEncrypted(val);
            return a;
        }

        // Comparison operators
        public static bool operator ==(ObscuredUlong a, ObscuredUlong b) => a.GetDecrypted() == b.GetDecrypted();
        public static bool operator !=(ObscuredUlong a, ObscuredUlong b) => a.GetDecrypted() != b.GetDecrypted();
        public static bool operator <(ObscuredUlong a, ObscuredUlong b) => a.GetDecrypted() < b.GetDecrypted();
        public static bool operator >(ObscuredUlong a, ObscuredUlong b) => a.GetDecrypted() > b.GetDecrypted();
        public static bool operator <=(ObscuredUlong a, ObscuredUlong b) => a.GetDecrypted() <= b.GetDecrypted();
        public static bool operator >=(ObscuredUlong a, ObscuredUlong b) => a.GetDecrypted() >= b.GetDecrypted();

        // IEquatable<ObscuredUlong>
        public bool Equals(ObscuredUlong other) => GetDecrypted() == other.GetDecrypted();

        // IComparable<ObscuredUlong>
        public int CompareTo(ObscuredUlong other) => GetDecrypted().CompareTo(other.GetDecrypted());

        // IFormattable
        public string ToString(string format, IFormatProvider formatProvider) => GetDecrypted().ToString(format, formatProvider);

        public override string ToString() => GetDecrypted().ToString();
        public override int GetHashCode() => GetDecrypted().GetHashCode();
        public override bool Equals(object obj) => obj is ObscuredUlong other && Equals(other);
    }
}
