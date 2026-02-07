using UnityEditor;
using UnityEngine;
using uDefend.AntiCheat;

namespace uDefend.Editor
{
    [CustomPropertyDrawer(typeof(ObscuredString))]
    public class ObscuredStringDrawer : PropertyDrawer
    {
        private const int ChecksumSalt = 0x4E6A_B8D3;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var encChars = property.FindPropertyRelative("_encryptedChars");
            var key = property.FindPropertyRelative("_key");
            var checksum = property.FindPropertyRelative("_checksum");

            string currentValue = "";
            if (key.intValue != 0 && encChars != null && encChars.arraySize > 0)
            {
                char[] decrypted = new char[encChars.arraySize];
                for (int i = 0; i < encChars.arraySize; i++)
                {
                    decrypted[i] = (char)(encChars.GetArrayElementAtIndex(i).intValue ^ (key.intValue + i));
                }
                currentValue = new string(decrypted);
            }

            EditorGUI.BeginChangeCheck();
            string newValue = EditorGUI.TextField(position, label, currentValue);
            if (EditorGUI.EndChangeCheck())
            {
                if (newValue == null)
                    newValue = "";

                encChars.arraySize = newValue.Length;
                for (int i = 0; i < newValue.Length; i++)
                {
                    encChars.GetArrayElementAtIndex(i).intValue = newValue[i] ^ (key.intValue + i);
                }
                checksum.intValue = ComputeChecksum(newValue);
            }

            EditorGUI.EndProperty();
        }

        private static int ComputeChecksum(string value)
        {
            if (value == null) return ChecksumSalt;
            int hash = ChecksumSalt;
            for (int i = 0; i < value.Length; i++)
            {
                hash = hash * 31 + value[i];
            }
            return hash;
        }
    }
}
