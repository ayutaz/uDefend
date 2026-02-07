using UnityEngine;

namespace uDefend.AntiCheat
{
    /// <summary>
    /// Detects memory tampering of ObscuredTypes.
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
            ObscuredLong.OnCheatingDetected += OnObscuredTypeCheating;
            ObscuredDouble.OnCheatingDetected += OnObscuredTypeCheating;
            ObscuredByte.OnCheatingDetected += OnObscuredTypeCheating;
            ObscuredSbyte.OnCheatingDetected += OnObscuredTypeCheating;
            ObscuredShort.OnCheatingDetected += OnObscuredTypeCheating;
            ObscuredUshort.OnCheatingDetected += OnObscuredTypeCheating;
            ObscuredUint.OnCheatingDetected += OnObscuredTypeCheating;
            ObscuredUlong.OnCheatingDetected += OnObscuredTypeCheating;
            ObscuredDecimal.OnCheatingDetected += OnObscuredTypeCheating;
            ObscuredChar.OnCheatingDetected += OnObscuredTypeCheating;
            ObscuredVector2.OnCheatingDetected += OnObscuredTypeCheating;
            ObscuredVector2Int.OnCheatingDetected += OnObscuredTypeCheating;
            ObscuredVector3.OnCheatingDetected += OnObscuredTypeCheating;
            ObscuredVector3Int.OnCheatingDetected += OnObscuredTypeCheating;
            ObscuredQuaternion.OnCheatingDetected += OnObscuredTypeCheating;
        }

        protected override void OnDetectionStopped()
        {
            ObscuredInt.OnCheatingDetected -= OnObscuredTypeCheating;
            ObscuredFloat.OnCheatingDetected -= OnObscuredTypeCheating;
            ObscuredBool.OnCheatingDetected -= OnObscuredTypeCheating;
            ObscuredString.OnCheatingDetected -= OnObscuredTypeCheating;
            ObscuredLong.OnCheatingDetected -= OnObscuredTypeCheating;
            ObscuredDouble.OnCheatingDetected -= OnObscuredTypeCheating;
            ObscuredByte.OnCheatingDetected -= OnObscuredTypeCheating;
            ObscuredSbyte.OnCheatingDetected -= OnObscuredTypeCheating;
            ObscuredShort.OnCheatingDetected -= OnObscuredTypeCheating;
            ObscuredUshort.OnCheatingDetected -= OnObscuredTypeCheating;
            ObscuredUint.OnCheatingDetected -= OnObscuredTypeCheating;
            ObscuredUlong.OnCheatingDetected -= OnObscuredTypeCheating;
            ObscuredDecimal.OnCheatingDetected -= OnObscuredTypeCheating;
            ObscuredChar.OnCheatingDetected -= OnObscuredTypeCheating;
            ObscuredVector2.OnCheatingDetected -= OnObscuredTypeCheating;
            ObscuredVector2Int.OnCheatingDetected -= OnObscuredTypeCheating;
            ObscuredVector3.OnCheatingDetected -= OnObscuredTypeCheating;
            ObscuredVector3Int.OnCheatingDetected -= OnObscuredTypeCheating;
            ObscuredQuaternion.OnCheatingDetected -= OnObscuredTypeCheating;
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
