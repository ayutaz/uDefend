using System;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace uDefend.AntiCheat
{
    [AddComponentMenu("uDefend/Detectors/Anti-Debug Detector")]
    [DisallowMultipleComponent]
    public sealed class AntiDebugDetector : DetectorBase
    {
        [Tooltip("Detect managed debugger (Debugger.IsAttached).")]
        [SerializeField] private bool _detectManagedDebugger = true;

        [Tooltip("Detect debug-related environment variables (e.g., ENABLE_MONO_DEBUG).")]
        [SerializeField] private bool _detectDebugEnvironmentVariables = true;

        [Tooltip("Detect suspicious processes (Cheat Engine, etc.). Windows only. Opt-in.")]
        [SerializeField] private bool _detectSuspiciousProcesses = true;

        [Tooltip("Process names to check (case-insensitive, partial match).")]
        [SerializeField] private string[] _suspiciousProcessNames = new[]
        {
            "cheatengine", "usagimimi", "artmoney", "gameguardian",
            "ollydbg", "x64dbg", "x32dbg", "ida", "ida64",
            "dnspy", "de4dot", "ilspy", "dotpeek"
        };

        [Tooltip("Detect breakpoints via frame time anomaly.")]
        [SerializeField] private bool _detectBreakpointTiming = true;

        [Tooltip("Frame time threshold in seconds before triggering timing detection.")]
        [SerializeField] private float _frameTimeThresholdSeconds = 3f;

        private static readonly string[] DebugEnvironmentVariables =
        {
            "ENABLE_MONO_DEBUG",
            "MONO_ENV_OPTIONS",
            "DNSPY_DEBUGGING",
            "COR_ENABLE_PROFILING",
            "CORECLR_ENABLE_PROFILING"
        };

        private Stopwatch _frameStopwatch;

        protected override void OnDetectionStarted()
        {
            _frameStopwatch = Stopwatch.StartNew();
        }

        protected override void OnDetectionStopped()
        {
            _frameStopwatch?.Stop();
            _frameStopwatch = null;
        }

        protected override bool CheckForCheat()
        {
            if (_detectManagedDebugger && Debugger.IsAttached)
            {
                return true;
            }

            if (_detectDebugEnvironmentVariables && CheckDebugEnvironmentVariables())
            {
                return true;
            }

#if UNITY_STANDALONE_WIN
            if (_detectSuspiciousProcesses && CheckSuspiciousProcesses())
            {
                return true;
            }
#endif

            if (_detectBreakpointTiming && CheckBreakpointTiming())
            {
                return true;
            }

            return false;
        }

        private bool CheckDebugEnvironmentVariables()
        {
            for (int i = 0; i < DebugEnvironmentVariables.Length; i++)
            {
                string value = Environment.GetEnvironmentVariable(DebugEnvironmentVariables[i]);
                if (!string.IsNullOrEmpty(value))
                {
                    return true;
                }
            }
            return false;
        }

#if UNITY_STANDALONE_WIN
        private bool CheckSuspiciousProcesses()
        {
            if (_suspiciousProcessNames == null || _suspiciousProcessNames.Length == 0)
                return false;

            try
            {
                var processes = System.Diagnostics.Process.GetProcesses();
                for (int i = 0; i < processes.Length; i++)
                {
                    try
                    {
                        string name = processes[i].ProcessName.ToLowerInvariant();
                        for (int j = 0; j < _suspiciousProcessNames.Length; j++)
                        {
                            if (name.Contains(_suspiciousProcessNames[j].ToLowerInvariant()))
                            {
                                return true;
                            }
                        }
                    }
                    catch
                    {
                        // Access denied for some system processes — skip
                    }
                    finally
                    {
                        processes[i].Dispose();
                    }
                }
            }
            catch
            {
                // GetProcesses may throw on some platforms — ignore
            }
            return false;
        }
#endif

        private bool CheckBreakpointTiming()
        {
            if (_frameStopwatch == null)
            {
                _frameStopwatch = Stopwatch.StartNew();
                return false;
            }

            double elapsed = _frameStopwatch.Elapsed.TotalSeconds;
            _frameStopwatch.Restart();

            return elapsed > _frameTimeThresholdSeconds;
        }
    }
}
