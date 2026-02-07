using System;
using UnityEngine;

namespace uDefend.AntiCheat
{
    [Serializable]
    public struct ObscuredLong : IEquatable<ObscuredLong>, IComparable<ObscuredLong>, IFormattable
    {
        private const long ChecksumSalt = 0x5A3E_F1C7_4B8D_2A6FL;

        public static event Action OnCheatingDetected;

        [SerializeField] private long _encryptedValue;
        [SerializeField] private long _key;
        [SerializeField] private long _checksum;
        [SerializeField] private long _fakeValue;
        [SerializeField] private long _decoyKey;

        private ObscuredLong(long value)
        {
            _key = ObscuredRandom.NextLong();
            _encryptedValue = value ^ _key;
            _checksum = value ^ ChecksumSalt;
            _decoyKey = ObscuredRandom.NextLong();
            _fakeValue = value ^ _decoyKey;
        }

        private bool IsDefault() => _key == 0 && _encryptedValue == 0 && _checksum == 0;

        private long GetDecrypted()
        {
            if (IsDefault()) return 0;

            long decrypted = _encryptedValue ^ _key;

            if ((decrypted ^ ChecksumSalt) != _checksum || (_fakeValue ^ _decoyKey) != decrypted)
            {
                OnCheatingDetected?.Invoke();
            }

            long newKey = ObscuredRandom.NextLong();
            _encryptedValue = decrypted ^ newKey;
            _key = newKey;

            return decrypted;
        }

        private void SetEncrypted(long value)
        {
            _key = ObscuredRandom.NextLong();
            _encryptedValue = value ^ _key;
            _checksum = value ^ ChecksumSalt;
            _decoyKey = ObscuredRandom.NextLong();
            _fakeValue = value ^ _decoyKey;
        }

        // Implicit conversions
        public static implicit operator ObscuredLong(long value) => new ObscuredLong(value);
        public static implicit operator long(ObscuredLong value) => value.GetDecrypted();

        // Arithmetic operators
        public static ObscuredLong operator +(ObscuredLong a, ObscuredLong b) => new ObscuredLong(a.GetDecrypted() + b.GetDecrypted());
        public static ObscuredLong operator -(ObscuredLong a, ObscuredLong b) => new ObscuredLong(a.GetDecrypted() - b.GetDecrypted());
        public static ObscuredLong operator *(ObscuredLong a, ObscuredLong b) => new ObscuredLong(a.GetDecrypted() * b.GetDecrypted());
        public static ObscuredLong operator /(ObscuredLong a, ObscuredLong b) => new ObscuredLong(a.GetDecrypted() / b.GetDecrypted());
        public static ObscuredLong operator %(ObscuredLong a, ObscuredLong b) => new ObscuredLong(a.GetDecrypted() % b.GetDecrypted());

        public static ObscuredLong operator ++(ObscuredLong a)
        {
            long val = a.GetDecrypted() + 1;
            a.SetEncrypted(val);
            return a;
        }

        public static ObscuredLong operator --(ObscuredLong a)
        {
            long val = a.GetDecrypted() - 1;
            a.SetEncrypted(val);
            return a;
        }

        // Comparison operators
        public static bool operator ==(ObscuredLong a, ObscuredLong b) => a.GetDecrypted() == b.GetDecrypted();
        public static bool operator !=(ObscuredLong a, ObscuredLong b) => a.GetDecrypted() != b.GetDecrypted();
        public static bool operator <(ObscuredLong a, ObscuredLong b) => a.GetDecrypted() < b.GetDecrypted();
        public static bool operator >(ObscuredLong a, ObscuredLong b) => a.GetDecrypted() > b.GetDecrypted();
        public static bool operator <=(ObscuredLong a, ObscuredLong b) => a.GetDecrypted() <= b.GetDecrypted();
        public static bool operator >=(ObscuredLong a, ObscuredLong b) => a.GetDecrypted() >= b.GetDecrypted();

        // IEquatable<ObscuredLong>
        public bool Equals(ObscuredLong other) => GetDecrypted() == other.GetDecrypted();

        // IComparable<ObscuredLong>
        public int CompareTo(ObscuredLong other) => GetDecrypted().CompareTo(other.GetDecrypted());

        // IFormattable
        public string ToString(string format, IFormatProvider formatProvider) => GetDecrypted().ToString(format, formatProvider);

        public override string ToString() => GetDecrypted().ToString();
        public override int GetHashCode() => GetDecrypted().GetHashCode();
        public override bool Equals(object obj) => obj is ObscuredLong other && Equals(other);
    }
}
