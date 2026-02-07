#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
using System;
using System.Reflection;
using NUnit.Framework;
using uDefend.KeyManagement;

namespace uDefend.Tests.KeyManagement
{
    [TestFixture]
    public class WindowsDpapiKeyProviderTests
    {
        private string _testKeyId;
        private WindowsDpapiKeyProvider _provider;

        [SetUp]
        public void SetUp()
        {
            _testKeyId = "test_" + Guid.NewGuid().ToString("N");
            _provider = new WindowsDpapiKeyProvider(_testKeyId);
        }

        [TearDown]
        public void TearDown()
        {
            try
            {
                _provider?.DeleteMasterKey();
            }
            catch
            {
                // Ignore errors during cleanup (e.g., already disposed)
            }

            _provider?.Dispose();
            _provider = null;
        }

        [Test]
        public void ComputeAppEntropy_Returns32Bytes()
        {
            // AppEntropy is a private static readonly field computed via SHA-256
            var entropyField = typeof(WindowsDpapiKeyProvider).GetField("AppEntropy", BindingFlags.NonPublic | BindingFlags.Static);
            var entropy = (byte[])entropyField.GetValue(null);

            Assert.AreEqual(32, entropy.Length, "SHA-256 should produce 32 bytes of entropy.");
        }

        [Test]
        public void ComputeAppEntropy_IsDeterministic()
        {
            // Call ComputeAppEntropy via reflection twice and compare
            var method = typeof(WindowsDpapiKeyProvider).GetMethod("ComputeAppEntropy", BindingFlags.NonPublic | BindingFlags.Static);
            var result1 = (byte[])method.Invoke(null, null);
            var result2 = (byte[])method.Invoke(null, null);

            Assert.AreEqual(result1, result2, "ComputeAppEntropy should be deterministic for the same Application.identifier.");
        }

        [Test]
        public void GetMasterKey_Returns32ByteKey()
        {
            byte[] key = _provider.GetMasterKey();

            Assert.AreEqual(32, key.Length, "Master key should be 32 bytes (AES-256).");
        }

        [Test]
        public void GetMasterKey_CalledTwice_ReturnsSameValue()
        {
            byte[] key1 = _provider.GetMasterKey();
            byte[] key2 = _provider.GetMasterKey();

            Assert.AreEqual(key1, key2, "Cached key should return the same value.");
            Assert.AreNotSame(key1, key2, "Should return defensive copies, not the same instance.");
        }

        [Test]
        public void HasMasterKey_BeforeAndAfterGeneration()
        {
            Assert.IsFalse(_provider.HasMasterKey(), "Should not have key before generation.");

            _provider.GetMasterKey();

            Assert.IsTrue(_provider.HasMasterKey(), "Should have key after generation.");
        }

        [Test]
        public void StoreMasterKey_ThenGetMasterKey_RoundTrips()
        {
            byte[] originalKey = new byte[32];
            new Random(42).NextBytes(originalKey);

            _provider.StoreMasterKey(originalKey);

            // Create a new provider with the same keyId to test persistence
            using var provider2 = new WindowsDpapiKeyProvider(_testKeyId);
            byte[] retrievedKey = provider2.GetMasterKey();

            Assert.AreEqual(originalKey, retrievedKey, "Retrieved key should match stored key.");
        }

        [Test]
        public void DeleteMasterKey_RemovesKey()
        {
            _provider.GetMasterKey();
            Assert.IsTrue(_provider.HasMasterKey());

            _provider.DeleteMasterKey();

            Assert.IsFalse(_provider.HasMasterKey(), "HasMasterKey should return false after deletion.");
        }

        [Test]
        public void Dispose_ThenGetMasterKey_ThrowsObjectDisposedException()
        {
            _provider.Dispose();

            Assert.Throws<ObjectDisposedException>(() => _provider.GetMasterKey());
        }
    }
}
#endif
