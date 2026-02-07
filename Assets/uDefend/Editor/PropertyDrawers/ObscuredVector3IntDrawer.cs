using UnityEditor;
using UnityEngine;
using uDefend.AntiCheat;

namespace uDefend.Editor
{
    [CustomPropertyDrawer(typeof(ObscuredVector3Int))]
    public class ObscuredVector3IntDrawer : PropertyDrawer
    {
        private const int ChecksumSalt = 0x3F6B_D9A1;

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

            Vector3Int currentValue = Vector3Int.zero;
            if (keyX.intValue != 0 || keyY.intValue != 0 || keyZ.intValue != 0)
            {
                currentValue = new Vector3Int(
                    encX.intValue ^ keyX.intValue,
                    encY.intValue ^ keyY.intValue,
                    encZ.intValue ^ keyZ.intValue);
            }

            EditorGUI.BeginChangeCheck();
            Vector3Int newValue = EditorGUI.Vector3IntField(position, label, currentValue);
            if (EditorGUI.EndChangeCheck())
            {
                encX.intValue = newValue.x ^ keyX.intValue;
                encY.intValue = newValue.y ^ keyY.intValue;
                encZ.intValue = newValue.z ^ keyZ.intValue;
                checksum.intValue = newValue.x ^ newValue.y ^ newValue.z ^ ChecksumSalt;
            }

            EditorGUI.EndProperty();
        }
    }
}
