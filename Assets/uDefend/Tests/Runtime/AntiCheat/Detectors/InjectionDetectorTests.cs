using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using uDefend.AntiCheat;

namespace uDefend.Tests.AntiCheat.Detectors
{
    [TestFixture]
    public class InjectionDetectorTests
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
            _go = new GameObject("InjectionTest");
            var detector = _go.AddComponent<InjectionDetector>();

            bool cheatingDetected = false;
            detector.AddCheatingDetectedListener(() => cheatingDetected = true);

            yield return null;
            Assert.IsTrue(detector.IsRunning);

            // Wait for several check intervals
            yield return new WaitForSeconds(0.5f);

            Assert.IsFalse(cheatingDetected, "InjectionDetector should not trigger when no new assemblies are loaded.");
            Assert.IsTrue(detector.IsRunning, "Detector should still be running.");
        }

        [UnityTest]
        public IEnumerator OnCheatingDetected_ListenerRegistration_Works()
        {
            _go = new GameObject("InjectionTest");
            var detector = _go.AddComponent<InjectionDetector>();

            int callCount = 0;
            detector.AddCheatingDetectedListener(() => callCount++);

            yield return null;

            yield return new WaitForSeconds(0.3f);
            Assert.AreEqual(0, callCount, "Listener should not be called under normal conditions.");
        }

        [UnityTest]
        public IEnumerator StopDetection_PreventsChecks()
        {
            _go = new GameObject("InjectionTest");
            var detector = _go.AddComponent<InjectionDetector>();

            bool cheatingDetected = false;
            detector.AddCheatingDetectedListener(() => cheatingDetected = true);

            yield return null;
            detector.StopDetection();
            Assert.IsFalse(detector.IsRunning);

            yield return new WaitForSeconds(0.3f);
            Assert.IsFalse(cheatingDetected, "Stopped detector should not fire events.");
        }
    }
}
