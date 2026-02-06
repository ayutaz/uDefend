using System;
using System.Text;
using NUnit.Framework;
using uDefend.Encryption;
using uDefend.KeyManagement;

namespace uDefend.Tests.KeyManagement
{
    [TestFixture]
    public class EnvelopeEncryptionTests
    {
        private Pbkdf2KeyProvider _keyProvider;
        private EnvelopeEncryption _envelope;

        [SetUp]
        public void SetUp()
        {
            var passphrase = Encoding.UTF8.GetBytes("test-master-passphrase");
            var salt = CryptoUtility.GenerateRandomBytes(16);
            _keyProvider = new Pbkdf2KeyProvider(passphrase, salt);
            _envelope = new EnvelopeEncryption(_keyProvider);
        }

        [Test]
        public void Encrypt_Decrypt_RoundTrip_ReturnsOriginalData()
        {
            var plaintext = Encoding.UTF8.GetBytes("Envelope encryption round-trip test");

            EncryptedEnvelope encrypted = _envelope.Encrypt(plaintext);
            byte[] decrypted = _envelope.Decrypt(encrypted);

            Assert.AreEqual(plaintext, decrypted);
        }

        [Test]
        public void Encrypt_SameData_ProducesDifferentEnvelopes()
        {
            var plaintext = Encoding.UTF8.GetBytes("Same data");

            EncryptedEnvelope envelope1 = _envelope.Encrypt(plaintext);
            EncryptedEnvelope envelope2 = _envelope.Encrypt(plaintext);

            Assert.AreNotEqual(envelope1.EncryptedDek, envelope2.EncryptedDek,
                "Each encryption should generate a new random DEK.");
            Assert.AreNotEqual(envelope1.EncryptedData, envelope2.EncryptedData,
                "Each encryption should use a different IV.");
        }

        [Test]
        public void Decrypt_TamperedData_ThrowsTamperDetectedException()
        {
            var plaintext = Encoding.UTF8.GetBytes("Tamper test");
            EncryptedEnvelope envelope = _envelope.Encrypt(plaintext);

            // Tamper with encrypted data
            byte[] tamperedData = new byte[envelope.EncryptedData.Length];
            Buffer.BlockCopy(envelope.EncryptedData, 0, tamperedData, 0, tamperedData.Length);
            tamperedData[20] ^= 0xFF;

            var tamperedEnvelope = new EncryptedEnvelope(envelope.EncryptedDek, tamperedData);

            Assert.Throws<TamperDetectedException>(() => _envelope.Decrypt(tamperedEnvelope));
        }

        [Test]
        public void Decrypt_TamperedDek_ThrowsTamperDetectedException()
        {
            var plaintext = Encoding.UTF8.GetBytes("Tamper DEK test");
            EncryptedEnvelope envelope = _envelope.Encrypt(plaintext);

            // Tamper with encrypted DEK
            byte[] tamperedDek = new byte[envelope.EncryptedDek.Length];
            Buffer.BlockCopy(envelope.EncryptedDek, 0, tamperedDek, 0, tamperedDek.Length);
            tamperedDek[20] ^= 0xFF;

            var tamperedEnvelope = new EncryptedEnvelope(tamperedDek, envelope.EncryptedData);

            Assert.Throws<TamperDetectedException>(() => _envelope.Decrypt(tamperedEnvelope));
        }

        [Test]
        public void RotateKey_DecryptsWithNewKey()
        {
            var plaintext = Encoding.UTF8.GetBytes("Key rotation test data");
            EncryptedEnvelope originalEnvelope = _envelope.Encrypt(plaintext);

            // Create a new key provider
            var newPassphrase = Encoding.UTF8.GetBytes("new-master-passphrase");
            var newSalt = CryptoUtility.GenerateRandomBytes(16);
            var newKeyProvider = new Pbkdf2KeyProvider(newPassphrase, newSalt);

            // Rotate the key
            EncryptedEnvelope rotatedEnvelope = _envelope.RotateKey(originalEnvelope, newKeyProvider);

            // Decrypt with new key provider
            var newEnvelope = new EnvelopeEncryption(newKeyProvider);
            byte[] decrypted = newEnvelope.Decrypt(rotatedEnvelope);

            Assert.AreEqual(plaintext, decrypted);
        }

        [Test]
        public void RotateKey_OldKeyCannotDecryptRotatedEnvelope()
        {
            var plaintext = Encoding.UTF8.GetBytes("Key rotation invalidation test");
            EncryptedEnvelope originalEnvelope = _envelope.Encrypt(plaintext);

            var newPassphrase = Encoding.UTF8.GetBytes("new-master-passphrase");
            var newSalt = CryptoUtility.GenerateRandomBytes(16);
            var newKeyProvider = new Pbkdf2KeyProvider(newPassphrase, newSalt);

            EncryptedEnvelope rotatedEnvelope = _envelope.RotateKey(originalEnvelope, newKeyProvider);

            // Old key should fail to decrypt the rotated envelope
            Assert.Throws<TamperDetectedException>(() => _envelope.Decrypt(rotatedEnvelope));
        }

        [Test]
        public void RotateKey_DataRemainsUnchanged()
        {
            var plaintext = Encoding.UTF8.GetBytes("Data should not change");
            EncryptedEnvelope originalEnvelope = _envelope.Encrypt(plaintext);

            var newPassphrase = Encoding.UTF8.GetBytes("new-master-passphrase");
            var newSalt = CryptoUtility.GenerateRandomBytes(16);
            var newKeyProvider = new Pbkdf2KeyProvider(newPassphrase, newSalt);

            EncryptedEnvelope rotatedEnvelope = _envelope.RotateKey(originalEnvelope, newKeyProvider);

            // Encrypted data should be identical (only DEK wrapper changes)
            Assert.AreEqual(originalEnvelope.EncryptedData, rotatedEnvelope.EncryptedData,
                "Key rotation should only re-encrypt the DEK, not the data.");
        }

        [Test]
        public void Encrypt_EmptyData_Succeeds()
        {
            var plaintext = Array.Empty<byte>();

            EncryptedEnvelope encrypted = _envelope.Encrypt(plaintext);
            byte[] decrypted = _envelope.Decrypt(encrypted);

            Assert.AreEqual(plaintext, decrypted);
        }
    }
}
