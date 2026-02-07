using UnityEditor;
using UnityEngine;
using uDefend.AntiCheat;

namespace uDefend.Editor
{
    [CustomPropertyDrawer(typeof(ObscuredByte))]
    public class ObscuredByteDrawer : PropertyDrawer
    {
        private const int ChecksumSalt = 0x2F7C_A3E1;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var encValue = property.FindPropertyRelative("_encryptedValue");
            var key = property.FindPropertyRelative("_key");
            var checksum = property.FindPropertyRelative("_checksum");

            int currentValue = 0;
            if (key.intValue != 0)
                currentValue = encValue.intValue ^ (byte)key.intValue;

            EditorGUI.BeginChangeCheck();
            int newValue = Mathf.Clamp(EditorGUI.IntField(position, label, currentValue), byte.MinValue, byte.MaxValue);
            if (EditorGUI.EndChangeCheck())
            {
                encValue.intValue = newValue ^ (byte)key.intValue;
                checksum.intValue = newValue ^ ChecksumSalt;
            }

            EditorGUI.EndProperty();
        }
    }
}
