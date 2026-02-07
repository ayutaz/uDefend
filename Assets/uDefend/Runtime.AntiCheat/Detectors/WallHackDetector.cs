using UnityEngine;

namespace uDefend.AntiCheat
{
    /// <summary>
    /// Detects wall hacks by creating a hidden physics sandbox.
    /// A Rigidbody is placed behind a collider and a raycast is performed to verify
    /// that the collider blocks the ray as expected. If the raycast passes through,
    /// wall hack (collision disabling) is detected.
    /// </summary>
    [AddComponentMenu("uDefend/Detectors/Wall Hack Detector")]
    [DisallowMultipleComponent]
    public sealed class WallHackDetector : DetectorBase
    {
        [Tooltip("Physics layer used for the hidden detection sandbox. " +
                 "Choose a dedicated layer that does not collide with game objects.")]
        [SerializeField] private int _detectionLayer = 31;

        [Tooltip("Position offset for the sandbox objects (far from the playable area).")]
        [SerializeField] private Vector3 _sandboxOffset = new Vector3(0f, -1000f, 0f);

        private GameObject _wallObject;
        private GameObject _probeObject;
        private bool _sandboxReady;

        protected override void OnDetectionStarted()
        {
            CreateSandbox();
        }

        protected override void OnDetectionStopped()
        {
            DestroySandbox();
        }

        protected override bool CheckForCheat()
        {
            if (!_sandboxReady || _wallObject == null || _probeObject == null)
            {
                // Sandbox was destroyed externally; recreate it.
                CreateSandbox();
                return false;
            }

            // The probe is behind the wall. A raycast from the probe toward the wall
            // should hit the wall's collider. If it doesn't, physics collisions have
            // been tampered with (wall hack).
            Vector3 origin = _probeObject.transform.position;
            Vector3 direction = (_wallObject.transform.position - origin).normalized;
            float distance = Vector3.Distance(origin, _wallObject.transform.position) + 1f;

            int layerMask = 1 << _detectionLayer;
            bool hitWall = Physics.Raycast(origin, direction, out _, distance, layerMask);

            return !hitWall;
        }

        private void CreateSandbox()
        {
            DestroySandbox();

            // --- Wall (collider) ---
            _wallObject = new GameObject("__uDefend_WH_Wall")
            {
                layer = _detectionLayer,
                hideFlags = HideFlags.HideAndDontSave
            };
            _wallObject.transform.position = transform.position + _sandboxOffset;

            var boxCollider = _wallObject.AddComponent<BoxCollider>();
            boxCollider.size = new Vector3(10f, 10f, 1f);

            // --- Probe (the "player" behind the wall) ---
            _probeObject = new GameObject("__uDefend_WH_Probe")
            {
                layer = _detectionLayer,
                hideFlags = HideFlags.HideAndDontSave
            };
            _probeObject.transform.position = _wallObject.transform.position + new Vector3(0f, 0f, -5f);

            var rb = _probeObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;

            _sandboxReady = true;
        }

        private void DestroySandbox()
        {
            _sandboxReady = false;

            if (_wallObject != null)
            {
                Destroy(_wallObject);
                _wallObject = null;
            }

            if (_probeObject != null)
            {
                Destroy(_probeObject);
                _probeObject = null;
            }
        }

        protected override void OnDestroy()
        {
            DestroySandbox();
            base.OnDestroy();
        }
    }
}
