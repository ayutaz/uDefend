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

        private ObscuredFloat(float value)
        {
            int bits = BitConverter.SingleToInt32Bits(value);
            _key = GenerateKey();
            _encryptedValue = bits ^ _key;
            _checksum = bits ^ ChecksumSalt;
        }

        private float GetDecrypted()
        {
            int bits = _encryptedValue ^ _key;
            if ((bits ^ ChecksumSalt) != _checksum)
            {
                OnCheatingDetected?.Invoke();
            }
            return BitConverter.Int32BitsToSingle(bits);
        }

        private void SetEncrypted(float value)
        {
            int bits = BitConverter.SingleToInt32Bits(value);
            _key = GenerateKey();
            _encryptedValue = bits ^ _key;
            _checksum = bits ^ ChecksumSalt;
        }

        private static int GenerateKey()
        {
            int key = UnityEngine.Random.Range(1, int.MaxValue);
            return key;
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
