using UnityEditor;
using UnityEditor.SceneManagement;

namespace uDefend.Tests.Editor
{
    /// <summary>
    /// Automatically saves open scenes before entering play mode to prevent
    /// the "Save Scene" dialog from blocking test execution.
    /// </summary>
    [InitializeOnLoad]
    static class TestSceneAutoSaver
    {
        static TestSceneAutoSaver()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                EditorSceneManager.SaveOpenScenes();
            }
        }
    }
}
