using System;
using System.Security.Cryptography;

namespace uDefend.Encryption
{
    public sealed class AesCbcHmacProvider : IEncryptionProvider
    {
        private const int IvSize = 16;
        private const int HmacSize = 32;
        private const int KeySize = 256;

        public byte[] Encrypt(byte[] plaintext, byte[] encryptionKey, byte[] hmacKey)
        {
            if (plaintext == null) throw new ArgumentNullException(nameof(plaintext));
            if (encryptionKey == null) throw new ArgumentNullException(nameof(encryptionKey));
            if (hmacKey == null) throw new ArgumentNullException(nameof(hmacKey));

            byte[] iv = CryptoUtility.GenerateRandomBytes(IvSize);
            byte[] encryptedData;

            using (var aes = Aes.Create())
            {
                aes.KeySize = KeySize;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = encryptionKey;
                aes.IV = iv;

                using (var encryptor = aes.CreateEncryptor())
                {
                    encryptedData = encryptor.TransformFinalBlock(plaintext, 0, plaintext.Length);
                }
            }

            // Result: IV (16B) + Ciphertext (variable) + HMAC (32B)
            var result = new byte[IvSize + encryptedData.Length + HmacSize];
            Buffer.BlockCopy(iv, 0, result, 0, IvSize);
            Buffer.BlockCopy(encryptedData, 0, result, IvSize, encryptedData.Length);

            // Compute HMAC over IV + ciphertext
            using (var hmac = new HMACSHA256(hmacKey))
            {
                byte[] mac = hmac.ComputeHash(result, 0, IvSize + encryptedData.Length);
                Buffer.BlockCopy(mac, 0, result, IvSize + encryptedData.Length, HmacSize);
            }

            return result;
        }

        public byte[] Decrypt(byte[] ciphertext, byte[] encryptionKey, byte[] hmacKey)
        {
            if (ciphertext == null) throw new ArgumentNullException(nameof(ciphertext));
            if (encryptionKey == null) throw new ArgumentNullException(nameof(encryptionKey));
            if (hmacKey == null) throw new ArgumentNullException(nameof(hmacKey));

            if (ciphertext.Length < IvSize + HmacSize + 1)
                throw new EncryptionException("Invalid data.");

            int encryptedDataLength = ciphertext.Length - IvSize - HmacSize;

            // Extract stored HMAC (last 32 bytes)
            var storedHmac = new byte[HmacSize];
            Buffer.BlockCopy(ciphertext, ciphertext.Length - HmacSize, storedHmac, 0, HmacSize);

            // Verify HMAC FIRST (before any decryption)
            byte[] computedHmac;
            using (var hmac = new HMACSHA256(hmacKey))
            {
                computedHmac = hmac.ComputeHash(ciphertext, 0, IvSize + encryptedDataLength);
            }

            if (!CryptoUtility.ConstantTimeEquals(storedHmac, computedHmac))
            {
                CryptoUtility.SecureClear(computedHmac);
                CryptoUtility.SecureClear(storedHmac);
                throw new TamperDetectedException();
            }

            CryptoUtility.SecureClear(computedHmac);
            CryptoUtility.SecureClear(storedHmac);

            // Extract IV
            var iv = new byte[IvSize];
            Buffer.BlockCopy(ciphertext, 0, iv, 0, IvSize);

            // Extract encrypted data
            var encryptedData = new byte[encryptedDataLength];
            Buffer.BlockCopy(ciphertext, IvSize, encryptedData, 0, encryptedDataLength);

            byte[] plaintext;
            try
            {
                using (var aes = Aes.Create())
                {
                    aes.KeySize = KeySize;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;
                    aes.Key = encryptionKey;
                    aes.IV = iv;

                    using (var decryptor = aes.CreateDecryptor())
                    {
                        plaintext = decryptor.TransformFinalBlock(encryptedData, 0, encryptedData.Length);
                    }
                }
            }
            catch (CryptographicException)
            {
                throw new EncryptionException("Decryption failed.");
            }
            finally
            {
                CryptoUtility.SecureClear(iv);
                CryptoUtility.SecureClear(encryptedData);
            }

            return plaintext;
        }
    }
}
