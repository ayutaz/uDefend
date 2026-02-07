using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using uDefend.AntiCheat;

namespace uDefend.Tests.AntiCheat.Detectors
{
    [TestFixture]
    public class ObscuredCheatingDetectorTests
    {
        private GameObject _go;

        [TearDown]
        public void TearDown()
        {
            if (_go != null)
            {
                Object.Destroy(_go);
                _go = null;
            }
        }

        [UnityTest]
        public IEnumerator NormalConditions_DoesNotTrigger()
        {
            _go = new GameObject("ObscuredCheatingTest");
            var detector = _go.AddComponent<ObscuredCheatingDetector>();

            bool cheatingDetected = false;
            detector.AddCheatingDetectedListener(() => cheatingDetected = true);

            yield return null;
            Assert.IsTrue(detector.IsRunning);

            // Use ObscuredTypes normally - should not trigger
            ObscuredInt normalInt = 42;
            int value = normalInt;
            Assert.AreEqual(42, value);

            yield return new WaitForSeconds(0.3f);

            Assert.IsFalse(cheatingDetected, "Should not trigger when ObscuredTypes are used normally.");
            Assert.IsTrue(detector.IsRunning);
        }

        [UnityTest]
        public IEnumerator DetectsObscuredIntTampering()
        {
            _go = new GameObject("ObscuredCheatingTest");
            var detector = _go.AddComponent<ObscuredCheatingDetector>();

            bool cheatingDetected = false;
            detector.AddCheatingDetectedListener(() => cheatingDetected = true);

            // Shorten check interval so PerformCheck runs quickly after tampering
            SetCheckInterval(detector, 0.05f);

            yield return null;
            Assert.IsTrue(detector.IsRunning);

            // Tamper with ObscuredInt using reflection (same pattern as ObscuredTypesTests)
            ObscuredInt obscured = 100;

            var field = typeof(ObscuredInt).GetField("_encryptedValue",
                BindingFlags.NonPublic | BindingFlags.Instance);
            object boxed = obscured;
            field.SetValue(boxed, 999999);
            obscured = (ObscuredInt)boxed;

            // Access the value to trigger cheat detection in ObscuredInt
            int _ = obscured;

            // Wait for the detector's periodic check to pick up the flag
            yield return new WaitForSeconds(0.3f);

            Assert.IsTrue(cheatingDetected, "Detector should fire when ObscuredInt is tampered with.");
        }

        [UnityTest]
        public IEnumerator DetectsObscuredFloatTampering()
        {
            _go = new GameObject("ObscuredCheatingTest");
            var detector = _go.AddComponent<ObscuredCheatingDetector>();

            bool cheatingDetected = false;
            detector.AddCheatingDetectedListener(() => cheatingDetected = true);

            // Shorten check interval so PerformCheck runs quickly after tampering
            SetCheckInterval(detector, 0.05f);

            yield return null;
            Assert.IsTrue(detector.IsRunning);

            // Tamper with ObscuredFloat using reflection
            ObscuredFloat obscured = 3.14f;

            var field = typeof(ObscuredFloat).GetField("_encryptedValue",
                BindingFlags.NonPublic | BindingFlags.Instance);
            object boxed = obscured;
            field.SetValue(boxed, 999999);
            obscured = (ObscuredFloat)boxed;

            // Access the value to trigger cheat detection
            float _ = obscured;

            yield return new WaitForSeconds(0.3f);

            Assert.IsTrue(cheatingDetected, "Detector should fire when ObscuredFloat is tampered with.");
        }

        [UnityTest]
        public IEnumerator OnDetectionStopped_UnsubscribesEvents()
        {
            _go = new GameObject("ObscuredCheatingTest");
            var detector = _go.AddComponent<ObscuredCheatingDetector>();

            bool cheatingDetected = false;
            detector.AddCheatingDetectedListener(() => cheatingDetected = true);

            yield return null;
            Assert.IsTrue(detector.IsRunning);

            // Stop detection - this should unsubscribe from ObscuredType events
            detector.StopDetection();
            Assert.IsFalse(detector.IsRunning);

            // Now tamper with an ObscuredInt
            ObscuredInt obscured = 100;

            var field = typeof(ObscuredInt).GetField("_encryptedValue",
                BindingFlags.NonPublic | BindingFlags.Instance);
            object boxed = obscured;
            field.SetValue(boxed, 999999);
            obscured = (ObscuredInt)boxed;

            // Access value - ObscuredInt.OnCheatingDetected fires, but detector should not see it
            int _ = obscured;

            yield return new WaitForSeconds(0.3f);

            Assert.IsFalse(cheatingDetected,
                "Detector should not fire after StopDetection even if ObscuredTypes are tampered with.");
        }
        private static void SetCheckInterval(DetectorBase detector, float interval)
        {
            var field = typeof(DetectorBase).GetField("_checkInterval",
                BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(detector, interval);
        }
    }
}
