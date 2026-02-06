using System;
using System.Security.Cryptography;

namespace uDefend.KeyManagement
{
    public static class KeyDerivation
    {
        private const int DerivedKeySize = 32;

        public static void DeriveKeys(byte[] masterKey, out byte[] encryptionKey, out byte[] hmacKey)
        {
            if (masterKey == null || masterKey.Length == 0)
                throw new ArgumentException("Master key must not be null or empty.", nameof(masterKey));

            // Derive encryption key: HMAC-SHA256(masterKey, "uDefend.Encryption")
            encryptionKey = DeriveSubKey(masterKey, "uDefend.Encryption");

            // Derive HMAC key: HMAC-SHA256(masterKey, "uDefend.HMAC")
            hmacKey = DeriveSubKey(masterKey, "uDefend.HMAC");
        }

        private static byte[] DeriveSubKey(byte[] masterKey, string context)
        {
            using (var hmac = new HMACSHA256(masterKey))
            {
                byte[] contextBytes = System.Text.Encoding.UTF8.GetBytes(context);
                byte[] derived = hmac.ComputeHash(contextBytes);

                // HMAC-SHA256 output is always 32 bytes, matching our DerivedKeySize
                return derived;
            }
        }
    }
}
