using UnityEngine;

namespace uDefend.AntiCheat
{
    /// <summary>
    /// Detects memory tampering of ObscuredTypes (ObscuredInt, ObscuredFloat, ObscuredBool, ObscuredString).
    /// Subscribes to each type's static OnCheatingDetected event and triggers when any of them fires.
    /// </summary>
    [AddComponentMenu("uDefend/Detectors/Obscured Cheating Detector")]
    [DisallowMultipleComponent]
    public sealed class ObscuredCheatingDetector : DetectorBase
    {
        private volatile bool _cheatingDetected;

        protected override void OnDetectionStarted()
        {
            _cheatingDetected = false;

            ObscuredInt.OnCheatingDetected += OnObscuredTypeCheating;
            ObscuredFloat.OnCheatingDetected += OnObscuredTypeCheating;
            ObscuredBool.OnCheatingDetected += OnObscuredTypeCheating;
            ObscuredString.OnCheatingDetected += OnObscuredTypeCheating;
        }

        protected override void OnDetectionStopped()
        {
            ObscuredInt.OnCheatingDetected -= OnObscuredTypeCheating;
            ObscuredFloat.OnCheatingDetected -= OnObscuredTypeCheating;
            ObscuredBool.OnCheatingDetected -= OnObscuredTypeCheating;
            ObscuredString.OnCheatingDetected -= OnObscuredTypeCheating;
        }

        private void OnObscuredTypeCheating()
        {
            _cheatingDetected = true;
        }

        protected override bool CheckForCheat()
        {
            return _cheatingDetected;
        }
    }
}
