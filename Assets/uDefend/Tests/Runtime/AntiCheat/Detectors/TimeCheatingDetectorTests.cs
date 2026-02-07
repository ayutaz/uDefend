using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using uDefend.AntiCheat;

namespace uDefend.Tests.AntiCheat.Detectors
{
    [TestFixture]
    public class TimeCheatingDetectorTests
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
            _go = new GameObject("TimeCheatingTest");
            var detector = _go.AddComponent<TimeCheatingDetector>();

            bool cheatingDetected = false;
            detector.AddCheatingDetectedListener(() => cheatingDetected = true);

            yield return null;
            Assert.IsTrue(detector.IsRunning);

            // Wait several check intervals
            yield return new WaitForSeconds(0.5f);

            Assert.IsFalse(cheatingDetected, "TimeCheatingDetector should not trigger under normal conditions.");
            Assert.IsTrue(detector.IsRunning, "Detector should still be running.");
        }

        [UnityTest]
        public IEnumerator OnCheatingDetected_ListenerRegistration_Works()
        {
            _go = new GameObject("TimeCheatingTest");
            var detector = _go.AddComponent<TimeCheatingDetector>();

            int callCount = 0;
            detector.AddCheatingDetectedListener(() => callCount++);

            yield return null;

            yield return new WaitForSeconds(0.3f);
            Assert.AreEqual(0, callCount, "Listener should not be called under normal conditions.");
        }

        [UnityTest]
        public IEnumerator StopAndRestart_ResetsBaseline()
        {
            _go = new GameObject("TimeCheatingTest");
            var detector = _go.AddComponent<TimeCheatingDetector>();

            bool cheatingDetected = false;
            detector.AddCheatingDetectedListener(() => cheatingDetected = true);

            yield return null;
            Assert.IsTrue(detector.IsRunning);

            detector.StopDetection();
            Assert.IsFalse(detector.IsRunning);

            // Wait a moment then restart - should re-capture baseline
            yield return new WaitForSeconds(0.2f);
            detector.StartDetection();
            Assert.IsTrue(detector.IsRunning);

            yield return new WaitForSeconds(0.3f);
            Assert.IsFalse(cheatingDetected, "Restarted detector should not false-positive.");
        }
    }
}
