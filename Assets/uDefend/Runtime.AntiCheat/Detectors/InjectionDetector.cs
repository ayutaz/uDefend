using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace uDefend.AntiCheat
{
    /// <summary>
    /// Detects DLL injection by comparing the currently loaded assemblies against a
    /// baseline snapshot captured when detection starts. Any assembly that appears
    /// after initialization and was not in the baseline is treated as a potential injection.
    /// </summary>
    /// <remarks>
    /// This is a simplified heuristic detector. Sophisticated attackers may load code
    /// into existing assemblies via reflection or native hooks. For production use,
    /// consider supplementing this with code signing verification and native integrity checks.
    /// </remarks>
    [AddComponentMenu("uDefend/Detectors/Injection Detector")]
    [DisallowMultipleComponent]
    public sealed class InjectionDetector : DetectorBase
    {
        [Tooltip("Additional assembly names to whitelist beyond those captured at startup. " +
                 "Use this for assemblies known to be loaded lazily (e.g., by plugins or asset bundles).")]
        [SerializeField] private string[] _additionalWhitelist = Array.Empty<string>();

        private HashSet<string> _baselineAssemblies;

        protected override void OnDetectionStarted()
        {
            CaptureBaseline();
        }

        protected override bool CheckForCheat()
        {
            if (_baselineAssemblies == null)
            {
                CaptureBaseline();
                return false;
            }

            Assembly[] currentAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            for (int i = 0; i < currentAssemblies.Length; i++)
            {
                string assemblyName = currentAssemblies[i].GetName().Name;
                if (!_baselineAssemblies.Contains(assemblyName))
                {
                    Debug.LogWarning(
                        $"[uDefend] InjectionDetector: Unknown assembly detected - \"{assemblyName}\". " +
                        "This may indicate DLL injection.");
                    return true;
                }
            }

            return false;
        }

        private void CaptureBaseline()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            _baselineAssemblies = new HashSet<string>(assemblies.Length + _additionalWhitelist.Length);

            for (int i = 0; i < assemblies.Length; i++)
            {
                _baselineAssemblies.Add(assemblies[i].GetName().Name);
            }

            for (int i = 0; i < _additionalWhitelist.Length; i++)
            {
                if (!string.IsNullOrEmpty(_additionalWhitelist[i]))
                {
                    _baselineAssemblies.Add(_additionalWhitelist[i]);
                }
            }
        }
    }
}
