// Assets/Editor/AutoScalerAutoAttach.cs
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Vahaponur.AutoScaler;

[InitializeOnLoad]
public static class AutoScalerAutoAttach
{
    static AutoScalerAutoAttach()
    {
        // Triggered on every hierarchy change
        EditorApplication.hierarchyChanged += OnHierarchyChanged;
    }

    static void OnHierarchyChanged()
    {
        // Check if auto-attach is enabled
        if (!AutoScalerSettings.AutoAttachEnabled)
            return;
            
        // Find recently added objects
        foreach (GameObject go in Selection.gameObjects)
        {
            if (go == null) continue;
            // Add if has renderer and doesn't have AutoScaler yet
            if (go.GetComponent<Renderer>() && !go.GetComponent<AutoScaler>())
            {
                Undo.AddComponent<AutoScaler>(go);   // undo support
                // Optionally auto-fit:
                // go.GetComponent<AutoScaler>().FitToTarget();
            }
        }
    }
}
#endif
