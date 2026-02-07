# Getting Started

## Requirements

- Unity 2022.3.12f1 LTS or later (Unity 6 supported)
- .NET Standard 2.1
- IL2CPP or Mono scripting backend

## Installation

### Via OpenUPM (Recommended)

```
openupm add com.udefend.core
```

### Via Unity Package Manager

1. Open **Window > Package Manager**
2. Click **+** > **Add package from git URL...**
3. Enter: `https://github.com/ayutaz/uDefend.git?path=Assets/uDefend`

### Via .unitypackage

Download the latest `.unitypackage` from the [Releases](https://github.com/ayutaz/uDefend/releases) page and import it into your project.

## Quick Start

### 1. Create Save Settings

Right-click in the Project window and select **Create > uDefend > Save Settings** to create a `SaveSettings` ScriptableObject.

### 2. Basic Save/Load

```csharp
using uDefend.Core;
using uDefend.KeyManagement;

// Create key provider (auto-selects platform-appropriate secure storage)
IKeyProvider keyProvider = PlatformKeyProvider.Create("com.example.mygame");

// Create SaveManager
var saveManager = new SaveManager(saveSettings, keyProvider);

// Save
await saveManager.SaveAsync("slot1", myData);

// Load
var data = await saveManager.LoadAsync<MyData>("slot1");
```

### 3. Anti-Cheat Protection

Replace standard types with ObscuredTypes for memory protection:

```csharp
using uDefend.AntiCheat;

// Before:
private int health = 100;

// After (memory-protected):
private ObscuredInt health = 100;
```

Add Detectors to your scene for runtime cheat detection:

1. Create an empty GameObject
2. Add `SpeedHackDetector`, `ObscuredCheatingDetector`, `AntiDebugDetector`, etc.
3. Configure callbacks in the Inspector

## Architecture

uDefend consists of two independent modules:

- **uDefend.Runtime** — Save/Load, encryption, key management, migration
- **uDefend.AntiCheat** — ObscuredTypes, Detectors, ObscuredPrefs

The AntiCheat module has **zero dependency** on the Runtime module, so you can use either independently.

## Platform Support

| Platform | Key Storage | Status |
|----------|------------|--------|
| Windows | DPAPI | Fully supported |
| Android | Keystore (TEE/StrongBox) | Fully supported |
| iOS | Keychain (Secure Enclave) | Fully supported |
| macOS/Linux/WebGL | PBKDF2 fallback | Best effort |
