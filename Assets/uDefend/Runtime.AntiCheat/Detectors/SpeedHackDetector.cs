using System.Diagnostics;
using UnityEngine;

namespace uDefend.AntiCheat
{
    /// <summary>
    /// Detects speed hacks by comparing wall-clock time (System.Diagnostics.Stopwatch)
    /// against Unity's Time.realtimeSinceStartup. If the two clocks diverge beyond
    /// the configured threshold, a speed hack is likely in use.
    /// </summary>
    [AddComponentMenu("uDefend/Detectors/Speed Hack Detector")]
    [DisallowMultipleComponent]
    public sealed class SpeedHackDetector : DetectorBase
    {
        [Tooltip("Maximum acceptable time difference in seconds between the system clock and Unity clock before triggering detection.")]
        [SerializeField] private float _maxTimeDifferenceThreshold = 1f;

        [Tooltip("Detect Time.timeScale manipulation beyond the allowed maximum.")]
        [SerializeField] private bool _detectTimeScaleManipulation = true;

        [Tooltip("Maximum allowed Time.timeScale before triggering detection.")]
        [SerializeField] private float _maxAllowedTimeScale = 3f;

        private Stopwatch _stopwatch;
        private float _lastUnityTime;

        protected override void OnDetectionStarted()
        {
            ResetBaseline();
        }

        protected override void OnDetectionStopped()
        {
            _stopwatch?.Stop();
            _stopwatch = null;
        }

        protected override bool CheckForCheat()
        {
            if (_detectTimeScaleManipulation && Time.timeScale > _maxAllowedTimeScale)
            {
                return true;
            }

            if (_stopwatch == null)
            {
                ResetBaseline();
                return false;
            }

            float currentUnityTime = Time.realtimeSinceStartup;
            double stopwatchElapsed = _stopwatch.Elapsed.TotalSeconds;

            float unityElapsed = currentUnityTime - _lastUnityTime;
            double systemElapsed = stopwatchElapsed;

            double difference = System.Math.Abs(systemElapsed - unityElapsed);

            // Reset baseline after each check to avoid accumulating small drifts
            // that are unrelated to cheating.
            ResetBaseline();

            return difference > _maxTimeDifferenceThreshold;
        }

        private void ResetBaseline()
        {
            _lastUnityTime = Time.realtimeSinceStartup;
            _stopwatch?.Stop();
            _stopwatch = Stopwatch.StartNew();
        }
    }
}
