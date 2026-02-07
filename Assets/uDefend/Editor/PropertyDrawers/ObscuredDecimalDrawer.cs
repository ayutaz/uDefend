using UnityEditor;
using UnityEngine;
using uDefend.AntiCheat;

namespace uDefend.Editor
{
    [CustomPropertyDrawer(typeof(ObscuredDecimal))]
    public class ObscuredDecimalDrawer : PropertyDrawer
    {
        private const int ChecksumSalt = 0x4D8E_B2C7;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var enc0 = property.FindPropertyRelative("_encrypted0");
            var enc1 = property.FindPropertyRelative("_encrypted1");
            var enc2 = property.FindPropertyRelative("_encrypted2");
            var enc3 = property.FindPropertyRelative("_encrypted3");
            var key0 = property.FindPropertyRelative("_key0");
            var key1 = property.FindPropertyRelative("_key1");
            var key2 = property.FindPropertyRelative("_key2");
            var key3 = property.FindPropertyRelative("_key3");
            var checksum = property.FindPropertyRelative("_checksum");

            decimal currentValue = 0m;
            bool hasKey = key0.intValue != 0 || key1.intValue != 0 || key2.intValue != 0 || key3.intValue != 0;
            if (hasKey)
            {
                int[] bits = new int[4];
                bits[0] = enc0.intValue ^ key0.intValue;
                bits[1] = enc1.intValue ^ key1.intValue;
                bits[2] = enc2.intValue ^ key2.intValue;
                bits[3] = enc3.intValue ^ key3.intValue;
                currentValue = new decimal(bits);
            }

            EditorGUI.BeginChangeCheck();
            string text = EditorGUI.TextField(position, label, currentValue.ToString());
            if (EditorGUI.EndChangeCheck())
            {
                if (decimal.TryParse(text, out decimal newValue))
                {
                    int[] newBits = decimal.GetBits(newValue);
                    enc0.intValue = newBits[0] ^ key0.intValue;
                    enc1.intValue = newBits[1] ^ key1.intValue;
                    enc2.intValue = newBits[2] ^ key2.intValue;
                    enc3.intValue = newBits[3] ^ key3.intValue;
                    checksum.intValue = newBits[0] ^ newBits[1] ^ newBits[2] ^ newBits[3] ^ ChecksumSalt;
                }
            }

            EditorGUI.EndProperty();
        }
    }
}
