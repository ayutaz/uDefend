using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using uDefend.AntiCheat;

namespace uDefend.Tests.AntiCheat.Detectors
{
    /// <summary>
    /// Tests for DetectorBase common behavior, using SpeedHackDetector as a concrete implementation.
    /// </summary>
    [TestFixture]
    public class DetectorBaseTests
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
        public IEnumerator AutoStart_Default_StartsDetection()
        {
            _go = new GameObject("DetectorBaseTest");
            var detector = _go.AddComponent<SpeedHackDetector>();

            // Wait for Start() to execute
            yield return null;

            Assert.IsTrue(detector.IsRunning, "Detector should auto-start by default.");
        }

        [UnityTest]
        public IEnumerator StopDetection_SetsIsRunningFalse()
        {
            _go = new GameObject("DetectorBaseTest");
            var detector = _go.AddComponent<SpeedHackDetector>();

            yield return null;
            Assert.IsTrue(detector.IsRunning);

            detector.StopDetection();
            Assert.IsFalse(detector.IsRunning, "IsRunning should be false after StopDetection.");
        }

        [UnityTest]
        public IEnumerator StartDetection_WhenAlreadyRunning_DoesNothing()
        {
            _go = new GameObject("DetectorBaseTest");
            var detector = _go.AddComponent<SpeedHackDetector>();

            yield return null;
            Assert.IsTrue(detector.IsRunning);

            // Calling StartDetection again should not throw or cause issues
            detector.StartDetection();
            Assert.IsTrue(detector.IsRunning, "Detector should still be running after redundant StartDetection call.");
        }

        [UnityTest]
        public IEnumerator StopThenRestart_Works()
        {
            _go = new GameObject("DetectorBaseTest");
            var detector = _go.AddComponent<SpeedHackDetector>();

            yield return null;
            Assert.IsTrue(detector.IsRunning);

            detector.StopDetection();
            Assert.IsFalse(detector.IsRunning);

            detector.StartDetection();
            Assert.IsTrue(detector.IsRunning, "Detector should be running again after restart.");
        }

        [UnityTest]
        public IEnumerator OnCheatingDetected_EventCanBeSubscribed()
        {
            _go = new GameObject("DetectorBaseTest");
            var detector = _go.AddComponent<SpeedHackDetector>();

            bool eventFired = false;
            detector.AddCheatingDetectedListener(() => eventFired = true);

            yield return null;

            // Under normal conditions, the event should not fire
            yield return new WaitForSeconds(0.2f);
            Assert.IsFalse(eventFired, "OnCheatingDetected should not fire under normal conditions.");
        }

        [UnityTest]
        public IEnumerator Destroy_StopsDetection()
        {
            _go = new GameObject("DetectorBaseTest");
            var detector = _go.AddComponent<SpeedHackDetector>();

            yield return null;
            Assert.IsTrue(detector.IsRunning);

            Object.Destroy(_go);
            _go = null;
            yield return null;

            // After destruction, the detector object is null/destroyed
            Assert.IsTrue(detector == null, "Detector should be destroyed.");
        }
    }
}
