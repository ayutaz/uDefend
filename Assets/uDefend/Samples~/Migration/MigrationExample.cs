using System;
using System.Text;
using UnityEngine;
using uDefend.Core;
using uDefend.KeyManagement;
using uDefend.Migration;

/// <summary>
/// Save data migration example.
/// Demonstrates how to handle save file version upgrades when your data model changes.
/// </summary>
public class MigrationExample : MonoBehaviour
{
    [SerializeField] private SaveSettings _saveSettings;

    private void Start()
    {
        // SaveSettings should have:
        //   CurrentVersion = 3
        //   MinSupportedVersion = 1

        // Create migration runner
        var runner = new MigrationRunner(
            currentVersion: _saveSettings.CurrentVersion,
            minSupportedVersion: _saveSettings.MinSupportedVersion);

        // Register sequential migrations: v1 → v2, v2 → v3
        runner.Register(new MigrationV1ToV2());
        runner.Register(new MigrationV2ToV3());

        // Create SaveManager with migration support
        IKeyProvider keyProvider = PlatformKeyProvider.Create("com.example.mygame");
        var saveManager = new SaveManager(_saveSettings, keyProvider, runner);

        // When LoadAsync is called, if the save file is version 1,
        // it will automatically run: v1→v2 then v2→v3 before deserialization.
        LoadWithMigration(saveManager);
    }

    private async void LoadWithMigration(SaveManager saveManager)
    {
        try
        {
            var data = await saveManager.LoadAsync<PlayerDataV3>("main");
            Debug.Log($"Loaded (migrated): {data.DisplayName}, HP={data.MaxHealth}");
        }
        catch (UnsupportedVersionException e)
        {
            Debug.LogError($"Save file too old: {e.Message}");
        }
        catch (MissingMigrationException e)
        {
            Debug.LogError($"Migration not registered: {e.Message}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Load failed: {e.Message}");
        }
    }

    // --- Data model versions ---

    [Serializable]
    public class PlayerDataV1
    {
        public string Name;
        public int HP;
    }

    [Serializable]
    public class PlayerDataV2
    {
        public string Name;
        public int MaxHealth;  // Renamed from HP
        public int CurrentHealth;  // New field
    }

    [Serializable]
    public class PlayerDataV3
    {
        public string DisplayName;  // Renamed from Name
        public int MaxHealth;
        public int CurrentHealth;
        public string[] Inventory;  // New field
    }

    // --- Migration implementations ---

    /// <summary>
    /// Migration from v1 to v2: Rename HP → MaxHealth, add CurrentHealth.
    /// </summary>
    public class MigrationV1ToV2 : IMigration
    {
        public ushort FromVersion => 1;
        public ushort ToVersion => 2;

        public byte[] Migrate(byte[] data)
        {
            // Parse v1 JSON, transform to v2 JSON
            string json = Encoding.UTF8.GetString(data);
            var v1 = JsonUtility.FromJson<PlayerDataV1>(json);

            var v2 = new PlayerDataV2
            {
                Name = v1.Name,
                MaxHealth = v1.HP,
                CurrentHealth = v1.HP  // Default: full health
            };

            return Encoding.UTF8.GetBytes(JsonUtility.ToJson(v2));
        }
    }

    /// <summary>
    /// Migration from v2 to v3: Rename Name → DisplayName, add Inventory.
    /// </summary>
    public class MigrationV2ToV3 : IMigration
    {
        public ushort FromVersion => 2;
        public ushort ToVersion => 3;

        public byte[] Migrate(byte[] data)
        {
            string json = Encoding.UTF8.GetString(data);
            var v2 = JsonUtility.FromJson<PlayerDataV2>(json);

            var v3 = new PlayerDataV3
            {
                DisplayName = v2.Name,
                MaxHealth = v2.MaxHealth,
                CurrentHealth = v2.CurrentHealth,
                Inventory = new string[0]  // Default: empty inventory
            };

            return Encoding.UTF8.GetBytes(JsonUtility.ToJson(v3));
        }
    }
}
