using UnityEditor;
using UnityEngine;
using uDefend.AntiCheat;

namespace uDefend.Editor
{
    [CustomPropertyDrawer(typeof(ObscuredLong))]
    public class ObscuredLongDrawer : PropertyDrawer
    {
        private const long ChecksumSalt = 0x5A3E_F1C7_4B8D_2A6FL;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var encValue = property.FindPropertyRelative("_encryptedValue");
            var key = property.FindPropertyRelative("_key");
            var checksum = property.FindPropertyRelative("_checksum");

            long currentValue = 0;
            if (key.longValue != 0)
                currentValue = encValue.longValue ^ key.longValue;

            EditorGUI.BeginChangeCheck();
            long newValue = EditorGUI.LongField(position, label, currentValue);
            if (EditorGUI.EndChangeCheck())
            {
                encValue.longValue = newValue ^ key.longValue;
                checksum.longValue = newValue ^ ChecksumSalt;
            }

            EditorGUI.EndProperty();
        }
    }
}
