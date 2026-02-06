using System;
using NUnit.Framework;
using uDefend.Serialization;

namespace uDefend.Tests.Serialization
{
    public class MessagePackSerializerTests
    {
        private MessagePackSerializerWrapper _serializer;

        [SetUp]
        public void SetUp()
        {
            _serializer = new MessagePackSerializerWrapper();
        }

        [Test]
        public void Serialize_WithoutMessagePack_ThrowsNotSupportedException()
        {
            Assert.Throws<NotSupportedException>(() => _serializer.Serialize(new object()));
        }

        [Test]
        public void Deserialize_WithoutMessagePack_ThrowsNotSupportedException()
        {
            Assert.Throws<NotSupportedException>(() => _serializer.Deserialize<object>(new byte[] { 0x00 }));
        }
    }
}
