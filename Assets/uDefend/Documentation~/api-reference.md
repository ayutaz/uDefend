# API Reference

## Namespaces

| Namespace | Description |
|-----------|-------------|
| `uDefend.Core` | SaveManager, SaveSettings, SaveFile, SaveSlot, BackupManager |
| `uDefend.Encryption` | IEncryptionProvider, AesCbcHmacProvider, CryptoUtility, StringEncryption |
| `uDefend.KeyManagement` | IKeyProvider, IKeyStore, PlatformKeyProvider, Pbkdf2KeyProvider, EnvelopeEncryption |
| `uDefend.Serialization` | ISerializer, JsonUtilitySerializer, MessagePackSerializerWrapper |
| `uDefend.Migration` | IMigration, MigrationRunner |
| `uDefend.AntiCheat` | ObscuredTypes (19), Detectors (5), ObscuredPrefs, ObscuredFilePrefs |

## Core Classes

### SaveManager

```csharp
// Constructors
SaveManager(SaveSettings settings, IKeyProvider keyProvider)
SaveManager(SaveSettings settings, IKeyProvider keyProvider, MigrationRunner migrationRunner)

// Methods
Task SaveAsync<T>(string slotId, T data)
Task<T> LoadAsync<T>(string slotId)
bool Exists(string slotId)
void Delete(string slotId)
string[] GetAllSlotIds()
```

### SaveSettings (ScriptableObject)

```csharp
EncryptionType Encryption    // AesCbcHmac (default), AesGcm, None
SerializerType Serializer    // Json (default), MessagePack
string SavePath              // Custom path (empty = persistentDataPath)
bool AutoBackup              // false (default)
int MaxBackupCount           // 3 (default)
ushort CurrentVersion        // 1 (default)
ushort MinSupportedVersion   // 1 (default)
```

## Encryption

### IEncryptionProvider

```csharp
byte[] Encrypt(byte[] plaintext, byte[] encryptionKey, byte[] hmacKey)
byte[] Decrypt(byte[] ciphertext, byte[] encryptionKey, byte[] hmacKey)
```

### CryptoUtility

```csharp
static byte[] GenerateRandomBytes(int length)
static bool FixedTimeEquals(byte[] a, byte[] b)
static void SecureClear(byte[] buffer)
```

### StringEncryption

```csharp
static string Encrypt(string plaintext, string password)
static string Decrypt(string ciphertext, string password)
```

## Key Management

### IKeyProvider

```csharp
byte[] GetMasterKey()   // Returns copy — caller must clear
bool HasMasterKey()
```

### IKeyStore (extends IKeyProvider)

```csharp
void StoreMasterKey(byte[] key)
void DeleteMasterKey()
```

### PlatformKeyProvider (Static Factory)

```csharp
static IKeyProvider Create(string identifier)
static IKeyStore CreateStore(string identifier)
```

### Pbkdf2KeyProvider (IDisposable)

```csharp
Pbkdf2KeyProvider(byte[] passphrase)
Pbkdf2KeyProvider(byte[] passphrase, byte[] salt)
byte[] GetSalt()
```

## Migration

### IMigration

```csharp
ushort FromVersion { get; }
ushort ToVersion { get; }
byte[] Migrate(byte[] data)
```

### MigrationRunner

```csharp
MigrationRunner(ushort currentVersion, ushort minSupportedVersion)
void Register(IMigration migration)
bool NeedsMigration(ushort fileVersion)
byte[] Run(byte[] data, ushort fileVersion)
```

## AntiCheat — ObscuredTypes

All ObscuredTypes share these features:
- `[Serializable]` struct
- `implicit operator` for transparent conversion
- `static event Action OnCheatingDetected`

## AntiCheat — Detectors

### DetectorBase (Abstract MonoBehaviour)

```csharp
UnityEvent OnCheatingDetected
bool IsRunning { get; }
void StartDetection()
void StopDetection()
```

### Concrete Detectors

- `SpeedHackDetector` — Stopwatch vs realtimeSinceStartup comparison
- `TimeCheatingDetector` — DateTime.UtcNow regression/jump detection
- `WallHackDetector` — Sandbox collider + raycast verification
- `InjectionDetector` — Assembly whitelist comparison
- `ObscuredCheatingDetector` — Subscribes to all 19 ObscuredType events

## AntiCheat — ObscuredPrefs

```csharp
static void SetInt(string key, int value)
static int GetInt(string key, int defaultValue = 0)
static void SetFloat(string key, float value)
static float GetFloat(string key, float defaultValue = 0f)
static void SetString(string key, string value)
static string GetString(string key, string defaultValue = "")
static bool HasKey(string key)
static void DeleteKey(string key)
static void DeleteAll()
```

## AntiCheat — ObscuredFilePrefs

Same API as ObscuredPrefs, plus:

```csharp
static void Save()    // Flush to disk
static void Load()    // Reload from disk
```
