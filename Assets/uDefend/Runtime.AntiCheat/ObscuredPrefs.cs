using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace uDefend.AntiCheat
{
    /// <summary>
    /// Encrypted wrapper around PlayerPrefs. Key names are hashed with SHA256 to prevent enumeration.
    /// Values are XOR-obfuscated with a checksum for tamper detection.
    /// This is NOT crypto-grade encryption -- it provides obfuscation suitable for PlayerPrefs.
    /// </summary>
    public static class ObscuredPrefs
    {
        private const char Separator = '|';
        private const int ChecksumLength = 8;

        private static string _deviceSalt;

        public static event Action OnTamperingDetected;

        private static string DeviceSalt
        {
            get
            {
                if (_deviceSalt == null)
                    _deviceSalt = SystemInfo.deviceUniqueIdentifier;
                return _deviceSalt;
            }
        }

        // --- Int ---

        public static void SetInt(string key, int value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            var hashedKey = HashKey(key);
            var encrypted = EncryptValue(BitConverter.GetBytes(value), hashedKey);
            PlayerPrefs.SetString(hashedKey, encrypted);
        }

        public static int GetInt(string key, int defaultValue = 0)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            var hashedKey = HashKey(key);
            if (!PlayerPrefs.HasKey(hashedKey))
                return defaultValue;

            var raw = PlayerPrefs.GetString(hashedKey);
            var bytes = DecryptValue(raw, hashedKey);
            if (bytes == null)
                return defaultValue;

            return BitConverter.ToInt32(bytes, 0);
        }

        // --- Float ---

        public static void SetFloat(string key, float value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            var hashedKey = HashKey(key);
            var encrypted = EncryptValue(BitConverter.GetBytes(value), hashedKey);
            PlayerPrefs.SetString(hashedKey, encrypted);
        }

        public static float GetFloat(string key, float defaultValue = 0f)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            var hashedKey = HashKey(key);
            if (!PlayerPrefs.HasKey(hashedKey))
                return defaultValue;

            var raw = PlayerPrefs.GetString(hashedKey);
            var bytes = DecryptValue(raw, hashedKey);
            if (bytes == null)
                return defaultValue;

            return BitConverter.ToSingle(bytes, 0);
        }

        // --- String ---

        public static void SetString(string key, string value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (value == null) throw new ArgumentNullException(nameof(value));
            var hashedKey = HashKey(key);
            var encrypted = EncryptValue(Encoding.UTF8.GetBytes(value), hashedKey);
            PlayerPrefs.SetString(hashedKey, encrypted);
        }

        public static string GetString(string key, string defaultValue = "")
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            var hashedKey = HashKey(key);
            if (!PlayerPrefs.HasKey(hashedKey))
                return defaultValue;

            var raw = PlayerPrefs.GetString(hashedKey);
            var bytes = DecryptValue(raw, hashedKey);
            if (bytes == null)
                return defaultValue;

            return Encoding.UTF8.GetString(bytes);
        }

        // --- Bool ---

        public static void SetBool(string key, bool value)
        {
            SetInt(key, value ? 1 : 0);
        }

        public static bool GetBool(string key, bool defaultValue = false)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            var hashedKey = HashKey(key);
            if (!PlayerPrefs.HasKey(hashedKey))
                return defaultValue;

            return GetInt(key) != 0;
        }

        // --- Management ---

        public static bool HasKey(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            return PlayerPrefs.HasKey(HashKey(key));
        }

        public static void DeleteKey(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            PlayerPrefs.DeleteKey(HashKey(key));
        }

        public static void DeleteAll()
        {
            PlayerPrefs.DeleteAll();
        }

        public static void Save()
        {
            PlayerPrefs.Save();
        }

        // --- Internal helpers ---

        internal static string HashKey(string rawKey)
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

        /// <summary>
        /// Decrypts a stored value. Returns null and fires OnTamperingDetected if checksum fails.
        /// </summary>
        private static byte[] DecryptValue(string stored, string hashedKey)
        {
            var sepIndex = stored.LastIndexOf(Separator);
            if (sepIndex < 0)
            {
                OnTamperingDetected?.Invoke();
                return null;
            }

            var encoded = stored.Substring(0, sepIndex);
            var storedChecksum = stored.Substring(sepIndex + 1);

            byte[] xored;
            try
            {
                xored = Convert.FromBase64String(encoded);
            }
            catch (FormatException)
            {
                OnTamperingDetected?.Invoke();
                return null;
            }

            var keyBytes = Encoding.UTF8.GetBytes(hashedKey);
            var valueBytes = XorBytes(xored, keyBytes);
            var computedChecksum = ComputeChecksum(valueBytes);

            if (storedChecksum != computedChecksum)
            {
                OnTamperingDetected?.Invoke();
                return null;
            }

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
