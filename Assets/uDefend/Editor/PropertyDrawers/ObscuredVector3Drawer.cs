using System;
using UnityEditor;
using UnityEngine;
using uDefend.AntiCheat;

namespace uDefend.Editor
{
    [CustomPropertyDrawer(typeof(ObscuredVector3))]
    public class ObscuredVector3Drawer : PropertyDrawer
    {
        private static readonly int ChecksumSalt = unchecked((int)0x8A3D_E5F2);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var encX = property.FindPropertyRelative("_encryptedX");
            var encY = property.FindPropertyRelative("_encryptedY");
            var encZ = property.FindPropertyRelative("_encryptedZ");
            var keyX = property.FindPropertyRelative("_keyX");
            var keyY = property.FindPropertyRelative("_keyY");
            var keyZ = property.FindPropertyRelative("_keyZ");
            var checksum = property.FindPropertyRelative("_checksum");

            Vector3 currentValue = Vector3.zero;
            if (keyX.intValue != 0 || keyY.intValue != 0 || keyZ.intValue != 0)
            {
                int bitsX = encX.intValue ^ keyX.intValue;
                int bitsY = encY.intValue ^ keyY.intValue;
                int bitsZ = encZ.intValue ^ keyZ.intValue;
                currentValue = new Vector3(
                    BitConverter.Int32BitsToSingle(bitsX),
                    BitConverter.Int32BitsToSingle(bitsY),
                    BitConverter.Int32BitsToSingle(bitsZ));
            }

            EditorGUI.BeginChangeCheck();
            Vector3 newValue = EditorGUI.Vector3Field(position, label, currentValue);
            if (EditorGUI.EndChangeCheck())
            {
                int newBitsX = BitConverter.SingleToInt32Bits(newValue.x);
                int newBitsY = BitConverter.SingleToInt32Bits(newValue.y);
                int newBitsZ = BitConverter.SingleToInt32Bits(newValue.z);
                encX.intValue = newBitsX ^ keyX.intValue;
                encY.intValue = newBitsY ^ keyY.intValue;
                encZ.intValue = newBitsZ ^ keyZ.intValue;
                checksum.intValue = newBitsX ^ newBitsY ^ newBitsZ ^ ChecksumSalt;
            }

            EditorGUI.EndProperty();
        }
    }
}
