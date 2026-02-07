using UnityEditor;
using UnityEngine;
using uDefend.Core;

namespace uDefend.Editor
{
    [CustomEditor(typeof(SaveSettings))]
    public class SaveSettingsEditor : UnityEditor.Editor
    {
        private SerializedProperty _encryption;
        private SerializedProperty _serializer;
        private SerializedProperty _savePath;
        private SerializedProperty _autoBackup;
        private SerializedProperty _maxBackupCount;
        private SerializedProperty _currentVersion;
        private SerializedProperty _minSupportedVersion;

        private void OnEnable()
        {
            _encryption = serializedObject.FindProperty("Encryption");
            _serializer = serializedObject.FindProperty("Serializer");
            _savePath = serializedObject.FindProperty("SavePath");
            _autoBackup = serializedObject.FindProperty("AutoBackup");
            _maxBackupCount = serializedObject.FindProperty("MaxBackupCount");
            _currentVersion = serializedObject.FindProperty("CurrentVersion");
            _minSupportedVersion = serializedObject.FindProperty("MinSupportedVersion");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Encryption Settings
            EditorGUILayout.LabelField("Encryption Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_encryption);
            if (_encryption.enumValueIndex == (int)EncryptionType.None)
            {
                EditorGUILayout.HelpBox(
                    "Encryption is disabled. Save data will be stored in plaintext. Do NOT ship with this setting.",
                    MessageType.Warning);
            }
            EditorGUILayout.Space(4);

            // Serialization Settings
            EditorGUILayout.LabelField("Serialization Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_serializer);
            EditorGUILayout.Space(4);

            // File Settings
            EditorGUILayout.LabelField("File Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_savePath, new GUIContent("Save Path", "Leave empty to use Application.persistentDataPath"));
            if (string.IsNullOrEmpty(_savePath.stringValue))
            {
                EditorGUILayout.HelpBox(
                    "Using Application.persistentDataPath as default save location.",
                    MessageType.Info);
            }
            EditorGUILayout.Space(4);

            // Backup Settings
            EditorGUILayout.LabelField("Backup Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_autoBackup);
            if (_autoBackup.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_maxBackupCount, new GUIContent("Max Backups"));
                if (_maxBackupCount.intValue < 1)
                    _maxBackupCount.intValue = 1;
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.Space(4);

            // Version Settings
            EditorGUILayout.LabelField("Version Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_currentVersion, new GUIContent("Current Version", "The version written to new save files."));
            EditorGUILayout.PropertyField(_minSupportedVersion, new GUIContent("Min Supported Version", "Save files older than this are rejected."));

            if (_minSupportedVersion.intValue > _currentVersion.intValue)
            {
                EditorGUILayout.HelpBox(
                    "Min Supported Version cannot be greater than Current Version.",
                    MessageType.Error);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
