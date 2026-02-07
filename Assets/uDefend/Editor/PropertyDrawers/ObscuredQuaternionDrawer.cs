using System;
using UnityEditor;
using UnityEngine;
using uDefend.AntiCheat;

namespace uDefend.Editor
{
    [CustomPropertyDrawer(typeof(ObscuredQuaternion))]
    public class ObscuredQuaternionDrawer : PropertyDrawer
    {
        private const int ChecksumSalt = 0x5E2C_A8F6;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var encX = property.FindPropertyRelative("_encryptedX");
            var encY = property.FindPropertyRelative("_encryptedY");
            var encZ = property.FindPropertyRelative("_encryptedZ");
            var encW = property.FindPropertyRelative("_encryptedW");
            var keyX = property.FindPropertyRelative("_keyX");
            var keyY = property.FindPropertyRelative("_keyY");
            var keyZ = property.FindPropertyRelative("_keyZ");
            var keyW = property.FindPropertyRelative("_keyW");
            var checksum = property.FindPropertyRelative("_checksum");

            Vector4 currentValue = Vector4.zero;
            if (keyX.intValue != 0 || keyY.intValue != 0 || keyZ.intValue != 0 || keyW.intValue != 0)
            {
                int bitsX = encX.intValue ^ keyX.intValue;
                int bitsY = encY.intValue ^ keyY.intValue;
                int bitsZ = encZ.intValue ^ keyZ.intValue;
                int bitsW = encW.intValue ^ keyW.intValue;
                currentValue = new Vector4(
                    BitConverter.Int32BitsToSingle(bitsX),
                    BitConverter.Int32BitsToSingle(bitsY),
                    BitConverter.Int32BitsToSingle(bitsZ),
                    BitConverter.Int32BitsToSingle(bitsW));
            }

            EditorGUI.BeginChangeCheck();
            Vector4 newValue = EditorGUI.Vector4Field(position, label, currentValue);
            if (EditorGUI.EndChangeCheck())
            {
                int newBitsX = BitConverter.SingleToInt32Bits(newValue.x);
                int newBitsY = BitConverter.SingleToInt32Bits(newValue.y);
                int newBitsZ = BitConverter.SingleToInt32Bits(newValue.z);
                int newBitsW = BitConverter.SingleToInt32Bits(newValue.w);
                encX.intValue = newBitsX ^ keyX.intValue;
                encY.intValue = newBitsY ^ keyY.intValue;
                encZ.intValue = newBitsZ ^ keyZ.intValue;
                encW.intValue = newBitsW ^ keyW.intValue;
                checksum.intValue = newBitsX ^ newBitsY ^ newBitsZ ^ newBitsW ^ ChecksumSalt;
            }

            EditorGUI.EndProperty();
        }
    }
}
