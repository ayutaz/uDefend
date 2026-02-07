using UnityEditor;
using UnityEngine;
using uDefend.AntiCheat;

namespace uDefend.Editor
{
    [CustomPropertyDrawer(typeof(ObscuredInt))]
    public class ObscuredIntDrawer : PropertyDrawer
    {
        private const int ChecksumSalt = 0x5A3E_F1C7;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var encValue = property.FindPropertyRelative("_encryptedValue");
            var key = property.FindPropertyRelative("_key");
            var checksum = property.FindPropertyRelative("_checksum");

            int currentValue = 0;
            if (key.intValue != 0)
                currentValue = encValue.intValue ^ key.intValue;

            EditorGUI.BeginChangeCheck();
            int newValue = EditorGUI.IntField(position, label, currentValue);
            if (EditorGUI.EndChangeCheck())
            {
                encValue.intValue = newValue ^ key.intValue;
                checksum.intValue = newValue ^ ChecksumSalt;
            }

            EditorGUI.EndProperty();
        }
    }
}
