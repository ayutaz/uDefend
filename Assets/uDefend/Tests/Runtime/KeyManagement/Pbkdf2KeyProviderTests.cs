using System;
using NUnit.Framework;
using uDefend.Encryption;
using uDefend.KeyManagement;

namespace uDefend.Tests.KeyManagement
{
    [TestFixture]
    public class Pbkdf2KeyProviderTests
    {
        [Test]
        public void GetMasterKey_SamePassphraseAndSalt_ReturnsDeterministicKey()
        {
            var passphrase = System.Text.Encoding.UTF8.GetBytes("test-passphrase");
            var salt = CryptoUtility.GenerateRandomBytes(16);

            var provider1 = new Pbkdf2KeyProvider(passphrase, salt);
            var provider2 = new Pbkdf2KeyProvider(passphrase, salt);

            byte[] key1 = provider1.GetMasterKey();
            byte[] key2 = provider2.GetMasterKey();

            Assert.AreEqual(key1, key2);
        }

        [Test]
        public void GetMasterKey_DifferentSalt_ReturnsDifferentKeys()
        {
            var passphrase = System.Text.Encoding.UTF8.GetBytes("test-passphrase");
            var salt1 = CryptoUtility.GenerateRandomBytes(16);
            var salt2 = CryptoUtility.GenerateRandomBytes(16);

            var provider1 = new Pbkdf2KeyProvider(passphrase, salt1);
            var provider2 = new Pbkdf2KeyProvider(passphrase, salt2);

            byte[] key1 = provider1.GetMasterKey();
            byte[] key2 = provider2.GetMasterKey();

            Assert.AreNotEqual(key1, key2);
        }

        [Test]
        public void GetMasterKey_Returns32ByteKey()
        {
            var passphrase = System.Text.Encoding.UTF8.GetBytes("test-passphrase");
            var salt = CryptoUtility.GenerateRandomBytes(16);
            var provider = new Pbkdf2KeyProvider(passphrase, salt);

            byte[] key = provider.GetMasterKey();

            Assert.AreEqual(32, key.Length);
        }

        [Test]
        public void GetMasterKey_CalledMultipleTimes_ReturnsSameKey()
        {
            var passphrase = System.Text.Encoding.UTF8.GetBytes("test-passphrase");
            var salt = CryptoUtility.GenerateRandomBytes(16);
            var provider = new Pbkdf2KeyProvider(passphrase, salt);

            byte[] key1 = provider.GetMasterKey();
            byte[] key2 = provider.GetMasterKey();

            Assert.AreSame(key1, key2, "Cached key should return the same instance.");
        }

        [Test]
        public void StoreMasterKey_ThrowsNotSupportedException()
        {
            var passphrase = System.Text.Encoding.UTF8.GetBytes("test-passphrase");
            var provider = new Pbkdf2KeyProvider(passphrase);

            Assert.Throws<NotSupportedException>(() =>
                provider.StoreMasterKey(new byte[32]));
        }

        [Test]
        public void HasMasterKey_BeforeGetMasterKey_WithoutSalt_ReturnsFalse()
        {
            var passphrase = System.Text.Encoding.UTF8.GetBytes("test-passphrase");
            var provider = new Pbkdf2KeyProvider(passphrase);

            Assert.IsFalse(provider.HasMasterKey());
        }

        [Test]
        public void HasMasterKey_AfterGetMasterKey_ReturnsTrue()
        {
            var passphrase = System.Text.Encoding.UTF8.GetBytes("test-passphrase");
            var provider = new Pbkdf2KeyProvider(passphrase);
            provider.GetMasterKey();

            Assert.IsTrue(provider.HasMasterKey());
        }

        [Test]
        public void Constructor_NullPassphrase_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new Pbkdf2KeyProvider(null));
        }

        [Test]
        public void Constructor_EmptyPassphrase_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new Pbkdf2KeyProvider(Array.Empty<byte>()));
        }

        [Test]
        public void GetSalt_ReturnsCopy()
        {
            var passphrase = System.Text.Encoding.UTF8.GetBytes("test-passphrase");
            var originalSalt = CryptoUtility.GenerateRandomBytes(16);
            var provider = new Pbkdf2KeyProvider(passphrase, originalSalt);

            byte[] salt1 = provider.GetSalt();
            byte[] salt2 = provider.GetSalt();

            Assert.AreEqual(salt1, salt2);
            Assert.AreNotSame(salt1, salt2, "GetSalt should return a copy, not the internal reference.");
        }
    }
}
