# uDefend

**English** | [日本語](README.md)

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](Assets/uDefend/LICENSE)
[![Unity](https://img.shields.io/badge/Unity-2022.3%2B-black.svg)](https://unity.com/)

**Save Data Encryption & Anti-Cheat Library for Unity**

uDefend is a Unity package that encrypts game save data with AES-256 and protects in-memory values against cheating. OWASP-recommended security settings are applied out of the box with zero configuration.

## Key Features

### Encrypted Save/Load
- **AES-256-CBC + HMAC-SHA256** (Encrypt-then-MAC) authenticated encryption
- Atomic file writes (crash resilient)
- Per-slot thread-safe mutual exclusion
- Automatic backup & rotation
- Version-based save data migration

### Platform Key Management
| Platform | Key Storage | Notes |
|:---:|:---:|---|
| Windows | DPAPI | User scope |
| Android | Keystore | TEE/StrongBox hardware-backed |
| iOS | Keychain | Secure Enclave support |
| Others | PBKDF2 fallback | SHA-256, 600,000 iterations |

### Anti-Cheat (Independent Module)
- **ObscuredTypes (19 types)**: Memory-protected versions of int, float, string, Vector3, etc.
- **Detectors (6 types)**: SpeedHack, TimeCheating, WallHack, Injection, ObscuredCheating, AntiDebug
- **ObscuredPrefs / ObscuredFilePrefs**: Encrypted PlayerPrefs alternative
- Custom PropertyDrawer for all 19 types in the Inspector

## Quick Start

### Save/Load

```csharp
using uDefend.Core;
using uDefend.KeyManagement;

// Automatically selects key provider for the current platform
IKeyProvider keyProvider = PlatformKeyProvider.Create("com.example.mygame");
var saveManager = new SaveManager(saveSettings, keyProvider);

// Save (serialize -> encrypt -> atomic write)
await saveManager.SaveAsync("slot1", playerData);

// Load (read -> HMAC verify -> decrypt -> deserialize)
var data = await saveManager.LoadAsync<PlayerData>("slot1");
```

### Memory Protection

```csharp
using uDefend.AntiCheat;

// Drop-in replacement for standard types
ObscuredInt health = 100;
health -= 25;  // Works just like a regular int

// Tampering detection
ObscuredInt.OnCheatingDetected += () => Debug.LogWarning("Cheat detected!");
```

## Requirements

- Unity 2022.3.12f1 LTS or later (Unity 6 compatible)
- .NET Standard 2.1
- IL2CPP / Mono supported

## Installation

### Unity Package Manager (Git URL)

1. Open **Window > Package Manager**
2. Click **+** > **Add package from git URL...**
3. Enter the following:

```
https://github.com/ayutaz/uDefend.git?path=Assets/uDefend
```

### .unitypackage

Download the latest `.unitypackage` from the [Releases](https://github.com/ayutaz/uDefend/releases) page and import it.

## Architecture

```
Assets/uDefend/
├── Runtime/                    # Core: Save/Load, Encryption, Key Management, Migration
│   ├── Core/                   # SaveManager, SaveFile, SaveSlot, SaveSettings
│   ├── Encryption/             # AES-CBC-HMAC, CryptoUtility, StringEncryption
│   ├── KeyManagement/          # DPAPI, Android Keystore, iOS Keychain, PBKDF2
│   ├── Serialization/          # JSON, MessagePack
│   ├── Migration/              # IMigration, MigrationRunner
│   └── Plugins/                # Native plugins (Android/iOS/Windows)
├── Runtime.AntiCheat/          # Independent module: ObscuredTypes, Detectors, Prefs
│   ├── ObscuredTypes/          # 19 types (Int, Float, Vector3, Quaternion, etc.)
│   ├── Detectors/              # 6 types (SpeedHack, WallHack, AntiDebug, etc.)
│   ├── ObscuredPrefs.cs
│   └── ObscuredFilePrefs.cs
├── Editor/                     # PropertyDrawer (19 types), EditorWindow (2 types)
├── Tests/                      # Unit tests
├── Samples~/                   # Sample scenes
└── Documentation~/             # Documentation
```

**The AntiCheat module has zero dependency on Runtime** — either module can be used independently.

## Security Design

- **Secure by Default**: OWASP-recommended settings with zero configuration
- CSPRNG-based IV / key / salt generation (`RandomNumberGenerator.Fill`)
- HMAC verification before decryption (padding oracle prevention)
- Constant-time HMAC comparison (timing attack prevention)
- Post-use buffer zeroing (`Array.Clear`)
- No deprecated APIs (`RijndaelManaged`, `BinaryFormatter`, `DES`, `MD5`)

## Documentation

See [`Assets/uDefend/Documentation~/`](Assets/uDefend/Documentation~/index.md) for details:

- [Getting Started](Assets/uDefend/Documentation~/getting-started.md) — Setup guide
- [Save/Load API](Assets/uDefend/Documentation~/save-load-api.md) — Save/Load API reference
- [Anti-Cheat](Assets/uDefend/Documentation~/anti-cheat.md) — ObscuredTypes & Detectors
- [Migration](Assets/uDefend/Documentation~/migration.md) — Save data migration
- [API Reference](Assets/uDefend/Documentation~/api-reference.md) — Full API reference

## Samples

Import from Unity Package Manager > uDefend > Samples:

| Sample | Description |
|:---:|---|
| Basic Usage | Basic save/load operations |
| Advanced Encryption | Low-level encryption API, custom IEncryptionProvider |
| Migration | Version-based save data migration |
| Anti-Cheat | ObscuredTypes and cheat detection Detector setup |

## License

MIT License — [LICENSE](Assets/uDefend/LICENSE)
