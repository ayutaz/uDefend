using System;
using System.Reflection;
using NUnit.Framework;
using uDefend.AntiCheat;

namespace uDefend.Tests.AntiCheat
{
    public class ObscuredTypesTests
    {
        // ===== ObscuredInt Tests =====

        [Test]
        public void ObscuredInt_ImplicitConversion_RoundTrip()
        {
            ObscuredInt obscured = 42;
            int result = obscured;

            Assert.AreEqual(42, result);
        }

        [Test]
        public void ObscuredInt_Arithmetic_MatchesRegularInt()
        {
            ObscuredInt a = 10;
            ObscuredInt b = 3;

            Assert.AreEqual(13, (int)(a + b));
            Assert.AreEqual(7, (int)(a - b));
            Assert.AreEqual(30, (int)(a * b));
            Assert.AreEqual(3, (int)(a / b));
            Assert.AreEqual(1, (int)(a % b));

            ObscuredInt c = 5;
            c++;
            Assert.AreEqual(6, (int)c);
            c--;
            Assert.AreEqual(5, (int)c);
        }

        [Test]
        public void ObscuredInt_Comparison_MatchesRegularInt()
        {
            ObscuredInt a = 10;
            ObscuredInt b = 20;
            ObscuredInt c = 10;

            Assert.IsTrue(a < b);
            Assert.IsTrue(b > a);
            Assert.IsTrue(a <= c);
            Assert.IsTrue(a >= c);
            Assert.IsTrue(a == c);
            Assert.IsTrue(a != b);
        }

        [Test]
        public void ObscuredInt_CheatingDetected_WhenMemoryTampered()
        {
            bool cheatingDetected = false;
            Action handler = () => cheatingDetected = true;
            ObscuredInt.OnCheatingDetected += handler;

            try
            {
                ObscuredInt obscured = 100;

                // Tamper with encrypted value using reflection
                var field = typeof(ObscuredInt).GetField("_encryptedValue", BindingFlags.NonPublic | BindingFlags.Instance);
                object boxed = obscured;
                field.SetValue(boxed, 999999);
                obscured = (ObscuredInt)boxed;

                // Access the value to trigger cheat detection
                int _ = obscured;

                Assert.IsTrue(cheatingDetected);
            }
            finally
            {
                ObscuredInt.OnCheatingDetected -= handler;
            }
        }

        [Test]
        public void ObscuredInt_ZeroValue_WorksCorrectly()
        {
            ObscuredInt obscured = 0;
            Assert.AreEqual(0, (int)obscured);
        }

        [Test]
        public void ObscuredInt_NegativeValue_WorksCorrectly()
        {
            ObscuredInt obscured = -42;
            Assert.AreEqual(-42, (int)obscured);
        }

        // ===== ObscuredFloat Tests =====

        [Test]
        public void ObscuredFloat_ImplicitConversion_RoundTrip()
        {
            ObscuredFloat obscured = 3.14f;
            float result = obscured;

            Assert.AreEqual(3.14f, result, 0.0001f);
        }

        [Test]
        public void ObscuredFloat_Arithmetic_Precision()
        {
            ObscuredFloat a = 10.5f;
            ObscuredFloat b = 3.2f;

            Assert.AreEqual(10.5f + 3.2f, (float)(a + b), 0.0001f);
            Assert.AreEqual(10.5f - 3.2f, (float)(a - b), 0.0001f);
            Assert.AreEqual(10.5f * 3.2f, (float)(a * b), 0.001f);
            Assert.AreEqual(10.5f / 3.2f, (float)(a / b), 0.001f);
        }

        [Test]
        public void ObscuredFloat_Comparison_MatchesRegularFloat()
        {
            ObscuredFloat a = 1.5f;
            ObscuredFloat b = 2.5f;
            ObscuredFloat c = 1.5f;

            Assert.IsTrue(a < b);
            Assert.IsTrue(b > a);
            Assert.IsTrue(a <= c);
            Assert.IsTrue(a >= c);
            Assert.IsTrue(a == c);
            Assert.IsTrue(a != b);
        }

        [Test]
        public void ObscuredFloat_CheatingDetected_WhenMemoryTampered()
        {
            bool cheatingDetected = false;
            Action handler = () => cheatingDetected = true;
            ObscuredFloat.OnCheatingDetected += handler;

            try
            {
                ObscuredFloat obscured = 99.9f;

                var field = typeof(ObscuredFloat).GetField("_encryptedValue", BindingFlags.NonPublic | BindingFlags.Instance);
                object boxed = obscured;
                field.SetValue(boxed, 12345);
                obscured = (ObscuredFloat)boxed;

                float _ = obscured;

                Assert.IsTrue(cheatingDetected);
            }
            finally
            {
                ObscuredFloat.OnCheatingDetected -= handler;
            }
        }

        // ===== ObscuredBool Tests =====

        [Test]
        public void ObscuredBool_ImplicitConversion_RoundTrip()
        {
            ObscuredBool obscuredTrue = true;
            ObscuredBool obscuredFalse = false;

            Assert.IsTrue(obscuredTrue);
            Assert.IsFalse(obscuredFalse);
        }

        [Test]
        public void ObscuredBool_LogicalOperators()
        {
            ObscuredBool t = true;
            ObscuredBool f = false;

            Assert.IsFalse(!t);
            Assert.IsTrue(!f);
            Assert.IsFalse((bool)(t & f));
            Assert.IsTrue((bool)(t | f));
            Assert.IsTrue((bool)(t ^ f));
            Assert.IsFalse((bool)(t ^ t));
        }

        [Test]
        public void ObscuredBool_Equality()
        {
            ObscuredBool a = true;
            ObscuredBool b = true;
            ObscuredBool c = false;

            Assert.IsTrue(a == b);
            Assert.IsTrue(a != c);
        }

        [Test]
        public void ObscuredBool_CheatingDetected_WhenMemoryTampered()
        {
            bool cheatingDetected = false;
            Action handler = () => cheatingDetected = true;
            ObscuredBool.OnCheatingDetected += handler;

            try
            {
                ObscuredBool obscured = true;

                var field = typeof(ObscuredBool).GetField("_encryptedValue", BindingFlags.NonPublic | BindingFlags.Instance);
                object boxed = obscured;
                field.SetValue(boxed, 777);
                obscured = (ObscuredBool)boxed;

                bool _ = obscured;

                Assert.IsTrue(cheatingDetected);
            }
            finally
            {
                ObscuredBool.OnCheatingDetected -= handler;
            }
        }

        // ===== ObscuredString Tests =====

        [Test]
        public void ObscuredString_ImplicitConversion_RoundTrip()
        {
            ObscuredString obscured = "Hello, World!";
            string result = obscured;

            Assert.AreEqual("Hello, World!", result);
        }

        [Test]
        public void ObscuredString_NullHandling()
        {
            ObscuredString obscured = (string)null;
            string result = obscured;

            Assert.IsNull(result);
            Assert.AreEqual(0, obscured.Length);
        }

        [Test]
        public void ObscuredString_Concatenation()
        {
            ObscuredString a = "Hello";
            ObscuredString b = ", World!";

            ObscuredString result = a + b;
            Assert.AreEqual("Hello, World!", (string)result);

            ObscuredString result2 = a + " suffix";
            Assert.AreEqual("Hello suffix", (string)result2);

            ObscuredString result3 = "prefix " + b;
            Assert.AreEqual("prefix , World!", (string)result3);
        }

        [Test]
        public void ObscuredString_EmptyString_WorksCorrectly()
        {
            ObscuredString obscured = "";
            string result = obscured;

            Assert.AreEqual("", result);
            Assert.AreEqual(0, obscured.Length);
        }

        [Test]
        public void ObscuredString_Equality()
        {
            ObscuredString a = "test";
            ObscuredString b = "test";
            ObscuredString c = "other";

            Assert.IsTrue(a == b);
            Assert.IsTrue(a != c);
        }

        [Test]
        public void ObscuredString_CheatingDetected_WhenMemoryTampered()
        {
            bool cheatingDetected = false;
            Action handler = () => cheatingDetected = true;
            ObscuredString.OnCheatingDetected += handler;

            try
            {
                ObscuredString obscured = "secret";

                var field = typeof(ObscuredString).GetField("_encryptedChars", BindingFlags.NonPublic | BindingFlags.Instance);
                object boxed = obscured;
                char[] tampered = new char[] { 'X', 'X', 'X', 'X', 'X', 'X' };
                field.SetValue(boxed, tampered);
                obscured = (ObscuredString)boxed;

                string _ = obscured;

                Assert.IsTrue(cheatingDetected);
            }
            finally
            {
                ObscuredString.OnCheatingDetected -= handler;
            }
        }

        [Test]
        public void ObscuredString_UnicodeContent_WorksCorrectly()
        {
            ObscuredString obscured = "こんにちは世界";
            string result = obscured;

            Assert.AreEqual("こんにちは世界", result);
        }
    }
}
