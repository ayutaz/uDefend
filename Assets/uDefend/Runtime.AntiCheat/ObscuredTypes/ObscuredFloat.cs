using System;
using UnityEngine;

namespace uDefend.AntiCheat
{
    [Serializable]
    public struct ObscuredFloat : IEquatable<ObscuredFloat>, IComparable<ObscuredFloat>, IFormattable
    {
        private const int ChecksumSalt = 0x7B2D_A4E9;

        public static event Action OnCheatingDetected;

        [SerializeField] private int _encryptedValue;
        [SerializeField] private int _key;
        [SerializeField] private int _checksum;
        [SerializeField] private float _fakeValue;

        private ObscuredFloat(float value)
        {
            int bits = BitConverter.SingleToInt32Bits(value);
            _key = ObscuredRandom.Next();
            _encryptedValue = bits ^ _key;
            _checksum = bits ^ ChecksumSalt;
            _fakeValue = value;
        }

        private bool IsDefault() => _key == 0 && _encryptedValue == 0 && _checksum == 0;

        private float GetDecrypted()
        {
            if (IsDefault()) return 0f;

            int bits = _encryptedValue ^ _key;

            if ((bits ^ ChecksumSalt) != _checksum || BitConverter.SingleToInt32Bits(_fakeValue) != bits)
            {
                OnCheatingDetected?.Invoke();
            }

            int newKey = ObscuredRandom.Next();
            _encryptedValue = bits ^ newKey;
            _key = newKey;

            return BitConverter.Int32BitsToSingle(bits);
        }

        private void SetEncrypted(float value)
        {
            int bits = BitConverter.SingleToInt32Bits(value);
            _key = ObscuredRandom.Next();
            _encryptedValue = bits ^ _key;
            _checksum = bits ^ ChecksumSalt;
            _fakeValue = value;
        }

        // Implicit conversions
        public static implicit operator ObscuredFloat(float value) => new ObscuredFloat(value);
        public static implicit operator float(ObscuredFloat value) => value.GetDecrypted();

        // Arithmetic operators
        public static ObscuredFloat operator +(ObscuredFloat a, ObscuredFloat b) => new ObscuredFloat(a.GetDecrypted() + b.GetDecrypted());
        public static ObscuredFloat operator -(ObscuredFloat a, ObscuredFloat b) => new ObscuredFloat(a.GetDecrypted() - b.GetDecrypted());
        public static ObscuredFloat operator *(ObscuredFloat a, ObscuredFloat b) => new ObscuredFloat(a.GetDecrypted() * b.GetDecrypted());
        public static ObscuredFloat operator /(ObscuredFloat a, ObscuredFloat b) => new ObscuredFloat(a.GetDecrypted() / b.GetDecrypted());

        // Comparison operators
        public static bool operator ==(ObscuredFloat a, ObscuredFloat b) => a.GetDecrypted() == b.GetDecrypted();
        public static bool operator !=(ObscuredFloat a, ObscuredFloat b) => a.GetDecrypted() != b.GetDecrypted();
        public static bool operator <(ObscuredFloat a, ObscuredFloat b) => a.GetDecrypted() < b.GetDecrypted();
        public static bool operator >(ObscuredFloat a, ObscuredFloat b) => a.GetDecrypted() > b.GetDecrypted();
        public static bool operator <=(ObscuredFloat a, ObscuredFloat b) => a.GetDecrypted() <= b.GetDecrypted();
        public static bool operator >=(ObscuredFloat a, ObscuredFloat b) => a.GetDecrypted() >= b.GetDecrypted();

        // IEquatable<ObscuredFloat>
        public bool Equals(ObscuredFloat other) => GetDecrypted() == other.GetDecrypted();

        // IComparable<ObscuredFloat>
        public int CompareTo(ObscuredFloat other) => GetDecrypted().CompareTo(other.GetDecrypted());

        // IFormattable
        public string ToString(string format, IFormatProvider formatProvider) => GetDecrypted().ToString(format, formatProvider);

        public override string ToString() => GetDecrypted().ToString();
        public override int GetHashCode() => GetDecrypted().GetHashCode();
        public override bool Equals(object obj) => obj is ObscuredFloat other && Equals(other);
    }
}
