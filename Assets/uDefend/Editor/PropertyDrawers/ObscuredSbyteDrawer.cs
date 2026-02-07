using UnityEditor;
using UnityEngine;
using uDefend.AntiCheat;

namespace uDefend.Editor
{
    [CustomPropertyDrawer(typeof(ObscuredSbyte))]
    public class ObscuredSbyteDrawer : PropertyDrawer
    {
        private const int ChecksumSalt = 0x1D5B_C8F4;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var encValue = property.FindPropertyRelative("_encryptedValue");
            var key = property.FindPropertyRelative("_key");
            var checksum = property.FindPropertyRelative("_checksum");

            int currentValue = 0;
            if (key.intValue != 0)
                currentValue = encValue.intValue ^ (sbyte)key.intValue;

            EditorGUI.BeginChangeCheck();
            int newValue = Mathf.Clamp(EditorGUI.IntField(position, label, currentValue), sbyte.MinValue, sbyte.MaxValue);
            if (EditorGUI.EndChangeCheck())
            {
                encValue.intValue = newValue ^ (sbyte)key.intValue;
                checksum.intValue = newValue ^ ChecksumSalt;
            }

            EditorGUI.EndProperty();
        }
    }
}
