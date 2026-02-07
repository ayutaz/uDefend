using UnityEditor;
using UnityEngine;
using uDefend.AntiCheat;

namespace uDefend.Editor
{
    [CustomPropertyDrawer(typeof(ObscuredChar))]
    public class ObscuredCharDrawer : PropertyDrawer
    {
        private const int ChecksumSalt = 0x59E3_A1D6;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var encValue = property.FindPropertyRelative("_encryptedValue");
            var key = property.FindPropertyRelative("_key");
            var checksum = property.FindPropertyRelative("_checksum");

            char currentValue = '\0';
            if (key.intValue != 0)
                currentValue = (char)(encValue.intValue ^ (char)key.intValue);

            EditorGUI.BeginChangeCheck();
            string text = EditorGUI.TextField(position, label, currentValue.ToString());
            if (EditorGUI.EndChangeCheck())
            {
                char newValue = text.Length > 0 ? text[0] : '\0';
                encValue.intValue = newValue ^ (char)key.intValue;
                checksum.intValue = newValue ^ ChecksumSalt;
            }

            EditorGUI.EndProperty();
        }
    }
}
