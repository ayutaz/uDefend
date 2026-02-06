using System;
using System.Diagnostics;
using NUnit.Framework;
using uDefend.Encryption;

namespace uDefend.Tests.Encryption
{
    [TestFixture]
    public class AesCbcHmacProviderTests
    {
        private AesCbcHmacProvider _provider;
        private byte[] _encryptionKey;
        private byte[] _hmacKey;

        [SetUp]
        public void SetUp()
        {
            _provider = new AesCbcHmacProvider();
            _encryptionKey = CryptoUtility.GenerateRandomBytes(32);
            _hmacKey = CryptoUtility.GenerateRandomBytes(32);
        }

        [Test]
        public void Encrypt_Decrypt_RoundTrip_ReturnsOriginalData()
        {
            var plaintext = System.Text.Encoding.UTF8.GetBytes("Hello, uDefend!");

            byte[] encrypted = _provider.Encrypt(plaintext, _encryptionKey, _hmacKey);
            byte[] decrypted = _provider.Decrypt(encrypted, _encryptionKey, _hmacKey);

            Assert.AreEqual(plaintext, decrypted);
        }

        [Test]
        public void Encrypt_SameData_ProducesDifferentCiphertexts()
        {
            var plaintext = System.Text.Encoding.UTF8.GetBytes("Same data twice");

            byte[] encrypted1 = _provider.Encrypt(plaintext, _encryptionKey, _hmacKey);
            byte[] encrypted2 = _provider.Encrypt(plaintext, _encryptionKey, _hmacKey);

            Assert.AreNotEqual(encrypted1, encrypted2,
                "Encrypting the same data twice must produce different ciphertexts due to random IV.");
        }

        [Test]
        public void Decrypt_TamperedData_ThrowsTamperDetectedException()
        {
            var plaintext = System.Text.Encoding.UTF8.GetBytes("Tamper test data");
            byte[] encrypted = _provider.Encrypt(plaintext, _encryptionKey, _hmacKey);

            // Tamper with a byte in the ciphertext region (after IV, before HMAC)
            encrypted[20] ^= 0xFF;

            Assert.Throws<TamperDetectedException>(() =>
                _provider.Decrypt(encrypted, _encryptionKey, _hmacKey));
        }

        [Test]
        public void Decrypt_TruncatedData_ThrowsException()
        {
            var truncated = new byte[10];

            Assert.Throws<EncryptionException>(() =>
                _provider.Decrypt(truncated, _encryptionKey, _hmacKey));
        }

        [Test]
        public void Encrypt_EmptyData_Succeeds()
        {
            var plaintext = Array.Empty<byte>();

            byte[] encrypted = _provider.Encrypt(plaintext, _encryptionKey, _hmacKey);
            byte[] decrypted = _provider.Decrypt(encrypted, _encryptionKey, _hmacKey);

            Assert.AreEqual(plaintext, decrypted);
        }

        [Test]
        public void Encrypt_LargeData_1MB_CompletesWithinTimeout()
        {
            var plaintext = CryptoUtility.GenerateRandomBytes(1024 * 1024);

            var sw = Stopwatch.StartNew();
            byte[] encrypted = _provider.Encrypt(plaintext, _encryptionKey, _hmacKey);
            byte[] decrypted = _provider.Decrypt(encrypted, _encryptionKey, _hmacKey);
            sw.Stop();

            Assert.AreEqual(plaintext, decrypted);
            Assert.Less(sw.ElapsedMilliseconds, 5000,
                "1MB encrypt+decrypt should complete within 5 seconds in the Editor.");
        }
    }
}
