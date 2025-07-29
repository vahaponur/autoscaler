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
        // Her hierarchy değiştiğinde tetiklenir
        EditorApplication.hierarchyChanged += OnHierarchyChanged;
    }

    static void OnHierarchyChanged()
    {
        // Check if auto-attach is enabled
        if (!AutoScalerSettings.AutoAttachEnabled)
            return;
            
        // Son eklenen objeleri bul
        foreach (GameObject go in Selection.gameObjects)
        {
            if (go == null) continue;
            // Render'ı varsa ve hâlâ AutoScaler yoksa ekle
            if (go.GetComponent<Renderer>() && !go.GetComponent<AutoScaler>())
            {
                Undo.AddComponent<AutoScaler>(go);   // undo desteği
                // İstersen otomatik Fit'le:
                // go.GetComponent<AutoScaler>().FitToTarget();
            }
        }
    }
}
#endif
