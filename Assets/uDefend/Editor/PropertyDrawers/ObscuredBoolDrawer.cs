using UnityEditor;
using UnityEngine;
using uDefend.AntiCheat;

namespace uDefend.Editor
{
    [CustomPropertyDrawer(typeof(ObscuredBool))]
    public class ObscuredBoolDrawer : PropertyDrawer
    {
        private const int ChecksumSalt = 0x3C8F_D62B;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var encValue = property.FindPropertyRelative("_encryptedValue");
            var key = property.FindPropertyRelative("_key");
            var checksum = property.FindPropertyRelative("_checksum");

            bool currentValue = false;
            if (key.intValue != 0)
                currentValue = (encValue.intValue ^ key.intValue) != 0;

            EditorGUI.BeginChangeCheck();
            bool newValue = EditorGUI.Toggle(position, label, currentValue);
            if (EditorGUI.EndChangeCheck())
            {
                int intVal = newValue ? 1 : 0;
                encValue.intValue = intVal ^ key.intValue;
                checksum.intValue = intVal ^ ChecksumSalt;
            }

            EditorGUI.EndProperty();
        }
    }
}
