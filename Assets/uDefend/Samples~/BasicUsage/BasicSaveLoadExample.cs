using System;
using System.Text;
using UnityEngine;
using uDefend.Core;
using uDefend.KeyManagement;

/// <summary>
/// Basic Save/Load example using uDefend.
/// Demonstrates the simplest way to save and load encrypted data.
/// </summary>
public class BasicSaveLoadExample : MonoBehaviour
{
    [SerializeField] private SaveSettings _saveSettings;

    private SaveManager _saveManager;

    [Serializable]
    public class PlayerData
    {
        public string PlayerName;
        public int Level;
        public float PlayTime;
        public int Gold;
    }

    private void Start()
    {
        // Create a key provider using the platform's native secure storage.
        // On Windows: DPAPI, Android: Keystore, iOS: Keychain, Other: PBKDF2 fallback
        IKeyProvider keyProvider = PlatformKeyProvider.Create("com.example.mygame");

        // Create the SaveManager with default settings
        _saveManager = new SaveManager(_saveSettings, keyProvider);
    }

    public async void SaveGame()
    {
        var data = new PlayerData
        {
            PlayerName = "Hero",
            Level = 10,
            PlayTime = 3600f,
            Gold = 5000
        };

        try
        {
            // Save to slot "main" — data is automatically serialized, encrypted, and written atomically
            await _saveManager.SaveAsync("main", data);
            Debug.Log("Game saved successfully!");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save: {e.Message}");
        }
    }

    public async void LoadGame()
    {
        try
        {
            if (!_saveManager.Exists("main"))
            {
                Debug.Log("No save file found.");
                return;
            }

            // Load from slot "main" — data is read, HMAC-verified, decrypted, and deserialized
            var data = await _saveManager.LoadAsync<PlayerData>("main");
            Debug.Log($"Loaded: {data.PlayerName}, Level {data.Level}, Gold {data.Gold}");
        }
        catch (uDefend.Encryption.TamperDetectedException)
        {
            Debug.LogError("Save file has been tampered with!");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load: {e.Message}");
        }
    }

    public void DeleteSave()
    {
        _saveManager.Delete("main");
        Debug.Log("Save deleted.");
    }

    public void ListSlots()
    {
        string[] slots = _saveManager.GetAllSlotIds();
        Debug.Log($"Found {slots.Length} save slot(s): {string.Join(", ", slots)}");
    }
}
