using System;
using NUnit.Framework;
using uDefend.Encryption;

namespace uDefend.Tests.Encryption
{
    [TestFixture]
    public class CryptoUtilityTests
    {
        [Test]
        public void GenerateRandomBytes_ReturnsRequestedLength()
        {
            int length = 32;
            byte[] bytes = CryptoUtility.GenerateRandomBytes(length);

            Assert.AreEqual(length, bytes.Length);
        }

        [Test]
        public void GenerateRandomBytes_ProducesDifferentValues()
        {
            byte[] bytes1 = CryptoUtility.GenerateRandomBytes(32);
            byte[] bytes2 = CryptoUtility.GenerateRandomBytes(32);

            Assert.AreNotEqual(bytes1, bytes2,
                "Two calls to GenerateRandomBytes should produce different values.");
        }

        [Test]
        public void ConstantTimeEquals_EqualArrays_ReturnsTrue()
        {
            var a = new byte[] { 1, 2, 3, 4, 5 };
            var b = new byte[] { 1, 2, 3, 4, 5 };

            Assert.IsTrue(CryptoUtility.ConstantTimeEquals(a, b));
        }

        [Test]
        public void ConstantTimeEquals_DifferentArrays_ReturnsFalse()
        {
            var a = new byte[] { 1, 2, 3, 4, 5 };
            var b = new byte[] { 1, 2, 3, 4, 6 };

            Assert.IsFalse(CryptoUtility.ConstantTimeEquals(a, b));
        }

        [Test]
        public void ConstantTimeEquals_DifferentLengths_ReturnsFalse()
        {
            var a = new byte[] { 1, 2, 3 };
            var b = new byte[] { 1, 2, 3, 4, 5 };

            Assert.IsFalse(CryptoUtility.ConstantTimeEquals(a, b));
        }

        [Test]
        public void SecureClear_ZerosBuffer()
        {
            var buffer = new byte[] { 0xDE, 0xAD, 0xBE, 0xEF };

            CryptoUtility.SecureClear(buffer);

            Assert.AreEqual(new byte[] { 0, 0, 0, 0 }, buffer);
        }
    }
}
