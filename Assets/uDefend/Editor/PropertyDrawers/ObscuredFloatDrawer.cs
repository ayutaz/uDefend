using System;
using UnityEditor;
using UnityEngine;
using uDefend.AntiCheat;

namespace uDefend.Editor
{
    [CustomPropertyDrawer(typeof(ObscuredFloat))]
    public class ObscuredFloatDrawer : PropertyDrawer
    {
        private const int ChecksumSalt = 0x7B2D_A4E9;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var encValue = property.FindPropertyRelative("_encryptedValue");
            var key = property.FindPropertyRelative("_key");
            var checksum = property.FindPropertyRelative("_checksum");

            float currentValue = 0f;
            if (key.intValue != 0)
            {
                int bits = encValue.intValue ^ key.intValue;
                currentValue = BitConverter.Int32BitsToSingle(bits);
            }

            EditorGUI.BeginChangeCheck();
            float newValue = EditorGUI.FloatField(position, label, currentValue);
            if (EditorGUI.EndChangeCheck())
            {
                int newBits = BitConverter.SingleToInt32Bits(newValue);
                encValue.intValue = newBits ^ key.intValue;
                checksum.intValue = newBits ^ ChecksumSalt;
            }

            EditorGUI.EndProperty();
        }
    }
}
