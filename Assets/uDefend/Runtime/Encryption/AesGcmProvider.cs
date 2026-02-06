using System;

namespace uDefend.Encryption
{
    /// <summary>
    /// AES-GCM provider stub. Will be implemented after Unity migrates to CoreCLR.
    /// </summary>
    public sealed class AesGcmProvider : IEncryptionProvider
    {
        public byte[] Encrypt(byte[] plaintext, byte[] encryptionKey, byte[] hmacKey)
        {
            throw new NotSupportedException(
                "AES-GCM is not supported in the current Unity runtime. It will be available after CoreCLR migration.");
        }

        public byte[] Decrypt(byte[] ciphertext, byte[] encryptionKey, byte[] hmacKey)
        {
            throw new NotSupportedException(
                "AES-GCM is not supported in the current Unity runtime. It will be available after CoreCLR migration.");
        }
    }
}
