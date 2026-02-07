# Save/Load API

## Overview

uDefend provides an encrypted Save/Load system with the following features:

- AES-256-CBC + HMAC-SHA256 (Encrypt-then-MAC) authenticated encryption
- Platform-native key management (DPAPI, Android Keystore, iOS Keychain)
- Atomic file writes (temp file + rename) for crash safety
- Per-slot locking for thread safety
- Automatic backup and rotation
- Version-based migration support

## SaveSettings

Create via **Create > uDefend > Save Settings** in the Project window.

| Property | Default | Description |
|----------|---------|-------------|
| Encryption | AesCbcHmac | Encryption algorithm (`AesCbcHmac`, `AesGcm`, `None`) |
| Serializer | Json | Serialization format (`Json`, `MessagePack`) |
| SavePath | (empty) | Custom save path. Empty uses `Application.persistentDataPath` |
| AutoBackup | false | Automatically create backup on each save |
| MaxBackupCount | 3 | Maximum number of backup files to keep |
| CurrentVersion | 1 | Current save file format version |
| MinSupportedVersion | 1 | Oldest version that can be migrated |

## SaveManager API

```csharp
var saveManager = new SaveManager(settings, keyProvider);
var saveManager = new SaveManager(settings, keyProvider, migrationRunner);
```

### Save

```csharp
await saveManager.SaveAsync<T>("slotId", data);
```

Serializes → Encrypts → Writes atomically (`.tmp` → rename).

### Load

```csharp
T data = await saveManager.LoadAsync<T>("slotId");
```

Reads → HMAC verification → Decrypts → Migrates (if needed) → Deserializes.

### Other Operations

```csharp
bool exists = saveManager.Exists("slotId");
saveManager.Delete("slotId");
string[] slots = saveManager.GetAllSlotIds();
```

## Save File Format

```
| Magic "uSAV" (4B) | Version (2B) | Flags (2B) | IV (16B) | Encrypted Data (var) | HMAC-SHA256 (32B) |
```

- **Magic Number**: `uSAV` — identifies the file type
- **Version**: uint16 — save format version for migration
- **Flags**: Reserved for future use (compression, etc.)
- **IV**: 16-byte random IV (regenerated every save)
- **Encrypted Data**: AES-256-CBC encrypted payload
- **HMAC**: SHA-256 HMAC over the entire preceding data

## Key Management

### PlatformKeyProvider (Recommended)

```csharp
// Auto-selects the best available provider for the current platform
IKeyProvider provider = PlatformKeyProvider.Create("com.example.mygame");
```

### Pbkdf2KeyProvider

```csharp
byte[] passphrase = Encoding.UTF8.GetBytes("user-password");
using var provider = new Pbkdf2KeyProvider(passphrase);
// SHA-256, 600,000 iterations (OWASP 2024 recommended)
```

### Envelope Encryption

```csharp
// Master key protects a Data Encryption Key (DEK)
// Key rotation only re-encrypts the DEK, not all data
var envelope = new EnvelopeEncryption(masterKeyProvider);
```

## Security Properties

- IV regenerated on every save (no IV reuse)
- HMAC verification before decryption (padding oracle prevention)
- Constant-time HMAC comparison (timing attack prevention)
- Buffer zeroing after use (`Array.Clear`)
- Error messages do not leak cryptographic internal state
- No deprecated APIs: `RijndaelManaged`, `BinaryFormatter`, `DES`, `MD5` are never used
