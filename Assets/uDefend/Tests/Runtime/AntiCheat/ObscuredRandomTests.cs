using System.Collections.Generic;
using NUnit.Framework;
using uDefend.AntiCheat;

namespace uDefend.Tests.AntiCheat
{
    [TestFixture]
    public class ObscuredRandomTests
    {
        [Test]
        public void Next_ReturnsNonZero()
        {
            for (int i = 0; i < 1000; i++)
            {
                int value = ObscuredRandom.Next();
                Assert.AreNotEqual(0, value, $"ObscuredRandom.Next() returned zero on iteration {i}.");
            }
        }

        [Test]
        public void NextLong_ReturnsNonZero()
        {
            for (int i = 0; i < 1000; i++)
            {
                long value = ObscuredRandom.NextLong();
                Assert.AreNotEqual(0L, value, $"ObscuredRandom.NextLong() returned zero on iteration {i}.");
            }
        }

        [Test]
        public void Next_MultipleCallsReturnDifferentValues()
        {
            var values = new HashSet<int>();
            for (int i = 0; i < 100; i++)
            {
                values.Add(ObscuredRandom.Next());
            }

            Assert.GreaterOrEqual(values.Count, 95,
                $"Expected at least 95 unique values out of 100, but got {values.Count}.");
        }

        [Test]
        public void NextLong_MultipleCallsReturnDifferentValues()
        {
            var values = new HashSet<long>();
            for (int i = 0; i < 100; i++)
            {
                values.Add(ObscuredRandom.NextLong());
            }

            Assert.GreaterOrEqual(values.Count, 95,
                $"Expected at least 95 unique values out of 100, but got {values.Count}.");
        }

        [Test]
        public void Next_AlwaysHasLsbSet()
        {
            for (int i = 0; i < 1000; i++)
            {
                int value = ObscuredRandom.Next();
                Assert.AreEqual(1, value & 1,
                    $"ObscuredRandom.Next() returned value with LSB=0 on iteration {i}: {value}");
            }
        }

        [Test]
        public void NextLong_AlwaysHasLsbSet()
        {
            for (int i = 0; i < 1000; i++)
            {
                long value = ObscuredRandom.NextLong();
                Assert.AreEqual(1L, value & 1L,
                    $"ObscuredRandom.NextLong() returned value with LSB=0 on iteration {i}: {value}");
            }
        }
    }
}
