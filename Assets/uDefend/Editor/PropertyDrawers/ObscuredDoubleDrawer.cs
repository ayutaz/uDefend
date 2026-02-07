using System;
using UnityEditor;
using UnityEngine;
using uDefend.AntiCheat;

namespace uDefend.Editor
{
    [CustomPropertyDrawer(typeof(ObscuredDouble))]
    public class ObscuredDoubleDrawer : PropertyDrawer
    {
        private const long ChecksumSalt = 0x6C4E_B3A7_D9F2_851EL;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var encValue = property.FindPropertyRelative("_encryptedValue");
            var key = property.FindPropertyRelative("_key");
            var checksum = property.FindPropertyRelative("_checksum");

            double currentValue = 0d;
            if (key.longValue != 0)
            {
                long bits = encValue.longValue ^ key.longValue;
                currentValue = BitConverter.Int64BitsToDouble(bits);
            }

            EditorGUI.BeginChangeCheck();
            double newValue = EditorGUI.DoubleField(position, label, currentValue);
            if (EditorGUI.EndChangeCheck())
            {
                long newBits = BitConverter.DoubleToInt64Bits(newValue);
                encValue.longValue = newBits ^ key.longValue;
                checksum.longValue = newBits ^ ChecksumSalt;
            }

            EditorGUI.EndProperty();
        }
    }
}
