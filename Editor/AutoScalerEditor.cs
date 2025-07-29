#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AutoScaler))]
public class AutoScalerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();                       // targetSize

        AutoScaler scaler = (AutoScaler)target;

        if (GUILayout.Button("Fit Now"))
        {
            Undo.RecordObject(scaler.transform, "Auto Scale");
            scaler.FitToTarget();
        }

        // Salt‑okunur gerçek boyut bilgisi
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Real‑world size (m):",
            $"{scaler.debugSize.x:F2} × {scaler.debugSize.y:F2} × {scaler.debugSize.z:F2}");
    }
}
#endif
