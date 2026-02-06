using System;
using System.Security.Cryptography;

namespace uDefend.Encryption
{
    public static class CryptoUtility
    {
        public static byte[] GenerateRandomBytes(int length)
        {
            if (length <= 0)
                throw new ArgumentOutOfRangeException(nameof(length), "Length must be positive.");

            var bytes = new byte[length];
            RandomNumberGenerator.Fill(bytes);
            return bytes;
        }

        public static bool ConstantTimeEquals(byte[] a, byte[] b)
        {
            if (a == null || b == null)
                return false;

            return CryptographicOperations.FixedTimeEquals(a, b);
        }

        public static void SecureClear(byte[] buffer)
        {
            if (buffer != null)
                Array.Clear(buffer, 0, buffer.Length);
        }
    }
}
