using System;
using System.Text;
using NUnit.Framework;
using uDefend.Serialization;

namespace uDefend.Tests.Serialization
{
    [System.Serializable]
    public class TestData
    {
        public string name;
        public int score;
        public float time;
    }

    [System.Serializable]
    public class TestDataV2
    {
        public string name;
        public int score;
        public float time;
        public int level;
    }

    public class JsonUtilitySerializerTests
    {
        private JsonUtilitySerializer _serializer;

        [SetUp]
        public void SetUp()
        {
            _serializer = new JsonUtilitySerializer();
        }

        [Test]
        public void Serialize_Deserialize_RoundTrip_ReturnsOriginalData()
        {
            var original = new TestData { name = "Player1", score = 42, time = 3.14f };

            var bytes = _serializer.Serialize(original);
            var result = _serializer.Deserialize<TestData>(bytes);

            Assert.AreEqual(original.name, result.name);
            Assert.AreEqual(original.score, result.score);
            Assert.AreEqual(original.time, result.time, 0.001f);
        }

        [Test]
        public void Serialize_NullData_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => _serializer.Serialize<TestData>(null));
        }

        [Test]
        public void Deserialize_NullData_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => _serializer.Deserialize<TestData>(null));
        }

        [Test]
        public void Deserialize_EmptyBytes_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() => _serializer.Deserialize<TestData>(Array.Empty<byte>()));
        }

        [Test]
        public void SchemaEvolution_NewFieldAdded_DeserializesWithDefault()
        {
            var original = new TestData { name = "Player1", score = 100, time = 5.5f };
            var bytes = _serializer.Serialize(original);

            var result = _serializer.Deserialize<TestDataV2>(bytes);

            Assert.AreEqual(original.name, result.name);
            Assert.AreEqual(original.score, result.score);
            Assert.AreEqual(original.time, result.time, 0.001f);
            Assert.AreEqual(0, result.level);
        }
    }
}
