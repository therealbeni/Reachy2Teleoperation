using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
[InitializeOnLoad]
public static class AutoSaver
{
    static double s_TimeBetweenSaves = 60d;
    static double s_LastSaveTime;
    static AutoSaver()
    {
        EditorApplication.update += OnEditorUpdate;
    }
    static void OnEditorUpdate()
    {
        if (EditorApplication.isPlaying)
            return;
        double timeSinceStartup = EditorApplication.timeSinceStartup;
        if (timeSinceStartup - s_LastSaveTime > s_TimeBetweenSaves)
        {
            s_LastSaveTime = timeSinceStartup;
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.isDirty)
                    EditorSceneManager.SaveScene(scene);
            }
        }
    }
}