using System;
using System.Text;
using NUnit.Framework;
using uDefend.Migration;

namespace uDefend.Tests.Migration
{
    [TestFixture]
    public class MigrationRunnerTests
    {
        private class TestMigration : IMigration
        {
            public ushort FromVersion { get; }
            public ushort ToVersion { get; }
            private readonly Func<byte[], byte[]> _transform;

            public TestMigration(ushort from, ushort to, Func<byte[], byte[]> transform)
            {
                FromVersion = from;
                ToVersion = to;
                _transform = transform;
            }

            public byte[] Migrate(byte[] data) => _transform(data);
        }

        [Test]
        public void Run_SingleStepMigration_TransformsData()
        {
            var runner = new MigrationRunner(2, 1);
            runner.Register(new TestMigration(1, 2, data =>
            {
                // Append a marker byte
                var result = new byte[data.Length + 1];
                Buffer.BlockCopy(data, 0, result, 0, data.Length);
                result[data.Length] = 0xAA;
                return result;
            }));

            byte[] original = Encoding.UTF8.GetBytes("v1-data");
            byte[] migrated = runner.Run(original, 1);

            Assert.AreEqual(original.Length + 1, migrated.Length);
            Assert.AreEqual(0xAA, migrated[migrated.Length - 1]);
        }

        [Test]
        public void Run_MultiStepMigration_AppliesSequentially()
        {
            var runner = new MigrationRunner(4, 1);

            // v1 -> v2: prefix with "v2:"
            runner.Register(new TestMigration(1, 2, data =>
            {
                var prefix = Encoding.UTF8.GetBytes("v2:");
                var result = new byte[prefix.Length + data.Length];
                Buffer.BlockCopy(prefix, 0, result, 0, prefix.Length);
                Buffer.BlockCopy(data, 0, result, prefix.Length, data.Length);
                return result;
            }));

            // v2 -> v3: prefix with "v3:"
            runner.Register(new TestMigration(2, 3, data =>
            {
                var prefix = Encoding.UTF8.GetBytes("v3:");
                var result = new byte[prefix.Length + data.Length];
                Buffer.BlockCopy(prefix, 0, result, 0, prefix.Length);
                Buffer.BlockCopy(data, 0, result, prefix.Length, data.Length);
                return result;
            }));

            // v3 -> v4: prefix with "v4:"
            runner.Register(new TestMigration(3, 4, data =>
            {
                var prefix = Encoding.UTF8.GetBytes("v4:");
                var result = new byte[prefix.Length + data.Length];
                Buffer.BlockCopy(prefix, 0, result, 0, prefix.Length);
                Buffer.BlockCopy(data, 0, result, prefix.Length, data.Length);
                return result;
            }));

            byte[] original = Encoding.UTF8.GetBytes("data");
            byte[] migrated = runner.Run(original, 1);

            string result = Encoding.UTF8.GetString(migrated);
            Assert.AreEqual("v4:v3:v2:data", result);
        }

        [Test]
        public void Run_VersionBelowMinSupported_ThrowsUnsupportedVersionException()
        {
            var runner = new MigrationRunner(5, 3);

            var ex = Assert.Throws<UnsupportedVersionException>(() =>
                runner.Run(new byte[] { 1 }, 2));

            Assert.AreEqual(2, ex.FileVersion);
            Assert.AreEqual(3, ex.MinSupportedVersion);
        }

        [Test]
        public void Run_VersionNewerThanCurrent_ThrowsMigrationException()
        {
            var runner = new MigrationRunner(3, 1);

            Assert.Throws<MigrationException>(() =>
                runner.Run(new byte[] { 1 }, 5));
        }

        [Test]
        public void Run_SameVersion_ReturnsDataUnchanged()
        {
            var runner = new MigrationRunner(3, 1);
            byte[] data = Encoding.UTF8.GetBytes("current");

            byte[] result = runner.Run(data, 3);

            Assert.AreSame(data, result);
        }

        [Test]
        public void Run_MissingMigrationStep_ThrowsMissingMigrationException()
        {
            var runner = new MigrationRunner(3, 1);
            // Register v1->v2 but NOT v2->v3
            runner.Register(new TestMigration(1, 2, data => data));

            var ex = Assert.Throws<MissingMigrationException>(() =>
                runner.Run(new byte[] { 1 }, 1));

            Assert.AreEqual(2, ex.FromVersion);
            Assert.AreEqual(3, ex.TargetVersion);
        }

        [Test]
        public void NeedsMigration_OlderVersion_ReturnsTrue()
        {
            var runner = new MigrationRunner(3, 1);
            Assert.IsTrue(runner.NeedsMigration(1));
            Assert.IsTrue(runner.NeedsMigration(2));
        }

        [Test]
        public void NeedsMigration_CurrentVersion_ReturnsFalse()
        {
            var runner = new MigrationRunner(3, 1);
            Assert.IsFalse(runner.NeedsMigration(3));
        }

        [Test]
        public void Register_NonSequentialMigration_ThrowsArgumentException()
        {
            var runner = new MigrationRunner(5, 1);

            Assert.Throws<ArgumentException>(() =>
                runner.Register(new TestMigration(1, 3, data => data)));
        }

        [Test]
        public void Register_DuplicateFromVersion_ThrowsArgumentException()
        {
            var runner = new MigrationRunner(3, 1);
            runner.Register(new TestMigration(1, 2, data => data));

            Assert.Throws<ArgumentException>(() =>
                runner.Register(new TestMigration(1, 2, data => data)));
        }

        [Test]
        public void Constructor_MinGreaterThanCurrent_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
                new MigrationRunner(2, 5));
        }

        [Test]
        public void Run_MigrationThrowsException_WrapsInMigrationException()
        {
            var runner = new MigrationRunner(2, 1);
            runner.Register(new TestMigration(1, 2, data =>
                throw new InvalidOperationException("bad migration")));

            var ex = Assert.Throws<MigrationException>(() =>
                runner.Run(new byte[] { 1 }, 1));

            Assert.IsNotNull(ex.InnerException);
            Assert.IsInstanceOf<InvalidOperationException>(ex.InnerException);
        }
    }
}
