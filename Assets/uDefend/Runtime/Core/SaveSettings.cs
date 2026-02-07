using UnityEngine;
using uDefend.Serialization;

namespace uDefend.Core
{
    [CreateAssetMenu(fileName = "SaveSettings", menuName = "uDefend/Save Settings")]
    public class SaveSettings : ScriptableObject
    {
        public EncryptionType Encryption = EncryptionType.AesCbcHmac;
        public SerializerType Serializer = SerializerType.Json;
        public string SavePath;
        public bool AutoBackup = false;
        public int MaxBackupCount = 3;
        public ushort CurrentVersion = 1;
        public ushort MinSupportedVersion = 1;

        public string GetSavePath()
        {
            return string.IsNullOrEmpty(SavePath) ? Application.persistentDataPath : SavePath;
        }
    }

    public enum EncryptionType
    {
        AesCbcHmac,
        AesGcm,
        None
    }
}
