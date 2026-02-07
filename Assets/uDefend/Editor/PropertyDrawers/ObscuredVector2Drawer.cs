using System;
using UnityEditor;
using UnityEngine;
using uDefend.AntiCheat;

namespace uDefend.Editor
{
    [CustomPropertyDrawer(typeof(ObscuredVector2))]
    public class ObscuredVector2Drawer : PropertyDrawer
    {
        private const int ChecksumSalt = 0x2B7F_C4D8;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var encX = property.FindPropertyRelative("_encryptedX");
            var encY = property.FindPropertyRelative("_encryptedY");
            var keyX = property.FindPropertyRelative("_keyX");
            var keyY = property.FindPropertyRelative("_keyY");
            var checksum = property.FindPropertyRelative("_checksum");

            Vector2 currentValue = Vector2.zero;
            if (keyX.intValue != 0 || keyY.intValue != 0)
            {
                int bitsX = encX.intValue ^ keyX.intValue;
                int bitsY = encY.intValue ^ keyY.intValue;
                currentValue = new Vector2(
                    BitConverter.Int32BitsToSingle(bitsX),
                    BitConverter.Int32BitsToSingle(bitsY));
            }

            EditorGUI.BeginChangeCheck();
            Vector2 newValue = EditorGUI.Vector2Field(position, label, currentValue);
            if (EditorGUI.EndChangeCheck())
            {
                int newBitsX = BitConverter.SingleToInt32Bits(newValue.x);
                int newBitsY = BitConverter.SingleToInt32Bits(newValue.y);
                encX.intValue = newBitsX ^ keyX.intValue;
                encY.intValue = newBitsY ^ keyY.intValue;
                checksum.intValue = newBitsX ^ newBitsY ^ ChecksumSalt;
            }

            EditorGUI.EndProperty();
        }
    }
}
