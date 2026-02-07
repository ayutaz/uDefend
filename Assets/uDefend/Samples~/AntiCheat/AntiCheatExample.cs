using UnityEngine;
using uDefend.AntiCheat;

/// <summary>
/// Anti-cheat usage example.
/// Demonstrates ObscuredTypes for memory protection and Detector setup.
/// </summary>
public class AntiCheatExample : MonoBehaviour
{
    // ObscuredTypes protect values in memory from memory editors (e.g., GameGuardian, Cheat Engine).
    // They encrypt the value with XOR and verify integrity via checksum.
    // Use them as drop-in replacements for standard types.
    private ObscuredInt _health = 100;
    private ObscuredFloat _speed = 5.5f;
    private ObscuredString _playerName = "Hero";
    private ObscuredLong _score = 0;

    // For Inspector-visible fields, use [SerializeField]
    [SerializeField] private ObscuredInt _gold = 500;
    [SerializeField] private ObscuredVector3 _position;

    private void Start()
    {
        SetupCheatingDetection();
        DemoObscuredTypes();
        DemoObscuredPrefs();
    }

    private void SetupCheatingDetection()
    {
        // ObscuredTypes raise a static event when memory tampering is detected.
        // Subscribe once to handle all types.
        ObscuredInt.OnCheatingDetected += HandleCheatingDetected;
        ObscuredFloat.OnCheatingDetected += HandleCheatingDetected;
        ObscuredString.OnCheatingDetected += HandleCheatingDetected;
        ObscuredLong.OnCheatingDetected += HandleCheatingDetected;
    }

    private void HandleCheatingDetected()
    {
        Debug.LogWarning("Cheating detected! Taking action...");
        // Possible responses:
        // - Log the incident for analytics
        // - Disconnect from multiplayer
        // - Invalidate the save file
        // - Reduce privileges
    }

    private void DemoObscuredTypes()
    {
        Debug.Log("=== ObscuredTypes Demo ===");

        // Use ObscuredTypes just like normal types — implicit conversion operators handle everything
        _health = _health - 25;
        Debug.Log($"Health: {(int)_health}");  // 75

        _speed = _speed * 1.5f;
        Debug.Log($"Speed: {(float)_speed}");

        _score = _score + 1000;
        Debug.Log($"Score: {(long)_score}");

        _playerName = "Champion";
        Debug.Log($"Name: {(string)_playerName}");

        // Vector types work the same way
        _position = new Vector3(1f, 2f, 3f);
        Vector3 pos = _position;
        Debug.Log($"Position: {pos}");
    }

    private void DemoObscuredPrefs()
    {
        Debug.Log("=== ObscuredPrefs Demo ===");

        // ObscuredPrefs wraps PlayerPrefs with encryption.
        // Values are stored encrypted — memory editors cannot read or modify them.
        ObscuredPrefs.SetInt("HighScore", 99999);
        ObscuredPrefs.SetString("LastLevel", "World-3");
        ObscuredPrefs.SetFloat("Volume", 0.8f);

        int highScore = ObscuredPrefs.GetInt("HighScore", 0);
        string lastLevel = ObscuredPrefs.GetString("LastLevel", "");
        float volume = ObscuredPrefs.GetFloat("Volume", 1.0f);

        Debug.Log($"HighScore: {highScore}, LastLevel: {lastLevel}, Volume: {volume}");

        // File-based encrypted preferences (for larger data)
        ObscuredFilePrefs.SetInt("TotalPlayTime", 36000);
        int playTime = ObscuredFilePrefs.GetInt("TotalPlayTime", 0);
        Debug.Log($"TotalPlayTime: {playTime}");
    }

    private void OnDestroy()
    {
        ObscuredInt.OnCheatingDetected -= HandleCheatingDetected;
        ObscuredFloat.OnCheatingDetected -= HandleCheatingDetected;
        ObscuredString.OnCheatingDetected -= HandleCheatingDetected;
        ObscuredLong.OnCheatingDetected -= HandleCheatingDetected;
    }
}
