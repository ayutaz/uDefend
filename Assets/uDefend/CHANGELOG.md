# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.1.0] - 2026-02-07

### Added

#### Core (M1-M5)
- AES-256-CBC + HMAC-SHA256 authenticated encryption (`AesCbcHmacProvider`)
- AES-GCM stub for future CoreCLR migration (`AesGcmProvider`)
- `NullEncryptionProvider` for development/testing (marked `[Obsolete]`)
- `StringEncryption` utility for simple string encrypt/decrypt
- `CryptoUtility` with CSPRNG, constant-time comparison, secure buffer clearing
- `ISerializer` abstraction with `JsonUtilitySerializer` and `MessagePackSerializerWrapper`
- `SaveManager` with async Save/Load, per-slot locking, slot management
- `SaveFile` with atomic writes (temp file + rename) and "uSAV" binary format
- `SaveSlot` for multi-slot save management
- `SaveSettings` ScriptableObject for zero-code configuration
- `BackupManager` with rotation and restore
- `IMigration` + `MigrationRunner` for sequential N→N+1 version migration

#### Key Management (M3, M6)
- `IKeyProvider` / `IKeyStore` abstractions
- `Pbkdf2KeyProvider` (SHA-256, 600,000 iterations, OWASP 2024)
- `EnvelopeEncryption` (Master → DEK two-tier)
- `KeyDerivation` (HMAC-SHA256 context-separated key derivation)
- `PlatformKeyProvider` factory with automatic platform detection
- `WindowsDpapiKeyProvider` (P/Invoke CryptProtectData/CryptUnprotectData)
- `AndroidKeystoreKeyProvider` + native `KeystoreHelper.java`
- `IosKeychainKeyProvider` + native `KeychainHelper.m`

#### Anti-Cheat (M7, M9)
- 19 ObscuredTypes: Int, Float, Bool, String, Long, Double, Byte, Sbyte, Short, Ushort, Uint, Ulong, Decimal, Char, Vector2, Vector3, Vector2Int, Vector3Int, Quaternion
- XOR encryption with per-instance random keys
- Checksum-based tamper detection with `OnCheatingDetected` static event
- `ObscuredPrefs` (encrypted PlayerPrefs wrapper)
- `ObscuredFilePrefs` (file-based encrypted preferences)
- `DetectorBase` abstract MonoBehaviour with configurable periodic checking
- `SpeedHackDetector`, `TimeCheatingDetector`, `WallHackDetector`, `InjectionDetector`, `ObscuredCheatingDetector`

#### Editor (M8)
- Custom PropertyDrawers for all 19 ObscuredTypes
- `SaveSettingsEditor` custom inspector
- `SaveDataEditorWindow` (Window > uDefend > Save Data Inspector)
- `ObscuredPrefsEditorWindow` (Window > uDefend > Obscured Prefs Inspector)

#### Samples & Documentation (M10)
- 4 sample scenes: Basic Usage, Advanced Encryption, Migration, Anti-Cheat
- Documentation: Getting Started, Save/Load API, Anti-Cheat, Migration, API Reference
- README, CHANGELOG, LICENSE

#### Testing
- 137+ unit tests covering all modules
- ObscuredTypes tests (52+ tests for all 19 types)
- Encryption, key management, migration, serialization tests
