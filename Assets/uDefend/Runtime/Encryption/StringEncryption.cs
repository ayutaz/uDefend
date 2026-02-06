using System;
using System.Text;

namespace uDefend.Encryption
{
    public static class StringEncryption
    {
        private static readonly AesCbcHmacProvider DefaultProvider = new AesCbcHmacProvider();

        public static string EncryptString(string plaintext, byte[] encryptionKey, byte[] hmacKey,
            IEncryptionProvider provider = null)
        {
            if (plaintext == null) throw new ArgumentNullException(nameof(plaintext));
            if (encryptionKey == null) throw new ArgumentNullException(nameof(encryptionKey));
            if (hmacKey == null) throw new ArgumentNullException(nameof(hmacKey));

            var effectiveProvider = provider ?? DefaultProvider;
            byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
            byte[] encrypted = effectiveProvider.Encrypt(plaintextBytes, encryptionKey, hmacKey);
            CryptoUtility.SecureClear(plaintextBytes);
            return Convert.ToBase64String(encrypted);
        }

        public static string DecryptString(string base64Ciphertext, byte[] encryptionKey, byte[] hmacKey,
            IEncryptionProvider provider = null)
        {
            if (base64Ciphertext == null) throw new ArgumentNullException(nameof(base64Ciphertext));
            if (encryptionKey == null) throw new ArgumentNullException(nameof(encryptionKey));
            if (hmacKey == null) throw new ArgumentNullException(nameof(hmacKey));

            var effectiveProvider = provider ?? DefaultProvider;
            byte[] ciphertext = Convert.FromBase64String(base64Ciphertext);
            byte[] decrypted = effectiveProvider.Decrypt(ciphertext, encryptionKey, hmacKey);
            string result = Encoding.UTF8.GetString(decrypted);
            CryptoUtility.SecureClear(decrypted);
            return result;
        }
    }
}
