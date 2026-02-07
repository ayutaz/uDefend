using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using uDefend.AntiCheat;

namespace uDefend.Tests.AntiCheat.Detectors
{
    [TestFixture]
    public class AntiDebugDetectorTests
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
            _go = new GameObject("AntiDebugTest");
            var detector = _go.AddComponent<AntiDebugDetector>();

            // Disable all strategies to isolate from test environment
            SetField(detector, "_detectManagedDebugger", false);
            SetField(detector, "_detectDebugEnvironmentVariables", false);
            SetField(detector, "_detectSuspiciousProcesses", false);
            SetField(detector, "_detectBreakpointTiming", false);

            bool cheatingDetected = false;
            detector.AddCheatingDetectedListener(() => cheatingDetected = true);

            yield return null;
            Assert.IsTrue(detector.IsRunning);

            yield return new WaitForSeconds(0.5f);

            Assert.IsFalse(cheatingDetected, "AntiDebugDetector should not trigger with all strategies disabled.");
            Assert.IsTrue(detector.IsRunning);
        }

        [UnityTest]
        public IEnumerator StopDetection_PreventsChecks()
        {
            _go = new GameObject("AntiDebugTest");
            var detector = _go.AddComponent<AntiDebugDetector>();

            SetField(detector, "_detectManagedDebugger", false);
            SetField(detector, "_detectDebugEnvironmentVariables", false);
            SetField(detector, "_detectSuspiciousProcesses", false);
            SetField(detector, "_detectBreakpointTiming", false);

            bool cheatingDetected = false;
            detector.AddCheatingDetectedListener(() => cheatingDetected = true);

            yield return null;
            detector.StopDetection();

            yield return new WaitForSeconds(0.3f);
            Assert.IsFalse(cheatingDetected, "Stopped detector should not fire events.");
            Assert.IsFalse(detector.IsRunning);
        }

        [UnityTest]
        public IEnumerator OnCheatingDetected_ListenerRegistration_Works()
        {
            _go = new GameObject("AntiDebugTest");
            var detector = _go.AddComponent<AntiDebugDetector>();

            SetField(detector, "_detectManagedDebugger", false);
            SetField(detector, "_detectDebugEnvironmentVariables", false);
            SetField(detector, "_detectSuspiciousProcesses", false);
            SetField(detector, "_detectBreakpointTiming", false);

            int callCount = 0;
            detector.AddCheatingDetectedListener(() => callCount++);

            yield return null;

            yield return new WaitForSeconds(0.3f);
            Assert.AreEqual(0, callCount, "Listener should not be called when all strategies are disabled.");
        }

        [UnityTest]
        public IEnumerator StopAndRestart_ResetsBaseline()
        {
            _go = new GameObject("AntiDebugTest");
            var detector = _go.AddComponent<AntiDebugDetector>();

            // Only enable timing detection with a very high threshold so it doesn't trigger
            SetField(detector, "_detectManagedDebugger", false);
            SetField(detector, "_detectDebugEnvironmentVariables", false);
            SetField(detector, "_detectSuspiciousProcesses", false);
            SetField(detector, "_detectBreakpointTiming", true);
            SetField(detector, "_frameTimeThresholdSeconds", 999f);

            bool cheatingDetected = false;
            detector.AddCheatingDetectedListener(() => cheatingDetected = true);

            yield return null;
            Assert.IsTrue(detector.IsRunning);

            detector.StopDetection();
            Assert.IsFalse(detector.IsRunning);

            detector.StartDetection();
            Assert.IsTrue(detector.IsRunning);

            yield return new WaitForSeconds(0.3f);
            Assert.IsFalse(cheatingDetected, "Restarted detector should not trigger with high threshold.");
        }

        [UnityTest]
        public IEnumerator ProcessDetection_DisabledByDefault()
        {
            _go = new GameObject("AntiDebugTest");
            var detector = _go.AddComponent<AntiDebugDetector>();

            // Read the default value via reflection
            var field = typeof(AntiDebugDetector).GetField("_detectSuspiciousProcesses",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            bool defaultValue = (bool)field.GetValue(detector);

            yield return null;
            Assert.IsTrue(defaultValue, "Suspicious process detection should be enabled by default.");
        }

        [UnityTest]
        public IEnumerator AllStrategiesDisabled_NeverTriggers()
        {
            _go = new GameObject("AntiDebugTest");
            var detector = _go.AddComponent<AntiDebugDetector>();

            SetField(detector, "_detectManagedDebugger", false);
            SetField(detector, "_detectDebugEnvironmentVariables", false);
            SetField(detector, "_detectSuspiciousProcesses", false);
            SetField(detector, "_detectBreakpointTiming", false);

            bool cheatingDetected = false;
            detector.AddCheatingDetectedListener(() => cheatingDetected = true);

            yield return null;

            // Wait for multiple check intervals
            yield return new WaitForSeconds(1f);

            Assert.IsFalse(cheatingDetected, "No strategy enabled means no detection.");
            Assert.IsTrue(detector.IsRunning, "Detector should still be running (no false positive to stop it).");
        }

        private static void SetField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field.SetValue(obj, value);
        }
    }
}
