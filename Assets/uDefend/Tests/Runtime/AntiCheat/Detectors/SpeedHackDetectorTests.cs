using System.Collections;
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
    }
}
