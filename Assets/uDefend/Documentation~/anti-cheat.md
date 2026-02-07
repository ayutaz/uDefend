# Anti-Cheat Module

## Overview

The anti-cheat module provides:

- **ObscuredTypes** (19 types) — Memory-level protection against value editing
- **Detectors** (6 types) — Runtime cheat detection
- **ObscuredPrefs / ObscuredFilePrefs** — Encrypted PlayerPrefs replacement

The module is fully independent from the Runtime module (`uDefend.AntiCheat.asmdef` has no references to `uDefend.Runtime.asmdef`).

## ObscuredTypes

### Supported Types (19)

| Category | Types |
|----------|-------|
| Integer | `ObscuredInt`, `ObscuredLong`, `ObscuredByte`, `ObscuredSbyte`, `ObscuredShort`, `ObscuredUshort`, `ObscuredUint`, `ObscuredUlong` |
| Floating | `ObscuredFloat`, `ObscuredDouble`, `ObscuredDecimal` |
| Other | `ObscuredBool`, `ObscuredString`, `ObscuredChar` |
| Unity | `ObscuredVector2`, `ObscuredVector3`, `ObscuredVector2Int`, `ObscuredVector3Int`, `ObscuredQuaternion` |

### Usage

ObscuredTypes are drop-in replacements for standard types. Implicit conversion operators make them transparent:

```csharp
// Replace standard type declarations
ObscuredInt health = 100;
ObscuredFloat speed = 5.5f;
ObscuredString name = "Player";

// Use normally — implicit conversion handles everything
health -= 25;
float adjustedSpeed = speed * 1.5f;
Debug.Log($"Name: {(string)name}");
```

### How It Works

Each ObscuredType stores values XOR-encrypted in memory with a per-instance random key:

```
Stored Value = Real Value XOR Random Key
```

Additionally, a checksum is computed and verified on every read:

```
Checksum = Real Value XOR Type-Specific Salt
```

If the stored encrypted value, key, or checksum has been tampered with by a memory editor, the checksum verification fails and the `OnCheatingDetected` event fires.

### Cheating Detection

```csharp
ObscuredInt.OnCheatingDetected += () => {
    Debug.LogWarning("Memory tampering detected!");
};
```

### Inspector Support

All 19 ObscuredTypes have custom PropertyDrawers. Values display as their decrypted form in the Inspector and can be edited normally.

## Detectors

All detectors inherit from `DetectorBase` and share common settings:

| Property | Default | Description |
|----------|---------|-------------|
| Check Interval | 1.0s | Seconds between checks |
| Auto Start | true | Start detection on Enable |
| Don't Destroy On Load | true | Persist across scene loads |
| On Cheating Detected | — | UnityEvent callback |

### SpeedHackDetector

Detects speed manipulation tools (e.g., Cheat Engine speedhack) by comparing `System.Diagnostics.Stopwatch` against `Time.realtimeSinceStartup`.

| Property | Default | Description |
|----------|---------|-------------|
| Max Time Difference Threshold | 1.0s | Wall-clock vs Unity-clock 最大許容差 |
| Detect TimeScale Manipulation | true | Time.timeScale の異常検知を有効化 |
| Max Allowed TimeScale | 3.0 | 検知を発火する timeScale 閾値（厳密に超えた場合のみ） |

### TimeCheatingDetector

Detects system clock manipulation (regression or large jumps in `DateTime.UtcNow`).

### WallHackDetector

Detects teleportation/noclip by placing a sandbox `BoxCollider` and verifying that raycasts behave correctly.

### InjectionDetector

Detects DLL injection by comparing loaded assemblies against a whitelist captured at startup.

### ObscuredCheatingDetector

Subscribes to all 19 ObscuredType `OnCheatingDetected` events and fires when any type reports tampering.

### AntiDebugDetector

Detects debugging tools and environments that may be used to reverse-engineer or tamper with the game at runtime.

| Property | Default | Description |
|----------|---------|-------------|
| Detect Managed Debugger | true | Checks `Debugger.IsAttached` |
| Detect Debug Environment Variables | true | Checks for `ENABLE_MONO_DEBUG`, `DNSPY_DEBUGGING`, etc. |
| Detect Suspicious Processes | false | Scans for Cheat Engine, IDA, dnSpy, etc. (Windows only, opt-in) |
| Detect Breakpoint Timing | true | Detects frame time anomalies caused by breakpoints |
| Frame Time Threshold | 10s | Seconds before a frame pause triggers detection |

### Setup

1. Add a Detector component to a GameObject (or create via menu)
2. Configure the `OnCheatingDetected` callback in the Inspector
3. The detector starts automatically by default

```csharp
// Or start/stop programmatically:
var detector = GetComponent<SpeedHackDetector>();
detector.StartDetection();
detector.StopDetection();
```

## ObscuredPrefs

Encrypted wrapper for Unity's `PlayerPrefs`:

```csharp
ObscuredPrefs.SetInt("Score", 1000);
int score = ObscuredPrefs.GetInt("Score", 0);

ObscuredPrefs.SetFloat("Volume", 0.8f);
ObscuredPrefs.SetString("Name", "Player");

ObscuredPrefs.HasKey("Score");
ObscuredPrefs.DeleteKey("Score");
ObscuredPrefs.DeleteAll();
```

## ObscuredFilePrefs

File-based encrypted preferences for larger datasets:

```csharp
ObscuredFilePrefs.SetInt("PlayTime", 3600);
int time = ObscuredFilePrefs.GetInt("PlayTime", 0);

ObscuredFilePrefs.Save();  // Flush to disk
```
