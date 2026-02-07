using UnityEditor;
using UnityEngine;

namespace uDefend.Editor
{
    public class ObscuredPrefsEditorWindow : EditorWindow
    {
        [MenuItem("Window/uDefend/Obscured Prefs Inspector")]
        public static void ShowWindow()
        {
            GetWindow<ObscuredPrefsEditorWindow>("Obscured Prefs");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Obscured Prefs Inspector", EditorStyles.boldLabel);
            EditorGUILayout.Space(8);

            EditorGUILayout.HelpBox(
                "ObscuredPrefs keys are SHA256-hashed and cannot be enumerated. " +
                "Use the buttons below to manage all PlayerPrefs.",
                MessageType.Info);

            EditorGUILayout.Space(8);

            GUI.color = new Color(1f, 0.6f, 0.6f);
            if (GUILayout.Button("Clear All PlayerPrefs", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Clear All PlayerPrefs",
                    "This will delete ALL PlayerPrefs (including ObscuredPrefs). This cannot be undone.",
                    "Clear All", "Cancel"))
                {
                    PlayerPrefs.DeleteAll();
                    PlayerPrefs.Save();
                    Debug.Log("[uDefend] All PlayerPrefs cleared.");
                }
            }
            GUI.color = Color.white;
        }
    }
}
