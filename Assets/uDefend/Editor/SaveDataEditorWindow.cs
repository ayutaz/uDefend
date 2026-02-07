using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using uDefend.Core;

namespace uDefend.Editor
{
    public class SaveDataEditorWindow : EditorWindow
    {
        private string _savePath;
        private Vector2 _scrollPos;

        [MenuItem("Window/uDefend/Save Data Inspector")]
        public static void ShowWindow()
        {
            GetWindow<SaveDataEditorWindow>("Save Data Inspector");
        }

        private void OnEnable()
        {
            _savePath = Application.persistentDataPath;
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Save Data Inspector", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            _savePath = EditorGUILayout.TextField("Save Directory", _savePath);

            if (GUILayout.Button("Use persistentDataPath"))
                _savePath = Application.persistentDataPath;

            EditorGUILayout.Space(8);

            if (string.IsNullOrEmpty(_savePath) || !Directory.Exists(_savePath))
            {
                EditorGUILayout.HelpBox("Directory does not exist.", MessageType.Warning);
                return;
            }

            string[] saveFiles = Directory.GetFiles(_savePath, "*.sav");
            if (saveFiles.Length == 0)
            {
                EditorGUILayout.HelpBox("No save files found in this directory.", MessageType.Info);
                return;
            }

            EditorGUILayout.LabelField($"Found {saveFiles.Length} save file(s):", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            foreach (string filePath in saveFiles)
            {
                DrawSaveFileEntry(filePath);
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawSaveFileEntry(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            string fileName = Path.GetFileNameWithoutExtension(filePath);

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.LabelField(fileName, EditorStyles.boldLabel);

            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("Size", FormatFileSize(fileInfo.Length));
            EditorGUILayout.LabelField("Last Modified", fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"));

            // Try to read version from header
            try
            {
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var reader = new BinaryReader(fs))
                {
                    if (SaveFileFormat.ValidateMagic(reader))
                    {
                        ushort version = reader.ReadUInt16();
                        ushort flags = reader.ReadUInt16();
                        var (compressed, encryption) = SaveFileFormat.ParseFlags(flags);
                        EditorGUILayout.LabelField("Version", version.ToString());
                        EditorGUILayout.LabelField("Encryption", encryption.ToString());
                        EditorGUILayout.LabelField("Compressed", compressed.ToString());
                    }
                    else
                    {
                        EditorGUILayout.LabelField("Format", "Invalid (bad magic)");
                    }
                }
            }
            catch (Exception)
            {
                EditorGUILayout.LabelField("Format", "Unreadable");
            }
            EditorGUI.indentLevel--;

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Backup", GUILayout.Width(70)))
            {
                var backupManager = new BackupManager();
                backupManager.CreateBackup(filePath, 3);
                Debug.Log($"[uDefend] Backup created for {fileName}");
            }

            GUI.color = new Color(1f, 0.6f, 0.6f);
            if (GUILayout.Button("Delete", GUILayout.Width(70)))
            {
                if (EditorUtility.DisplayDialog("Delete Save File",
                    $"Are you sure you want to delete '{fileName}'?", "Delete", "Cancel"))
                {
                    File.Delete(filePath);
                    Debug.Log($"[uDefend] Deleted save file: {fileName}");
                }
            }
            GUI.color = Color.white;

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }

        private static string FormatFileSize(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F1} KB";
            return $"{bytes / (1024.0 * 1024.0):F2} MB";
        }
    }
}
