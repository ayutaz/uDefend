# uDefend

**Save data encryption and anti-cheat library for Unity.**

uDefend provides enterprise-grade security for your Unity game's save data and runtime memory, with zero-config defaults that follow OWASP best practices.

## Features

### Encrypted Save/Load
- **AES-256-CBC + HMAC-SHA256** (Encrypt-then-MAC) authenticated encryption
- Atomic file writes for crash safety
- Per-slot thread-safe concurrent access
- Automatic backup and rotation
- Version-based save data migration

### Platform Key Management
- **Windows**: DPAPI (user-scoped)
- **Android**: Keystore (hardware-backed TEE/StrongBox)
- **iOS**: Keychain (Secure Enclave)
- **Fallback**: PBKDF2 (SHA-256, 600,000 iterations)

### Anti-Cheat (Independent Module)
- **19 ObscuredTypes**: Memory-protected replacements for int, float, string, Vector3, etc.
- **5 Detectors**: SpeedHack, TimeCheating, WallHack, Injection, ObscuredCheating
- **ObscuredPrefs**: Encrypted PlayerPrefs replacement
- Inspector support with custom PropertyDrawers for all 19 types

## Quick Start

```csharp
using uDefend.Core;
using uDefend.KeyManagement;

// Setup
IKeyProvider keyProvider = PlatformKeyProvider.Create("com.example.mygame");
var saveManager = new SaveManager(saveSettings, keyProvider);

// Save (serialize → encrypt → write atomically)
await saveManager.SaveAsync("slot1", playerData);

// Load (read → verify HMAC → decrypt → deserialize)
var data = await saveManager.LoadAsync<PlayerData>("slot1");
```

```csharp
using uDefend.AntiCheat;

// Drop-in memory protection
ObscuredInt health = 100;
health -= 25;  // Works just like a normal int
```

## Requirements

- Unity 2022.3.12f1 LTS or later
- .NET Standard 2.1
- IL2CPP or Mono scripting backend

## Installation

### OpenUPM
```
openupm add com.udefend.core
```

### Unity Package Manager
Add via git URL: `https://github.com/YOUR_ORG/uDefend.git?path=Assets/uDefend`

## Documentation

See the [Documentation](Documentation~/index.md) folder for detailed guides:

- [Getting Started](Documentation~/getting-started.md)
- [Save/Load API](Documentation~/save-load-api.md)
- [Anti-Cheat](Documentation~/anti-cheat.md)
- [Migration](Documentation~/migration.md)
- [API Reference](Documentation~/api-reference.md)

## Samples

Import samples via Unity Package Manager > uDefend > Samples:

| Sample | Description |
|--------|-------------|
| Basic Usage | Save/Load with default encryption |
| Advanced Encryption | Custom IEncryptionProvider, low-level API |
| Migration | Version-based save data migration |
| Anti-Cheat | ObscuredTypes and Detector setup |

## Architecture

```
uDefend/
├── Runtime/           # Core: Save/Load, Encryption, Keys, Migration
├── Runtime.AntiCheat/ # Independent: ObscuredTypes, Detectors, Prefs
├── Editor/            # PropertyDrawers, EditorWindows
└── Tests/             # 137+ unit tests
```

The AntiCheat module has **zero dependency** on the Runtime module — use either independently.

## Security Design

- **Secure by Default**: OWASP-recommended settings out of the box
- CSPRNG for all random data (IV, keys, salts)
- HMAC verification before decryption (padding oracle prevention)
- Constant-time HMAC comparison (timing attack prevention)
- Sensitive buffers zeroed after use
- No deprecated APIs (`RijndaelManaged`, `BinaryFormatter`, `DES`, `MD5`)

## License

MIT License. See [LICENSE](LICENSE) for details.
