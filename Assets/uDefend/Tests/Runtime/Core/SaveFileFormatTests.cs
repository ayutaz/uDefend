using System.IO;
using NUnit.Framework;
using uDefend.Core;

namespace uDefend.Tests.Core
{
    [TestFixture]
    public class SaveFileFormatTests
    {
        [Test]
        public void WriteRead_Header_RoundTrip()
        {
            ushort version = 42;
            ushort flags = SaveFileFormat.BuildFlags(true, EncryptionType.AesCbcHmac);

            using (var ms = new MemoryStream())
            {
                using (var writer = new BinaryWriter(ms, System.Text.Encoding.UTF8, true))
                {
                    SaveFileFormat.WriteHeader(writer, version, flags);
                }

                ms.Position = 0;
                using (var reader = new BinaryReader(ms))
                {
                    var (readVersion, readFlags) = SaveFileFormat.ReadHeader(reader);
                    Assert.AreEqual(version, readVersion);
                    Assert.AreEqual(flags, readFlags);
                }
            }
        }

        [Test]
        public void ValidateMagic_CorrectMagic_ReturnsTrue()
        {
            using (var ms = new MemoryStream(SaveFileFormat.MagicBytes))
            using (var reader = new BinaryReader(ms))
            {
                Assert.IsTrue(SaveFileFormat.ValidateMagic(reader));
            }
        }

        [Test]
        public void ValidateMagic_WrongMagic_ReturnsFalse()
        {
            byte[] wrongMagic = { 0x00, 0x00, 0x00, 0x00 };
            using (var ms = new MemoryStream(wrongMagic))
            using (var reader = new BinaryReader(ms))
            {
                Assert.IsFalse(SaveFileFormat.ValidateMagic(reader));
            }
        }

        [Test]
        public void ValidateMagic_TruncatedData_ReturnsFalse()
        {
            byte[] truncated = { 0x75, 0x53 };
            using (var ms = new MemoryStream(truncated))
            using (var reader = new BinaryReader(ms))
            {
                Assert.IsFalse(SaveFileFormat.ValidateMagic(reader));
            }
        }

        [Test]
        public void BuildFlags_ParseFlags_RoundTrip()
        {
            ushort flags = SaveFileFormat.BuildFlags(true, EncryptionType.AesGcm);
            var (compressed, encryption) = SaveFileFormat.ParseFlags(flags);

            Assert.IsTrue(compressed);
            Assert.AreEqual(EncryptionType.AesGcm, encryption);
        }

        [Test]
        public void BuildFlags_NoCompression_AesCbcHmac()
        {
            ushort flags = SaveFileFormat.BuildFlags(false, EncryptionType.AesCbcHmac);
            var (compressed, encryption) = SaveFileFormat.ParseFlags(flags);

            Assert.IsFalse(compressed);
            Assert.AreEqual(EncryptionType.AesCbcHmac, encryption);
        }

        [Test]
        public void ReadHeader_WrongMagic_ThrowsInvalidDataException()
        {
            byte[] badFile = { 0xDE, 0xAD, 0xBE, 0xEF, 0x01, 0x00, 0x00, 0x00 };
            using (var ms = new MemoryStream(badFile))
            using (var reader = new BinaryReader(ms))
            {
                Assert.Throws<InvalidDataException>(() => SaveFileFormat.ReadHeader(reader));
            }
        }
    }
}
