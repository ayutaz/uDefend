using System;
using UnityEngine;

namespace uDefend.AntiCheat
{
    /// <summary>
    /// Detects system clock manipulation by comparing the progression of DateTime.UtcNow
    /// against Time.realtimeSinceStartup. If the system clock jumps backward or leaps
    /// forward far beyond what realtimeSinceStartup indicates, tampering is suspected.
    /// </summary>
    [AddComponentMenu("uDefend/Detectors/Time Cheating Detector")]
    [DisallowMultipleComponent]
    public sealed class TimeCheatingDetector : DetectorBase
    {
        [Tooltip("Maximum acceptable forward time jump in seconds between the system clock and Unity's real time. " +
                 "If the system clock advances more than this beyond what Unity's real time indicates, cheating is detected.")]
        [SerializeField] private float _maxTimeJumpSeconds = 60f;

        [Tooltip("Whether to detect the system clock moving backward (always suspicious).")]
        [SerializeField] private bool _detectBackwardTimeTravel = true;

        private DateTime _lastUtcTime;
        private float _lastRealtimeSinceStartup;

        protected override void OnDetectionStarted()
        {
            ResetBaseline();
        }

        protected override bool CheckForCheat()
        {
            DateTime currentUtcTime = DateTime.UtcNow;
            float currentRealtime = Time.realtimeSinceStartup;

            double systemElapsedSeconds = (currentUtcTime - _lastUtcTime).TotalSeconds;
            double unityElapsedSeconds = currentRealtime - _lastRealtimeSinceStartup;

            _lastUtcTime = currentUtcTime;
            _lastRealtimeSinceStartup = currentRealtime;

            // Detect backward time travel: system clock moved backward while Unity time moved forward.
            if (_detectBackwardTimeTravel && systemElapsedSeconds < -1.0)
            {
                return true;
            }

            // Detect forward time jump: system clock advanced far more than Unity's real time.
            // We allow a tolerance of _maxTimeJumpSeconds for legitimate cases (e.g., device sleep/resume).
            double drift = systemElapsedSeconds - unityElapsedSeconds;
            if (drift > _maxTimeJumpSeconds)
            {
                return true;
            }

            return false;
        }

        private void ResetBaseline()
        {
            _lastUtcTime = DateTime.UtcNow;
            _lastRealtimeSinceStartup = Time.realtimeSinceStartup;
        }
    }
}
