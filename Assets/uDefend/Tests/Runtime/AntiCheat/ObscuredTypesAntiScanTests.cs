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
        public void ObscuredInt_FakeValue_SetToPlaintext()
        {
            ObscuredInt obscured = 42;

            object boxed = obscured;
            int fakeValue = (int)GetField(boxed, "_fakeValue");

            Assert.AreEqual(42, fakeValue, "Fake value should contain the plaintext value.");
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

                object boxed = obscured;
                SetField(ref boxed, "_fakeValue", 999.0f);
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

                object boxed = obscured;
                SetField(ref boxed, "_fakeX", 999.0f);
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
        public void ObscuredString_FakeValueTampered_DetectsCheating()
        {
            bool cheatingDetected = false;
            Action handler = () => cheatingDetected = true;
            ObscuredString.OnCheatingDetected += handler;

            try
            {
                ObscuredString obscured = "Hello";

                object boxed = obscured;
                SetField(ref boxed, "_fakeValue", "Hacked");
                obscured = (ObscuredString)boxed;

                string _ = obscured;

                Assert.IsTrue(cheatingDetected, "Tampering with string _fakeValue should trigger cheating detection.");
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
