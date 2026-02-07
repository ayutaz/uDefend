# Save Data Migration

## Overview

uDefend supports versioned save files with automatic sequential migration. When your data model changes between game updates, migrations transform old save data to the current format.

## How It Works

1. Each save file stores a version number (uint16) in its header
2. `MigrationRunner` holds registered migrations for each version transition
3. On load, if the file version is older than `CurrentVersion`, migrations run sequentially: `v1→v2→v3→...→vN`

## Setup

### 1. Define Migrations

Implement `IMigration` for each version transition:

```csharp
using uDefend.Migration;

public class MigrationV1ToV2 : IMigration
{
    public ushort FromVersion => 1;
    public ushort ToVersion => 2;

    public byte[] Migrate(byte[] data)
    {
        // Transform serialized data from v1 format to v2 format
        string json = Encoding.UTF8.GetString(data);
        var v1 = JsonUtility.FromJson<DataV1>(json);

        var v2 = new DataV2
        {
            Name = v1.Name,
            MaxHealth = v1.HP,        // Renamed field
            CurrentHealth = v1.HP     // New field with default
        };

        return Encoding.UTF8.GetBytes(JsonUtility.ToJson(v2));
    }
}
```

### 2. Register Migrations

```csharp
var runner = new MigrationRunner(
    currentVersion: 3,
    minSupportedVersion: 1
);

runner.Register(new MigrationV1ToV2());
runner.Register(new MigrationV2ToV3());
```

### 3. Pass to SaveManager

```csharp
var saveManager = new SaveManager(settings, keyProvider, runner);

// Migrations run automatically during LoadAsync
var data = await saveManager.LoadAsync<DataV3>("main");
```

## Rules

- Migrations must be **sequential**: `ToVersion == FromVersion + 1`
- Only one migration per `FromVersion` can be registered
- Files older than `MinSupportedVersion` throw `UnsupportedVersionException`
- Files newer than `CurrentVersion` throw `MigrationException`
- Migration failures throw `MigrationException` with the original exception as inner

## Exceptions

| Exception | When |
|-----------|------|
| `MigrationException` | Base exception / migration logic failure |
| `UnsupportedVersionException` | File version < MinSupportedVersion |
| `MissingMigrationException` | Required migration not registered |

## Best Practices

- Always increment `CurrentVersion` in SaveSettings when changing data models
- Keep old migration code — don't delete `V1ToV2` just because you're on V5
- Test migrations with representative data from each version
- Set `MinSupportedVersion` to drop support for very old formats
