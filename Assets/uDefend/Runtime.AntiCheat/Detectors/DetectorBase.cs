using System;
using UnityEngine;
using UnityEngine.Events;

namespace uDefend.AntiCheat
{
    /// <summary>
    /// Abstract base class for all cheat detectors.
    /// Provides a common framework for periodic cheat checking with Inspector-configurable settings.
    /// </summary>
    public abstract class DetectorBase : MonoBehaviour
    {
        [Tooltip("Interval in seconds between cheat checks.")]
        [SerializeField] private float _checkInterval = 1f;

        [Tooltip("Automatically start detection when the component is enabled.")]
        [SerializeField] private bool _autoStart = true;

        [Tooltip("Persist this detector across scene loads.")]
        [SerializeField] private bool _dontDestroyOnLoad = true;

        [Tooltip("Invoked when cheating is detected. Configure callbacks in the Inspector or via code.")]
        public UnityEvent OnCheatingDetected;

        /// <summary>
        /// Whether the detector is currently running periodic checks.
        /// </summary>
        public bool IsRunning { get; private set; }

        protected virtual void Start()
        {
            if (_dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }

            if (_autoStart)
            {
                StartDetection();
            }
        }

        /// <summary>
        /// Begin periodic cheat detection. Does nothing if already running.
        /// </summary>
        public void StartDetection()
        {
            if (IsRunning) return;

            IsRunning = true;
            OnDetectionStarted();
            InvokeRepeating(nameof(PerformCheck), 0f, _checkInterval);
        }

        /// <summary>
        /// Stop periodic cheat detection.
        /// </summary>
        public void StopDetection()
        {
            if (!IsRunning) return;

            IsRunning = false;
            CancelInvoke(nameof(PerformCheck));
            OnDetectionStopped();
        }

        /// <summary>
        /// Called when detection starts. Override to perform initialization (e.g., subscribing to events).
        /// </summary>
        protected virtual void OnDetectionStarted() { }

        /// <summary>
        /// Called when detection stops. Override to perform cleanup (e.g., unsubscribing from events).
        /// </summary>
        protected virtual void OnDetectionStopped() { }

        private void PerformCheck()
        {
            if (CheckForCheat())
            {
                OnCheatingDetected?.Invoke();
                StopDetection();
            }
        }

        /// <summary>
        /// Implement this method to perform the actual cheat check.
        /// </summary>
        /// <returns>True if cheating was detected, false otherwise.</returns>
        protected abstract bool CheckForCheat();

        protected virtual void OnDestroy()
        {
            StopDetection();
        }
    }
}
