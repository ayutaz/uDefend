using System;
using UnityEngine;

namespace uDefend.AntiCheat
{
    [Serializable]
    public struct ObscuredShort : IEquatable<ObscuredShort>, IComparable<ObscuredShort>, IFormattable
    {
        private const int ChecksumSalt = 0x3E9A_D7B2;

        public static event Action OnCheatingDetected;

        [SerializeField] private int _encryptedValue;
        [SerializeField] private int _key;
        [SerializeField] private int _checksum;
        [SerializeField] private int _fakeValue;

        private ObscuredShort(short value)
        {
            _key = ObscuredRandom.Next();
            _encryptedValue = value ^ (short)_key;
            _checksum = value ^ ChecksumSalt;
            _fakeValue = value;
        }

        private bool IsDefault() => _key == 0 && _encryptedValue == 0 && _checksum == 0;

        private short GetDecrypted()
        {
            if (IsDefault()) return 0;

            short decrypted = (short)(_encryptedValue ^ (short)_key);

            if ((decrypted ^ ChecksumSalt) != _checksum || _fakeValue != decrypted)
            {
                OnCheatingDetected?.Invoke();
            }

            int newKey = ObscuredRandom.Next();
            _encryptedValue = decrypted ^ (short)newKey;
            _key = newKey;

            return decrypted;
        }

        private void SetEncrypted(short value)
        {
            _key = ObscuredRandom.Next();
            _encryptedValue = value ^ (short)_key;
            _checksum = value ^ ChecksumSalt;
            _fakeValue = value;
        }

        // Implicit conversions
        public static implicit operator ObscuredShort(short value) => new ObscuredShort(value);
        public static implicit operator short(ObscuredShort value) => value.GetDecrypted();

        // Arithmetic operators
        public static ObscuredShort operator +(ObscuredShort a, ObscuredShort b) => new ObscuredShort((short)(a.GetDecrypted() + b.GetDecrypted()));
        public static ObscuredShort operator -(ObscuredShort a, ObscuredShort b) => new ObscuredShort((short)(a.GetDecrypted() - b.GetDecrypted()));
        public static ObscuredShort operator *(ObscuredShort a, ObscuredShort b) => new ObscuredShort((short)(a.GetDecrypted() * b.GetDecrypted()));
        public static ObscuredShort operator /(ObscuredShort a, ObscuredShort b) => new ObscuredShort((short)(a.GetDecrypted() / b.GetDecrypted()));
        public static ObscuredShort operator %(ObscuredShort a, ObscuredShort b) => new ObscuredShort((short)(a.GetDecrypted() % b.GetDecrypted()));

        public static ObscuredShort operator ++(ObscuredShort a)
        {
            short val = (short)(a.GetDecrypted() + 1);
            a.SetEncrypted(val);
            return a;
        }

        public static ObscuredShort operator --(ObscuredShort a)
        {
            short val = (short)(a.GetDecrypted() - 1);
            a.SetEncrypted(val);
            return a;
        }

        // Comparison operators
        public static bool operator ==(ObscuredShort a, ObscuredShort b) => a.GetDecrypted() == b.GetDecrypted();
        public static bool operator !=(ObscuredShort a, ObscuredShort b) => a.GetDecrypted() != b.GetDecrypted();
        public static bool operator <(ObscuredShort a, ObscuredShort b) => a.GetDecrypted() < b.GetDecrypted();
        public static bool operator >(ObscuredShort a, ObscuredShort b) => a.GetDecrypted() > b.GetDecrypted();
        public static bool operator <=(ObscuredShort a, ObscuredShort b) => a.GetDecrypted() <= b.GetDecrypted();
        public static bool operator >=(ObscuredShort a, ObscuredShort b) => a.GetDecrypted() >= b.GetDecrypted();

        // IEquatable<ObscuredShort>
        public bool Equals(ObscuredShort other) => GetDecrypted() == other.GetDecrypted();

        // IComparable<ObscuredShort>
        public int CompareTo(ObscuredShort other) => GetDecrypted().CompareTo(other.GetDecrypted());

        // IFormattable
        public string ToString(string format, IFormatProvider formatProvider) => GetDecrypted().ToString(format, formatProvider);

        public override string ToString() => GetDecrypted().ToString();
        public override int GetHashCode() => GetDecrypted().GetHashCode();
        public override bool Equals(object obj) => obj is ObscuredShort other && Equals(other);
    }
}
