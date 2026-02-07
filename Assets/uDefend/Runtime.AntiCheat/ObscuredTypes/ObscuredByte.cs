using System;
using UnityEngine;

namespace uDefend.AntiCheat
{
    [Serializable]
    public struct ObscuredByte : IEquatable<ObscuredByte>, IComparable<ObscuredByte>, IFormattable
    {
        private const int ChecksumSalt = 0x2F7C_A3E1;

        public static event Action OnCheatingDetected;

        [SerializeField] private int _encryptedValue;
        [SerializeField] private int _key;
        [SerializeField] private int _checksum;
        [SerializeField] private int _fakeValue;

        private ObscuredByte(byte value)
        {
            _key = ObscuredRandom.Next();
            _encryptedValue = value ^ (byte)_key;
            _checksum = value ^ ChecksumSalt;
            _fakeValue = value;
        }

        private bool IsDefault() => _key == 0 && _encryptedValue == 0 && _checksum == 0;

        private byte GetDecrypted()
        {
            if (IsDefault()) return 0;

            byte decrypted = (byte)(_encryptedValue ^ (byte)_key);

            if ((decrypted ^ ChecksumSalt) != _checksum || _fakeValue != decrypted)
            {
                OnCheatingDetected?.Invoke();
            }

            int newKey = ObscuredRandom.Next();
            _encryptedValue = decrypted ^ (byte)newKey;
            _key = newKey;

            return decrypted;
        }

        private void SetEncrypted(byte value)
        {
            _key = ObscuredRandom.Next();
            _encryptedValue = value ^ (byte)_key;
            _checksum = value ^ ChecksumSalt;
            _fakeValue = value;
        }

        // Implicit conversions
        public static implicit operator ObscuredByte(byte value) => new ObscuredByte(value);
        public static implicit operator byte(ObscuredByte value) => value.GetDecrypted();

        // Arithmetic operators
        public static ObscuredByte operator +(ObscuredByte a, ObscuredByte b) => new ObscuredByte((byte)(a.GetDecrypted() + b.GetDecrypted()));
        public static ObscuredByte operator -(ObscuredByte a, ObscuredByte b) => new ObscuredByte((byte)(a.GetDecrypted() - b.GetDecrypted()));
        public static ObscuredByte operator *(ObscuredByte a, ObscuredByte b) => new ObscuredByte((byte)(a.GetDecrypted() * b.GetDecrypted()));
        public static ObscuredByte operator /(ObscuredByte a, ObscuredByte b) => new ObscuredByte((byte)(a.GetDecrypted() / b.GetDecrypted()));
        public static ObscuredByte operator %(ObscuredByte a, ObscuredByte b) => new ObscuredByte((byte)(a.GetDecrypted() % b.GetDecrypted()));

        public static ObscuredByte operator ++(ObscuredByte a)
        {
            byte val = (byte)(a.GetDecrypted() + 1);
            a.SetEncrypted(val);
            return a;
        }

        public static ObscuredByte operator --(ObscuredByte a)
        {
            byte val = (byte)(a.GetDecrypted() - 1);
            a.SetEncrypted(val);
            return a;
        }

        // Comparison operators
        public static bool operator ==(ObscuredByte a, ObscuredByte b) => a.GetDecrypted() == b.GetDecrypted();
        public static bool operator !=(ObscuredByte a, ObscuredByte b) => a.GetDecrypted() != b.GetDecrypted();
        public static bool operator <(ObscuredByte a, ObscuredByte b) => a.GetDecrypted() < b.GetDecrypted();
        public static bool operator >(ObscuredByte a, ObscuredByte b) => a.GetDecrypted() > b.GetDecrypted();
        public static bool operator <=(ObscuredByte a, ObscuredByte b) => a.GetDecrypted() <= b.GetDecrypted();
        public static bool operator >=(ObscuredByte a, ObscuredByte b) => a.GetDecrypted() >= b.GetDecrypted();

        // IEquatable<ObscuredByte>
        public bool Equals(ObscuredByte other) => GetDecrypted() == other.GetDecrypted();

        // IComparable<ObscuredByte>
        public int CompareTo(ObscuredByte other) => GetDecrypted().CompareTo(other.GetDecrypted());

        // IFormattable
        public string ToString(string format, IFormatProvider formatProvider) => GetDecrypted().ToString(format, formatProvider);

        public override string ToString() => GetDecrypted().ToString();
        public override int GetHashCode() => GetDecrypted().GetHashCode();
        public override bool Equals(object obj) => obj is ObscuredByte other && Equals(other);
    }
}
