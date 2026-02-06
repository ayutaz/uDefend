using System;
using NUnit.Framework;
using uDefend.Encryption;
using uDefend.KeyManagement;

namespace uDefend.Tests.KeyManagement
{
    [TestFixture]
    public class KeyDerivationTests
    {
        [Test]
        public void DeriveKeys_ReturnsTwoDistinctKeys()
        {
            byte[] masterKey = CryptoUtility.GenerateRandomBytes(32);

            KeyDerivation.DeriveKeys(masterKey, out byte[] encryptionKey, out byte[] hmacKey);

            Assert.AreNotEqual(encryptionKey, hmacKey,
                "Encryption key and HMAC key must be different.");
        }

        [Test]
        public void DeriveKeys_Returns32ByteKeys()
        {
            byte[] masterKey = CryptoUtility.GenerateRandomBytes(32);

            KeyDerivation.DeriveKeys(masterKey, out byte[] encryptionKey, out byte[] hmacKey);

            Assert.AreEqual(32, encryptionKey.Length);
            Assert.AreEqual(32, hmacKey.Length);
        }

        [Test]
        public void DeriveKeys_SameMasterKey_ReturnsDeterministicKeys()
        {
            byte[] masterKey = CryptoUtility.GenerateRandomBytes(32);

            KeyDerivation.DeriveKeys(masterKey, out byte[] encKey1, out byte[] hmacKey1);
            KeyDerivation.DeriveKeys(masterKey, out byte[] encKey2, out byte[] hmacKey2);

            Assert.AreEqual(encKey1, encKey2);
            Assert.AreEqual(hmacKey1, hmacKey2);
        }

        [Test]
        public void DeriveKeys_DifferentMasterKeys_ReturnsDifferentKeys()
        {
            byte[] masterKey1 = CryptoUtility.GenerateRandomBytes(32);
            byte[] masterKey2 = CryptoUtility.GenerateRandomBytes(32);

            KeyDerivation.DeriveKeys(masterKey1, out byte[] encKey1, out byte[] hmacKey1);
            KeyDerivation.DeriveKeys(masterKey2, out byte[] encKey2, out byte[] hmacKey2);

            Assert.AreNotEqual(encKey1, encKey2);
            Assert.AreNotEqual(hmacKey1, hmacKey2);
        }

        [Test]
        public void DeriveKeys_NullMasterKey_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                KeyDerivation.DeriveKeys(null, out _, out _));
        }

        [Test]
        public void DeriveKeys_EmptyMasterKey_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                KeyDerivation.DeriveKeys(Array.Empty<byte>(), out _, out _));
        }
    }
}
