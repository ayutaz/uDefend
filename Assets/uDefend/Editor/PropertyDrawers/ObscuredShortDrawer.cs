using UnityEditor;
using UnityEngine;
using uDefend.AntiCheat;

namespace uDefend.Editor
{
    [CustomPropertyDrawer(typeof(ObscuredShort))]
    public class ObscuredShortDrawer : PropertyDrawer
    {
        private const int ChecksumSalt = 0x3E9A_D7B2;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var encValue = property.FindPropertyRelative("_encryptedValue");
            var key = property.FindPropertyRelative("_key");
            var checksum = property.FindPropertyRelative("_checksum");

            int currentValue = 0;
            if (key.intValue != 0)
                currentValue = encValue.intValue ^ (short)key.intValue;

            EditorGUI.BeginChangeCheck();
            int newValue = Mathf.Clamp(EditorGUI.IntField(position, label, currentValue), short.MinValue, short.MaxValue);
            if (EditorGUI.EndChangeCheck())
            {
                encValue.intValue = newValue ^ (short)key.intValue;
                checksum.intValue = newValue ^ ChecksumSalt;
            }

            EditorGUI.EndProperty();
        }
    }
}
