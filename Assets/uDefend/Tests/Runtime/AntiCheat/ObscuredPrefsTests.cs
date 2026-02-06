using System;
using System.IO;
using NUnit.Framework;
using uDefend.AntiCheat;
using UnityEngine;

namespace uDefend.Tests.AntiCheat
{
    [TestFixture]
    public class ObscuredPrefsTests
    {
        [SetUp]
        public void SetUp()
        {
            ObscuredPrefs.DeleteAll();
            ObscuredPrefs.Save();
        }

        [TearDown]
        public void TearDown()
        {
            ObscuredPrefs.DeleteAll();
            ObscuredPrefs.Save();
        }

        [Test]
        public void SetGet_Int_RoundTrip()
        {
            ObscuredPrefs.SetInt("score", 12345);
            var result = ObscuredPrefs.GetInt("score");
            Assert.AreEqual(12345, result);
        }

        [Test]
        public void SetGet_Float_RoundTrip()
        {
            ObscuredPrefs.SetFloat("time", 3.14f);
            var result = ObscuredPrefs.GetFloat("time");
            Assert.AreEqual(3.14f, result, 0.001f);
        }

        [Test]
        public void SetGet_String_RoundTrip()
        {
            ObscuredPrefs.SetString("name", "Player1");
            var result = ObscuredPrefs.GetString("name");
            Assert.AreEqual("Player1", result);
        }

        [Test]
        public void SetGet_Bool_RoundTrip()
        {
            ObscuredPrefs.SetBool("flag", true);
            Assert.IsTrue(ObscuredPrefs.GetBool("flag"));

            ObscuredPrefs.SetBool("flag", false);
            Assert.IsFalse(ObscuredPrefs.GetBool("flag"));
        }

        [Test]
        public void HasKey_AfterSet_ReturnsTrue()
        {
            Assert.IsFalse(ObscuredPrefs.HasKey("test"));
            ObscuredPrefs.SetInt("test", 1);
            Assert.IsTrue(ObscuredPrefs.HasKey("test"));
        }

        [Test]
        public void DeleteKey_RemovesKey()
        {
            ObscuredPrefs.SetInt("toDelete", 42);
            Assert.IsTrue(ObscuredPrefs.HasKey("toDelete"));

            ObscuredPrefs.DeleteKey("toDelete");
            Assert.IsFalse(ObscuredPrefs.HasKey("toDelete"));
        }

        [Test]
        public void GetInt_DefaultValue_WhenKeyMissing()
        {
            var result = ObscuredPrefs.GetInt("missing", 99);
            Assert.AreEqual(99, result);
        }

        [Test]
        public void TamperingDetected_WhenValueModified()
        {
            bool tampered = false;
            Action handler = () => tampered = true;
            ObscuredPrefs.OnTamperingDetected += handler;

            try
            {
                ObscuredPrefs.SetInt("secure", 100);

                // Directly tamper with the stored value in PlayerPrefs
                var hashedKey = ObscuredPrefs.HashKey("secure");
                PlayerPrefs.SetString(hashedKey, "tampered_garbage");

                ObscuredPrefs.GetInt("secure");

                Assert.IsTrue(tampered, "OnTamperingDetected should have been invoked.");
            }
            finally
            {
                ObscuredPrefs.OnTamperingDetected -= handler;
            }
        }
    }

    [TestFixture]
    public class ObscuredFilePrefsTests
    {
        private string _testFilePath;
        private ObscuredFilePrefs _prefs;

        [SetUp]
        public void SetUp()
        {
            _testFilePath = Path.Combine(Application.temporaryCachePath, "test_prefs.dat");
            if (File.Exists(_testFilePath))
                File.Delete(_testFilePath);
            _prefs = new ObscuredFilePrefs(_testFilePath);
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(_testFilePath))
                File.Delete(_testFilePath);
        }

        [Test]
        public void SetGet_Int_RoundTrip()
        {
            _prefs.SetInt("score", 9999);
            Assert.AreEqual(9999, _prefs.GetInt("score"));
        }

        [Test]
        public void SetGet_Float_RoundTrip()
        {
            _prefs.SetFloat("rate", 2.718f);
            Assert.AreEqual(2.718f, _prefs.GetFloat("rate"), 0.001f);
        }

        [Test]
        public void SetGet_String_RoundTrip()
        {
            _prefs.SetString("greeting", "Hello World");
            Assert.AreEqual("Hello World", _prefs.GetString("greeting"));
        }

        [Test]
        public void SetGet_Bool_RoundTrip()
        {
            _prefs.SetBool("active", true);
            Assert.IsTrue(_prefs.GetBool("active"));
        }

        [Test]
        public void SaveLoad_RoundTrip()
        {
            _prefs.SetInt("a", 1);
            _prefs.SetString("b", "test");
            _prefs.SetFloat("c", 1.5f);
            _prefs.SetBool("d", true);
            _prefs.Save();

            var loaded = new ObscuredFilePrefs(_testFilePath);
            loaded.Load();

            Assert.AreEqual(1, loaded.GetInt("a"));
            Assert.AreEqual("test", loaded.GetString("b"));
            Assert.AreEqual(1.5f, loaded.GetFloat("c"), 0.001f);
            Assert.IsTrue(loaded.GetBool("d"));
        }

        [Test]
        public void HasKey_AfterSet_ReturnsTrue()
        {
            Assert.IsFalse(_prefs.HasKey("x"));
            _prefs.SetInt("x", 1);
            Assert.IsTrue(_prefs.HasKey("x"));
        }

        [Test]
        public void DeleteKey_RemovesKey()
        {
            _prefs.SetInt("del", 1);
            _prefs.DeleteKey("del");
            Assert.IsFalse(_prefs.HasKey("del"));
        }

        [Test]
        public void Load_NonexistentFile_NoError()
        {
            var prefs = new ObscuredFilePrefs(Path.Combine(Application.temporaryCachePath, "nonexistent.dat"));
            Assert.DoesNotThrow(() => prefs.Load());
        }
    }
}
