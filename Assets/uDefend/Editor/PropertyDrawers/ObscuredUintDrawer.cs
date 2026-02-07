using System;
using UnityEditor;
using UnityEngine;
using uDefend.AntiCheat;

namespace uDefend.Editor
{
    [CustomPropertyDrawer(typeof(ObscuredUint))]
    public class ObscuredUintDrawer : PropertyDrawer
    {
        private const int ChecksumSalt = 0x6F1D_C5B8;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var encValue = property.FindPropertyRelative("_encryptedValue");
            var key = property.FindPropertyRelative("_key");
            var checksum = property.FindPropertyRelative("_checksum");

            long currentValue = 0;
            if (key.intValue != 0)
                currentValue = (uint)(encValue.intValue ^ key.intValue);

            EditorGUI.BeginChangeCheck();
            long newValue = Math.Clamp(EditorGUI.LongField(position, label, currentValue), uint.MinValue, uint.MaxValue);
            if (EditorGUI.EndChangeCheck())
            {
                encValue.intValue = (int)((uint)newValue ^ (uint)key.intValue);
                checksum.intValue = (int)((uint)newValue ^ (uint)ChecksumSalt);
            }

            EditorGUI.EndProperty();
        }
    }
}
