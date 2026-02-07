using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using uDefend.AntiCheat;

namespace uDefend.Tests.AntiCheat.Detectors
{
    [TestFixture]
    public class WallHackDetectorTests
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
        public IEnumerator SandboxCreation_CreatesHiddenObjects()
        {
            _go = new GameObject("WallHackTest");
            _go.AddComponent<WallHackDetector>();

            // Wait for Start() and OnDetectionStarted() to create sandbox
            yield return null;

            // Sandbox objects use HideFlags.HideAndDontSave, so GameObject.Find won't work.
            // Use Resources.FindObjectsOfTypeAll to find them.
            var allColliders = Resources.FindObjectsOfTypeAll<BoxCollider>();
            bool wallExists = allColliders.Any(c => c.gameObject.name == "__uDefend_WH_Wall");

            var allRigidbodies = Resources.FindObjectsOfTypeAll<Rigidbody>();
            bool probeExists = allRigidbodies.Any(r => r.gameObject.name == "__uDefend_WH_Probe");

            Assert.IsTrue(wallExists, "Wall sandbox object should exist after detection starts.");
            Assert.IsTrue(probeExists, "Probe sandbox object should exist after detection starts.");
        }

        [UnityTest]
        public IEnumerator NormalConditions_DoesNotTrigger()
        {
            _go = new GameObject("WallHackTest");
            var detector = _go.AddComponent<WallHackDetector>();

            bool cheatingDetected = false;
            detector.OnCheatingDetected = new UnityEngine.Events.UnityEvent();
            detector.OnCheatingDetected.AddListener(() => cheatingDetected = true);

            yield return null;
            Assert.IsTrue(detector.IsRunning);

            // Wait for physics to settle and checks to run
            yield return new WaitForSeconds(0.5f);

            Assert.IsFalse(cheatingDetected, "WallHackDetector should not trigger with normal physics.");
            Assert.IsTrue(detector.IsRunning, "Detector should still be running.");
        }

        [UnityTest]
        public IEnumerator Destroy_CleansSandbox()
        {
            _go = new GameObject("WallHackTest");
            _go.AddComponent<WallHackDetector>();

            yield return null;

            // Verify sandbox exists
            var wallsBefore = Resources.FindObjectsOfTypeAll<BoxCollider>()
                .Count(c => c.gameObject.name == "__uDefend_WH_Wall");
            Assert.IsTrue(wallsBefore > 0, "Sandbox should exist before destroy.");

            Object.Destroy(_go);
            _go = null;
            yield return null;

            // After destruction, sandbox objects should be cleaned up
            var wallsAfter = Resources.FindObjectsOfTypeAll<BoxCollider>()
                .Where(c => c != null && c.gameObject != null)
                .Count(c => c.gameObject.name == "__uDefend_WH_Wall");
            Assert.AreEqual(0, wallsAfter, "Sandbox wall should be cleaned up after destroy.");
        }

        [UnityTest]
        public IEnumerator StopDetection_CleansSandbox()
        {
            _go = new GameObject("WallHackTest");
            var detector = _go.AddComponent<WallHackDetector>();

            yield return null;
            Assert.IsTrue(detector.IsRunning);

            detector.StopDetection();
            yield return null;

            // Sandbox should be cleaned up when detection is stopped
            var walls = Resources.FindObjectsOfTypeAll<BoxCollider>()
                .Where(c => c != null && c.gameObject != null)
                .Count(c => c.gameObject.name == "__uDefend_WH_Wall");
            Assert.AreEqual(0, walls, "Sandbox should be cleaned up after StopDetection.");
        }
    }
}
