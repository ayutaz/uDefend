using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using uDefend.Core;
using uDefend.Encryption;
using uDefend.KeyManagement;
using uDefend.Serialization;

namespace uDefend.Tests.Core
{
    [TestFixture]
    public class SaveManagerTests
    {
        private string _testDir;
        private SaveSettings _settings;
        private Pbkdf2KeyProvider _keyProvider;
        private SaveManager _manager;

        [Serializable]
        private class TestData
        {
            public string Name;
            public int Score;
            public float Time;
        }

        [SetUp]
        public void SetUp()
        {
            _testDir = Path.Combine(Application.temporaryCachePath, "uDefendTests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_testDir);

            _settings = ScriptableObject.CreateInstance<SaveSettings>();
            _settings.SavePath = _testDir;
            _settings.Encryption = EncryptionType.AesCbcHmac;
            _settings.Serializer = SerializerType.Json;
            _settings.CurrentVersion = 1;
            _settings.AutoBackup = false;

            var passphrase = System.Text.Encoding.UTF8.GetBytes("test-passphrase");
            var salt = CryptoUtility.GenerateRandomBytes(16);
            _keyProvider = new Pbkdf2KeyProvider(passphrase, salt);

            _manager = new SaveManager(_settings, _keyProvider);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_testDir))
            {
                try { Directory.Delete(_testDir, true); }
                catch { /* best effort */ }
            }

            if (_settings != null)
                UnityEngine.Object.DestroyImmediate(_settings);

            _keyProvider?.Dispose();
        }

        [Test]
        public async Task SaveAsync_LoadAsync_RoundTrip()
        {
            var data = new TestData { Name = "Player1", Score = 100, Time = 12.5f };

            await _manager.SaveAsync("slot1", data);
            var loaded = await _manager.LoadAsync<TestData>("slot1");

            Assert.AreEqual(data.Name, loaded.Name);
            Assert.AreEqual(data.Score, loaded.Score);
            Assert.AreEqual(data.Time, loaded.Time, 0.001f);
        }

        [Test]
        public async Task SaveAsync_LoadAsync_WithEncryption_RoundTrip()
        {
            _settings.Encryption = EncryptionType.AesCbcHmac;
            var manager = new SaveManager(_settings, _keyProvider);

            var data = new TestData { Name = "Encrypted", Score = 999, Time = 60.0f };

            await manager.SaveAsync("encrypted_slot", data);

            // Verify file is not plaintext
            string filePath = Path.Combine(_testDir, "encrypted_slot.sav");
            byte[] rawBytes = File.ReadAllBytes(filePath);
            string rawString = System.Text.Encoding.UTF8.GetString(rawBytes);
            Assert.IsFalse(rawString.Contains("Encrypted"),
                "Encrypted file should not contain plaintext data.");

            var loaded = await manager.LoadAsync<TestData>("encrypted_slot");
            Assert.AreEqual("Encrypted", loaded.Name);
            Assert.AreEqual(999, loaded.Score);
        }

        [Test]
        public async Task Exists_AfterSave_ReturnsTrue()
        {
            Assert.IsFalse(_manager.Exists("slot_exists"));

            var data = new TestData { Name = "Test", Score = 1, Time = 0f };
            await _manager.SaveAsync("slot_exists", data);

            Assert.IsTrue(_manager.Exists("slot_exists"));
        }

        [Test]
        public async Task Delete_RemovesFile()
        {
            var data = new TestData { Name = "Delete", Score = 0, Time = 0f };
            await _manager.SaveAsync("slot_delete", data);
            Assert.IsTrue(_manager.Exists("slot_delete"));

            _manager.Delete("slot_delete");
            Assert.IsFalse(_manager.Exists("slot_delete"));
        }

        [Test]
        public async Task LoadAsync_TamperedFile_ThrowsException()
        {
            var data = new TestData { Name = "Tamper", Score = 42, Time = 1.0f };
            await _manager.SaveAsync("slot_tamper", data);

            // Tamper with the file
            string filePath = Path.Combine(_testDir, "slot_tamper.sav");
            byte[] fileBytes = File.ReadAllBytes(filePath);
            // Tamper in the encrypted data area (after header)
            if (fileBytes.Length > 20)
                fileBytes[15] ^= 0xFF;
            File.WriteAllBytes(filePath, fileBytes);

            Assert.ThrowsAsync<TamperDetectedException>(async () =>
                await _manager.LoadAsync<TestData>("slot_tamper"));
        }

        [Test]
        public async Task GetAllSlotIds_ReturnsCorrectList()
        {
            var data = new TestData { Name = "Multi", Score = 0, Time = 0f };
            await _manager.SaveAsync("alpha", data);
            await _manager.SaveAsync("beta", data);
            await _manager.SaveAsync("gamma", data);

            string[] slots = _manager.GetAllSlotIds();
            Assert.AreEqual(3, slots.Length);
            CollectionAssert.Contains(slots, "alpha");
            CollectionAssert.Contains(slots, "beta");
            CollectionAssert.Contains(slots, "gamma");
        }

        [Test]
        public void LoadAsync_NonexistentSlot_ThrowsFileNotFoundException()
        {
            Assert.ThrowsAsync<FileNotFoundException>(async () =>
                await _manager.LoadAsync<TestData>("nonexistent"));
        }

        [Test]
        public async Task SaveAsync_AtomicWrite_NoTmpFileRemains()
        {
            var data = new TestData { Name = "Atomic", Score = 1, Time = 0f };
            await _manager.SaveAsync("slot_atomic", data);

            string tmpPath = Path.Combine(_testDir, "slot_atomic.sav.tmp");
            Assert.IsFalse(File.Exists(tmpPath), "Temp file should not remain after successful write.");
        }
    }
}
