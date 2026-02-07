using UnityEditor;
using UnityEngine;
using uDefend.AntiCheat;

namespace uDefend.Editor
{
    [CustomPropertyDrawer(typeof(ObscuredVector2Int))]
    public class ObscuredVector2IntDrawer : PropertyDrawer
    {
        private const int ChecksumSalt = 0x1C4A_F7D3;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var encX = property.FindPropertyRelative("_encryptedX");
            var encY = property.FindPropertyRelative("_encryptedY");
            var keyX = property.FindPropertyRelative("_keyX");
            var keyY = property.FindPropertyRelative("_keyY");
            var checksum = property.FindPropertyRelative("_checksum");

            Vector2Int currentValue = Vector2Int.zero;
            if (keyX.intValue != 0 || keyY.intValue != 0)
            {
                currentValue = new Vector2Int(
                    encX.intValue ^ keyX.intValue,
                    encY.intValue ^ keyY.intValue);
            }

            EditorGUI.BeginChangeCheck();
            Vector2Int newValue = EditorGUI.Vector2IntField(position, label, currentValue);
            if (EditorGUI.EndChangeCheck())
            {
                encX.intValue = newValue.x ^ keyX.intValue;
                encY.intValue = newValue.y ^ keyY.intValue;
                checksum.intValue = newValue.x ^ newValue.y ^ ChecksumSalt;
            }

            EditorGUI.EndProperty();
        }
    }
}
