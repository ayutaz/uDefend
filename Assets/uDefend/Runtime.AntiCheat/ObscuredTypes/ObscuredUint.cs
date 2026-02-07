using System;
using UnityEngine;

namespace uDefend.AntiCheat
{
    [Serializable]
    public struct ObscuredUint : IEquatable<ObscuredUint>, IComparable<ObscuredUint>, IFormattable
    {
        private const int ChecksumSalt = 0x6F1D_C5B8;

        public static event Action OnCheatingDetected;

        [SerializeField] private int _encryptedValue;
        [SerializeField] private int _key;
        [SerializeField] private int _checksum;
        [SerializeField] private int _fakeValue;

        private ObscuredUint(uint value)
        {
            _key = ObscuredRandom.Next();
            _encryptedValue = (int)(value ^ (uint)_key);
            _checksum = (int)(value ^ (uint)ChecksumSalt);
            _fakeValue = (int)value;
        }

        private bool IsDefault() => _key == 0 && _encryptedValue == 0 && _checksum == 0;

        private uint GetDecrypted()
        {
            if (IsDefault()) return 0;

            uint decrypted = (uint)_encryptedValue ^ (uint)_key;

            if ((decrypted ^ (uint)ChecksumSalt) != (uint)_checksum || (uint)_fakeValue != decrypted)
            {
                OnCheatingDetected?.Invoke();
            }

            int newKey = ObscuredRandom.Next();
            _encryptedValue = (int)(decrypted ^ (uint)newKey);
            _key = newKey;

            return decrypted;
        }

        private void SetEncrypted(uint value)
        {
            _key = ObscuredRandom.Next();
            _encryptedValue = (int)(value ^ (uint)_key);
            _checksum = (int)(value ^ (uint)ChecksumSalt);
            _fakeValue = (int)value;
        }

        // Implicit conversions
        public static implicit operator ObscuredUint(uint value) => new ObscuredUint(value);
        public static implicit operator uint(ObscuredUint value) => value.GetDecrypted();

        // Arithmetic operators
        public static ObscuredUint operator +(ObscuredUint a, ObscuredUint b) => new ObscuredUint(a.GetDecrypted() + b.GetDecrypted());
        public static ObscuredUint operator -(ObscuredUint a, ObscuredUint b) => new ObscuredUint(a.GetDecrypted() - b.GetDecrypted());
        public static ObscuredUint operator *(ObscuredUint a, ObscuredUint b) => new ObscuredUint(a.GetDecrypted() * b.GetDecrypted());
        public static ObscuredUint operator /(ObscuredUint a, ObscuredUint b) => new ObscuredUint(a.GetDecrypted() / b.GetDecrypted());
        public static ObscuredUint operator %(ObscuredUint a, ObscuredUint b) => new ObscuredUint(a.GetDecrypted() % b.GetDecrypted());

        public static ObscuredUint operator ++(ObscuredUint a)
        {
            uint val = a.GetDecrypted() + 1;
            a.SetEncrypted(val);
            return a;
        }

        public static ObscuredUint operator --(ObscuredUint a)
        {
            uint val = a.GetDecrypted() - 1;
            a.SetEncrypted(val);
            return a;
        }

        // Comparison operators
        public static bool operator ==(ObscuredUint a, ObscuredUint b) => a.GetDecrypted() == b.GetDecrypted();
        public static bool operator !=(ObscuredUint a, ObscuredUint b) => a.GetDecrypted() != b.GetDecrypted();
        public static bool operator <(ObscuredUint a, ObscuredUint b) => a.GetDecrypted() < b.GetDecrypted();
        public static bool operator >(ObscuredUint a, ObscuredUint b) => a.GetDecrypted() > b.GetDecrypted();
        public static bool operator <=(ObscuredUint a, ObscuredUint b) => a.GetDecrypted() <= b.GetDecrypted();
        public static bool operator >=(ObscuredUint a, ObscuredUint b) => a.GetDecrypted() >= b.GetDecrypted();

        // IEquatable<ObscuredUint>
        public bool Equals(ObscuredUint other) => GetDecrypted() == other.GetDecrypted();

        // IComparable<ObscuredUint>
        public int CompareTo(ObscuredUint other) => GetDecrypted().CompareTo(other.GetDecrypted());

        // IFormattable
        public string ToString(string format, IFormatProvider formatProvider) => GetDecrypted().ToString(format, formatProvider);

        public override string ToString() => GetDecrypted().ToString();
        public override int GetHashCode() => GetDecrypted().GetHashCode();
        public override bool Equals(object obj) => obj is ObscuredUint other && Equals(other);
    }
}
