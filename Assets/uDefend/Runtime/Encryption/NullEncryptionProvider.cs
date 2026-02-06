using System;

namespace uDefend.Encryption
{
    /// <summary>
    /// WARNING: This provider performs NO encryption. For development and debugging only.
    /// Do NOT use in production builds.
    /// </summary>
    [Obsolete("NullEncryptionProvider performs no encryption. Use AesCbcHmacProvider for production.")]
    public sealed class NullEncryptionProvider : IEncryptionProvider
    {
        public byte[] Encrypt(byte[] plaintext, byte[] encryptionKey, byte[] hmacKey)
        {
            if (plaintext == null) throw new ArgumentNullException(nameof(plaintext));

            var copy = new byte[plaintext.Length];
            Buffer.BlockCopy(plaintext, 0, copy, 0, plaintext.Length);
            return copy;
        }

        public byte[] Decrypt(byte[] ciphertext, byte[] encryptionKey, byte[] hmacKey)
        {
            if (ciphertext == null) throw new ArgumentNullException(nameof(ciphertext));

            var copy = new byte[ciphertext.Length];
            Buffer.BlockCopy(ciphertext, 0, copy, 0, ciphertext.Length);
            return copy;
        }
    }
}
