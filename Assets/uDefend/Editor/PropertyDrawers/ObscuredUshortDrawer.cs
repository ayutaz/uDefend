using UnityEditor;
using UnityEngine;
using uDefend.AntiCheat;

namespace uDefend.Editor
{
    [CustomPropertyDrawer(typeof(ObscuredUshort))]
    public class ObscuredUshortDrawer : PropertyDrawer
    {
        private const int ChecksumSalt = 0x48BD_E6A3;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var encValue = property.FindPropertyRelative("_encryptedValue");
            var key = property.FindPropertyRelative("_key");
            var checksum = property.FindPropertyRelative("_checksum");

            int currentValue = 0;
            if (key.intValue != 0)
                currentValue = encValue.intValue ^ (ushort)key.intValue;

            EditorGUI.BeginChangeCheck();
            int newValue = Mathf.Clamp(EditorGUI.IntField(position, label, currentValue), ushort.MinValue, ushort.MaxValue);
            if (EditorGUI.EndChangeCheck())
            {
                encValue.intValue = newValue ^ (ushort)key.intValue;
                checksum.intValue = newValue ^ ChecksumSalt;
            }

            EditorGUI.EndProperty();
        }
    }
}
