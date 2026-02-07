using UnityEditor;
using UnityEngine;
using uDefend.AntiCheat;

namespace uDefend.Editor
{
    [CustomPropertyDrawer(typeof(ObscuredUlong))]
    public class ObscuredUlongDrawer : PropertyDrawer
    {
        private const long ChecksumSalt = 0x7A2F_D8B1_3E6C_94A5L;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var encValue = property.FindPropertyRelative("_encryptedValue");
            var key = property.FindPropertyRelative("_key");
            var checksum = property.FindPropertyRelative("_checksum");

            ulong currentValue = 0;
            if (key.longValue != 0)
                currentValue = (ulong)encValue.longValue ^ (ulong)key.longValue;

            EditorGUI.BeginChangeCheck();
            string text = EditorGUI.TextField(position, label, currentValue.ToString());
            if (EditorGUI.EndChangeCheck())
            {
                if (ulong.TryParse(text, out ulong newValue))
                {
                    encValue.longValue = (long)(newValue ^ (ulong)key.longValue);
                    checksum.longValue = (long)(newValue ^ (ulong)ChecksumSalt);
                }
            }

            EditorGUI.EndProperty();
        }
    }
}
