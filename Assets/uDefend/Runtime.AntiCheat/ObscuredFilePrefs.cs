using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace uDefend.AntiCheat
{
    /// <summary>
    /// File-based encrypted preferences. Same obfuscation as ObscuredPrefs but stores to a file
    /// instead of PlayerPrefs. Suitable for data that should not go through PlayerPrefs.
    /// </summary>
    public class ObscuredFilePrefs
    {
        private const char Separator = '|';
        private const char KvSeparator = '=';
        private const int ChecksumLength = 8;

        private readonly string _filePath;
        private readonly Dictionary<string, string> _data = new Dictionary<string, string>();
        private string _deviceSalt;

        private string DeviceSalt
        {
            get
            {
                if (_deviceSalt == null)
                    _deviceSalt = SystemInfo.deviceUniqueIdentifier;
                return _deviceSalt;
            }
        }

        public ObscuredFilePrefs(string filePath = null)
        {
            _filePath = filePath ?? Path.Combine(Application.persistentDataPath, "prefs.dat");
        }

        // --- Int ---

        public void SetInt(string key, int value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            var hashedKey = HashKey(key);
            _data[hashedKey] = EncryptValue(BitConverter.GetBytes(value), hashedKey);
        }

        public int GetInt(string key, int defaultValue = 0)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            var bytes = GetDecryptedBytes(key);
            if (bytes == null) return defaultValue;
            return BitConverter.ToInt32(bytes, 0);
        }

        // --- Float ---

        public void SetFloat(string key, float value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            var hashedKey = HashKey(key);
            _data[hashedKey] = EncryptValue(BitConverter.GetBytes(value), hashedKey);
        }

        public float GetFloat(string key, float defaultValue = 0f)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            var bytes = GetDecryptedBytes(key);
            if (bytes == null) return defaultValue;
            return BitConverter.ToSingle(bytes, 0);
        }

        // --- String ---

        public void SetString(string key, string value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (value == null) throw new ArgumentNullException(nameof(value));
            var hashedKey = HashKey(key);
            _data[hashedKey] = EncryptValue(Encoding.UTF8.GetBytes(value), hashedKey);
        }

        public string GetString(string key, string defaultValue = "")
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            var bytes = GetDecryptedBytes(key);
            if (bytes == null) return defaultValue;
            return Encoding.UTF8.GetString(bytes);
        }

        // --- Bool ---

        public void SetBool(string key, bool value)
        {
            SetInt(key, value ? 1 : 0);
        }

        public bool GetBool(string key, bool defaultValue = false)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            var hashedKey = HashKey(key);
            if (!_data.ContainsKey(hashedKey)) return defaultValue;
            return GetInt(key) != 0;
        }

        // --- Management ---

        public bool HasKey(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            return _data.ContainsKey(HashKey(key));
        }

        public void DeleteKey(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            _data.Remove(HashKey(key));
        }

        public void DeleteAll()
        {
            _data.Clear();
        }

        public void Save()
        {
            var dir = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var tempPath = _filePath + ".tmp";
            using (var writer = new StreamWriter(tempPath, false, Encoding.UTF8))
            {
                foreach (var kvp in _data)
                {
                    writer.WriteLine(kvp.Key + KvSeparator + kvp.Value);
                }
            }

            if (File.Exists(_filePath))
                File.Delete(_filePath);
            File.Move(tempPath, _filePath);
        }

        public void Load()
        {
            _data.Clear();

            if (!File.Exists(_filePath))
                return;

            using (var reader = new StreamReader(_filePath, Encoding.UTF8))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var eqIndex = line.IndexOf(KvSeparator);
                    if (eqIndex < 0) continue;
                    var k = line.Substring(0, eqIndex);
                    var v = line.Substring(eqIndex + 1);
                    _data[k] = v;
                }
            }
        }

        // --- Internal helpers ---

        private byte[] GetDecryptedBytes(string key)
        {
            var hashedKey = HashKey(key);
            if (!_data.TryGetValue(hashedKey, out var stored))
                return null;
            return DecryptValue(stored, hashedKey);
        }

        private string HashKey(string rawKey)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(rawKey + DeviceSalt));
                return Convert.ToBase64String(bytes);
            }
        }

        private static string EncryptValue(byte[] valueBytes, string hashedKey)
        {
            var keyBytes = Encoding.UTF8.GetBytes(hashedKey);
            var xored = XorBytes(valueBytes, keyBytes);
            var encoded = Convert.ToBase64String(xored);
            var checksum = ComputeChecksum(valueBytes);
            return encoded + Separator + checksum;
        }

        private static byte[] DecryptValue(string stored, string hashedKey)
        {
            var sepIndex = stored.LastIndexOf(Separator);
            if (sepIndex < 0)
                return null;

            var encoded = stored.Substring(0, sepIndex);
            var storedChecksum = stored.Substring(sepIndex + 1);

            byte[] xored;
            try
            {
                xored = Convert.FromBase64String(encoded);
            }
            catch (FormatException)
            {
                return null;
            }

            var keyBytes = Encoding.UTF8.GetBytes(hashedKey);
            var valueBytes = XorBytes(xored, keyBytes);
            var computedChecksum = ComputeChecksum(valueBytes);

            if (storedChecksum != computedChecksum)
                return null;

            return valueBytes;
        }

        private static byte[] XorBytes(byte[] data, byte[] key)
        {
            var result = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                result[i] = (byte)(data[i] ^ key[i % key.Length]);
            }
            return result;
        }

        private static string ComputeChecksum(byte[] data)
        {
            using (var sha = SHA256.Create())
            {
                var hash = sha.ComputeHash(data);
                return Convert.ToBase64String(hash).Substring(0, ChecksumLength);
            }
        }
    }
}
