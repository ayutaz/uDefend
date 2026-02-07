using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using uDefend.AntiCheat;

namespace uDefend.Tests.AntiCheat.Detectors
{
    [TestFixture]
    public class SpeedHackDetectorTests
    {
        private GameObject _go;
        private float _savedTimeScale;

        [SetUp]
        public void SetUp()
        {
            _savedTimeScale = Time.timeScale;
        }

        [TearDown]
        public void TearDown()
        {
            Time.timeScale = _savedTimeScale;

            if (_go != null)
            {
                Object.Destroy(_go);
                _go = null;
            }
        }

        private SpeedHackDetector CreateDetectorWithFastCheck(float checkInterval = 0.05f)
        {
            var detector = _go.AddComponent<SpeedHackDetector>();

            var checkIntervalField = typeof(DetectorBase).GetField("_checkInterval", BindingFlags.NonPublic | BindingFlags.Instance);
            checkIntervalField.SetValue(detector, checkInterval);

            return detector;
        }

        [UnityTest]
        public IEnumerator NormalConditions_DoesNotTrigger()
        {
            _go = new GameObject("SpeedHackTest");
            var detector = _go.AddComponent<SpeedHackDetector>();

            bool cheatingDetected = false;
            detector.AddCheatingDetectedListener(() => cheatingDetected = true);

            yield return null;
            Assert.IsTrue(detector.IsRunning);

            // Wait several check intervals - should not trigger under normal conditions
            yield return new WaitForSeconds(0.5f);

            Assert.IsFalse(cheatingDetected, "SpeedHackDetector should not trigger under normal conditions.");
            Assert.IsTrue(detector.IsRunning, "Detector should still be running.");
        }

        [UnityTest]
        public IEnumerator OnCheatingDetected_ListenerRegistration_Works()
        {
            _go = new GameObject("SpeedHackTest");
            var detector = _go.AddComponent<SpeedHackDetector>();

            int callCount = 0;
            detector.AddCheatingDetectedListener(() => callCount++);

            yield return null;

            // Verify listener is registered and not called spuriously
            yield return new WaitForSeconds(0.3f);
            Assert.AreEqual(0, callCount, "Listener should not be called under normal conditions.");
        }

        [UnityTest]
        public IEnumerator StopDetection_PreventsChecks()
        {
            _go = new GameObject("SpeedHackTest");
            var detector = _go.AddComponent<SpeedHackDetector>();

            bool cheatingDetected = false;
            detector.AddCheatingDetectedListener(() => cheatingDetected = true);

            yield return null;
            detector.StopDetection();

            yield return new WaitForSeconds(0.3f);
            Assert.IsFalse(cheatingDetected, "Stopped detector should not fire events.");
            Assert.IsFalse(detector.IsRunning);
        }

        [UnityTest]
        public IEnumerator TimeScaleAboveMax_TriggersDetection()
        {
            _go = new GameObject("SpeedHackTest_TimeScale");
            var detector = CreateDetectorWithFastCheck();

            var maxScaleField = typeof(SpeedHackDetector).GetField("_maxAllowedTimeScale", BindingFlags.NonPublic | BindingFlags.Instance);
            maxScaleField.SetValue(detector, 3f);

            bool cheatingDetected = false;
            detector.AddCheatingDetectedListener(() => cheatingDetected = true);

            yield return null; // Start()
            Assert.IsTrue(detector.IsRunning);

            Time.timeScale = 4f; // Above threshold of 3f
            yield return new WaitForSecondsRealtime(0.2f);

            Assert.IsTrue(cheatingDetected, "Should detect timeScale above max threshold.");
            Assert.IsFalse(detector.IsRunning, "Detector should stop after detection.");
        }

        [UnityTest]
        public IEnumerator TimeScaleAtMax_DoesNotTrigger()
        {
            _go = new GameObject("SpeedHackTest_TimeScale");
            var detector = CreateDetectorWithFastCheck();

            var maxScaleField = typeof(SpeedHackDetector).GetField("_maxAllowedTimeScale", BindingFlags.NonPublic | BindingFlags.Instance);
            maxScaleField.SetValue(detector, 3f);

            bool cheatingDetected = false;
            detector.AddCheatingDetectedListener(() => cheatingDetected = true);

            yield return null; // Start()

            Time.timeScale = 3f; // Exactly at threshold (uses > not >=)
            yield return new WaitForSecondsRealtime(0.2f);

            Assert.IsFalse(cheatingDetected, "Should not trigger when timeScale equals max (strict greater-than).");
            Assert.IsTrue(detector.IsRunning);
        }

        [UnityTest]
        public IEnumerator TimeScaleBelowMax_DoesNotTrigger()
        {
            _go = new GameObject("SpeedHackTest_TimeScale");
            var detector = CreateDetectorWithFastCheck();

            var maxScaleField = typeof(SpeedHackDetector).GetField("_maxAllowedTimeScale", BindingFlags.NonPublic | BindingFlags.Instance);
            maxScaleField.SetValue(detector, 3f);

            bool cheatingDetected = false;
            detector.AddCheatingDetectedListener(() => cheatingDetected = true);

            yield return null; // Start()

            Time.timeScale = 2f; // Below threshold
            yield return new WaitForSecondsRealtime(0.2f);

            Assert.IsFalse(cheatingDetected, "Should not trigger when timeScale is below max.");
            Assert.IsTrue(detector.IsRunning);
        }

        [UnityTest]
        public IEnumerator TimeScaleDetectionDisabled_DoesNotTrigger()
        {
            _go = new GameObject("SpeedHackTest_TimeScale");
            var detector = CreateDetectorWithFastCheck();

            var detectField = typeof(SpeedHackDetector).GetField("_detectTimeScaleManipulation", BindingFlags.NonPublic | BindingFlags.Instance);
            detectField.SetValue(detector, false);

            var maxScaleField = typeof(SpeedHackDetector).GetField("_maxAllowedTimeScale", BindingFlags.NonPublic | BindingFlags.Instance);
            maxScaleField.SetValue(detector, 3f);

            bool cheatingDetected = false;
            detector.AddCheatingDetectedListener(() => cheatingDetected = true);

            yield return null; // Start()

            Time.timeScale = 10f; // Way above threshold, but detection disabled
            yield return new WaitForSecondsRealtime(0.2f);

            Assert.IsFalse(cheatingDetected, "Should not trigger when timeScale detection is disabled.");
            Assert.IsTrue(detector.IsRunning);
        }
    }
}
