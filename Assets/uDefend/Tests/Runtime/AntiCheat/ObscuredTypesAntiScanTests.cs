using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using uDefend.AntiCheat;

namespace uDefend.Tests.AntiCheat
{
    [TestFixture]
    public class ObscuredTypesAntiScanTests
    {
        [Test]
        public void ObscuredInt_FakeValue_IsObfuscated()
        {
            ObscuredInt obscured = 42;

            object boxed = obscured;
            int fakeValue = (int)GetField(boxed, "_fakeValue");
            int decoyKey = (int)GetField(boxed, "_decoyKey");

            // After anti-scan hardening, _fakeValue stores value ^ _decoyKey (not plaintext)
            Assert.AreEqual(42, fakeValue ^ decoyKey, "Fake value XOR decoyKey should equal the plaintext value.");
            Assert.AreNotEqual(42, fakeValue, "Fake value should NOT be the raw plaintext (anti-scan hardening).");
        }

        [Test]
        public void ObscuredInt_FakeValueTampered_DetectsCheating()
        {
            bool cheatingDetected = false;
            Action handler = () => cheatingDetected = true;
            ObscuredInt.OnCheatingDetected += handler;

            try
            {
                ObscuredInt obscured = 100;

                // Tamper with the fake value (simulating a memory scanner changing the decoy)
                object boxed = obscured;
                SetField(ref boxed, "_fakeValue", 999);
                obscured = (ObscuredInt)boxed;

                // Access the value to trigger detection
                int _ = obscured;

                Assert.IsTrue(cheatingDetected, "Tampering with _fakeValue should trigger cheating detection.");
            }
            finally
            {
                ObscuredInt.OnCheatingDetected -= handler;
            }
        }

        [Test]
        public void ObscuredInt_FakeValueNotTampered_NoCheatingDetected()
        {
            bool cheatingDetected = false;
            Action handler = () => cheatingDetected = true;
            ObscuredInt.OnCheatingDetected += handler;

            try
            {
                ObscuredInt obscured = 100;
                int value = obscured;

                Assert.AreEqual(100, value);
                Assert.IsFalse(cheatingDetected, "Normal access should not trigger cheating detection.");
            }
            finally
            {
                ObscuredInt.OnCheatingDetected -= handler;
            }
        }

        [Test]
        public void ObscuredFloat_FakeValueTampered_DetectsCheating()
        {
            bool cheatingDetected = false;
            Action handler = () => cheatingDetected = true;
            ObscuredFloat.OnCheatingDetected += handler;

            try
            {
                ObscuredFloat obscured = 3.14f;

                // After anti-scan hardening, _fakeValue is int (bit-converted float XOR decoyKey)
                object boxed = obscured;
                SetField(ref boxed, "_fakeValue", 999);
                obscured = (ObscuredFloat)boxed;

                float _ = obscured;

                Assert.IsTrue(cheatingDetected, "Tampering with float _fakeValue should trigger cheating detection.");
            }
            finally
            {
                ObscuredFloat.OnCheatingDetected -= handler;
            }
        }

        [Test]
        public void ObscuredVector3_FakeValueTampered_DetectsCheating()
        {
            bool cheatingDetected = false;
            Action handler = () => cheatingDetected = true;
            ObscuredVector3.OnCheatingDetected += handler;

            try
            {
                ObscuredVector3 obscured = new Vector3(1f, 2f, 3f);

                // After anti-scan hardening, _fakeX is int (bit-converted float XOR decoyKey)
                object boxed = obscured;
                SetField(ref boxed, "_fakeX", 999);
                obscured = (ObscuredVector3)boxed;

                Vector3 _ = obscured;

                Assert.IsTrue(cheatingDetected, "Tampering with Vector3 _fakeX should trigger cheating detection.");
            }
            finally
            {
                ObscuredVector3.OnCheatingDetected -= handler;
            }
        }

        [Test]
        public void ObscuredString_FakeHashTampered_DetectsCheating()
        {
            bool cheatingDetected = false;
            Action handler = () => cheatingDetected = true;
            ObscuredString.OnCheatingDetected += handler;

            try
            {
                ObscuredString obscured = "Hello";

                // After anti-scan hardening, _fakeHash is int (checksum XOR decoyKey)
                object boxed = obscured;
                SetField(ref boxed, "_fakeHash", 999);
                obscured = (ObscuredString)boxed;

                string _ = obscured;

                Assert.IsTrue(cheatingDetected, "Tampering with string _fakeHash should trigger cheating detection.");
            }
            finally
            {
                ObscuredString.OnCheatingDetected -= handler;
            }
        }

        [Test]
        public void ObscuredInt_DefaultStruct_NoFalsePositive()
        {
            bool cheatingDetected = false;
            Action handler = () => cheatingDetected = true;
            ObscuredInt.OnCheatingDetected += handler;

            try
            {
                ObscuredInt obscured = default;
                int value = obscured;

                Assert.AreEqual(0, value);
                Assert.IsFalse(cheatingDetected, "Default struct should not trigger false positive.");
            }
            finally
            {
                ObscuredInt.OnCheatingDetected -= handler;
            }
        }

        private static object GetField(object obj, string fieldName)
        {
            var field = obj.GetType().GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            return field.GetValue(obj);
        }

        private static void SetField(ref object boxed, string fieldName, object value)
        {
            var field = boxed.GetType().GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(boxed, value);
        }
    }
}
