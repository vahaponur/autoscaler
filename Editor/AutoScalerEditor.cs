#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Vahaponur.AutoScaler;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(global::AutoScaler))]
[CanEditMultipleObjects]
public class AutoScalerEditor : Editor
{
    private SerializedProperty selectedPresetNameProp;
    private SerializedProperty targetSizeProp;
    private SerializedProperty includeChildrenProp;
    private SerializedProperty scaleAnchorProp;
    private SerializedProperty anchorOffsetProp;
    private string iconPath = "Packages/com.vahaponur.autoscaler/Editor/Icon/AutoScaler_Icon.png";
    private string[] presetNames;
    private List<ScalePreset> presets;
    
    // For icon
    private Texture2D customIcon;
    
    private void OnEnable()
    {
        selectedPresetNameProp = serializedObject.FindProperty("selectedPresetName");
        targetSizeProp = serializedObject.FindProperty("targetSize");
        includeChildrenProp = serializedObject.FindProperty("includeChildren");
        scaleAnchorProp = serializedObject.FindProperty("scaleAnchor");
        anchorOffsetProp = serializedObject.FindProperty("anchorOffset");
        
        // Load icon
        customIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);
        
        RefreshPresetList();
    }
    
    private void RefreshPresetList()
    {
        presets = AutoScalerPresets.GetAllPresets();
        var nameList = new List<string> { "None" };
        nameList.AddRange(presets.Select(p => p.name));
        presetNames = nameList.ToArray();
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        global::AutoScaler scaler = (global::AutoScaler)target;
        
        // Custom Header with Icon
        EditorGUILayout.Space(5);
        GUILayout.BeginHorizontal();
        
        // Show icon (if loaded)
        if (customIcon != null)
        {
            GUILayout.Label(customIcon, GUILayout.Width(32), GUILayout.Height(32));
        }
        
        // Component title
        GUILayout.BeginVertical();
        GUILayout.Space(6);
        GUILayout.Label("Auto Scaler", EditorStyles.boldLabel);
        GUILayout.EndVertical();
        
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
        
        // Preset Selection
        EditorGUILayout.LabelField("Preset", EditorStyles.boldLabel);
        
        // Find current selection index
        int currentIndex = 0;
        for (int i = 0; i < presetNames.Length; i++)
        {
            if (presetNames[i] == selectedPresetNameProp.stringValue)
            {
                currentIndex = i;
                break;
            }
        }
        
        // Draw preset dropdown
        EditorGUI.BeginChangeCheck();
        int newIndex = EditorGUILayout.Popup("Select Preset", currentIndex, presetNames);
        if (EditorGUI.EndChangeCheck())
        {
            selectedPresetNameProp.stringValue = presetNames[newIndex];
            
            // Apply preset settings
            if (newIndex > 0) // Not "None"
            {
                var preset = presets[newIndex - 1]; // -1 because "None" is at index 0
                
                Undo.RecordObject(scaler, "Apply Preset");
                
                targetSizeProp.floatValue = preset.targetSize;
                scaleAnchorProp.enumValueIndex = (int)preset.pivotPreset;
                anchorOffsetProp.vector3Value = preset.customPivotOffset;
                
                // Apply immediately
                serializedObject.ApplyModifiedProperties();
                scaler.FitToTarget();
            }
        }
        
        // Quick preset buttons
        EditorGUILayout.BeginHorizontal();
        foreach (var preset in presets.Where(p => p.isDefault))
        {
            if (GUILayout.Button(preset.name))
            {
                Undo.RecordObject(scaler, $"Apply {preset.name} Preset");
                
                selectedPresetNameProp.stringValue = preset.name;
                targetSizeProp.floatValue = preset.targetSize;
                scaleAnchorProp.enumValueIndex = (int)preset.pivotPreset;
                anchorOffsetProp.vector3Value = preset.customPivotOffset;
                
                serializedObject.ApplyModifiedProperties();
                scaler.FitToTarget();
            }
        }
        EditorGUILayout.EndHorizontal();
        
        // Manual Settings (show as disabled if preset is selected)
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Manual Settings", EditorStyles.boldLabel);
        
        bool hasPreset = selectedPresetNameProp.stringValue != "None";
        
        GUI.enabled = !hasPreset;
        EditorGUILayout.PropertyField(targetSizeProp);
        GUI.enabled = true;
        
        EditorGUILayout.PropertyField(includeChildrenProp);
        
        // Scale Anchor
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Scale Anchor", EditorStyles.boldLabel);
        
        GUI.enabled = !hasPreset;
        
        // Draw scale anchor dropdown
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(scaleAnchorProp);
        if (EditorGUI.EndChangeCheck())
        {
            // Update anchor offset based on selected preset
            PivotPreset selectedPreset = (PivotPreset)scaleAnchorProp.enumValueIndex;
            UpdateAnchorOffsetForPreset(selectedPreset);
            
            // Clear preset selection when manually changing
            if (hasPreset)
                selectedPresetNameProp.stringValue = "None";
        }
        
        // Draw anchor offset
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(anchorOffsetProp);
        if (EditorGUI.EndChangeCheck())
        {
            // Check if the entered values match any preset
            CheckAndUpdatePresetFromOffset();
            
            // Clear preset selection when manually changing
            if (hasPreset)
                selectedPresetNameProp.stringValue = "None";
        }
        
        GUI.enabled = true;
        
        serializedObject.ApplyModifiedProperties();

        // Size mismatch warning
        if (scaler.targetSize > 0 && !scaler.IsScaleCorrect())
        {
            // Use local bounds for rotation-independent size check
            Bounds localBounds = scaler.GetLocalBounds();
            float currentLargest = Mathf.Max(localBounds.size.x, localBounds.size.y, localBounds.size.z);
            
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "⚠️ Size doesn't match target!\n" +
                $"Target size: {scaler.targetSize:F2}m\n" +
                $"Current size: {currentLargest:F2}m\n" +
                $"Difference: {Mathf.Abs(currentLargest - scaler.targetSize):F2}m", 
                MessageType.Warning);
            
            // Big button to fix it
            GUI.backgroundColor = new Color(1f, 0.6f, 0f); // Orange
            if (GUILayout.Button("Apply Target Size", GUILayout.Height(30)))
            {
                Undo.RecordObject(scaler.transform, "Apply Target Size");
                scaler.FitToTarget();
            }
            GUI.backgroundColor = Color.white;
        }

        // Real-world size info
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Real-world size (m):",
            $"W:{scaler.debugSize.x:F2} × H:{scaler.debugSize.y:F2} × D:{scaler.debugSize.z:F2}");
            
        // Show current bounds height
        if (scaler.debugSize.y > 0)
        {
            EditorGUILayout.LabelField("Current Height:", $"{scaler.debugSize.y:F2} m");
        }
    }
    
    private void UpdateAnchorOffsetForPreset(PivotPreset preset)
    {
        Vector3 newOffset = anchorOffsetProp.vector3Value;
        
        switch (preset)
        {
            case PivotPreset.BottomCenter:
                newOffset = new Vector3(0.5f, 0f, 0.5f);
                break;
            case PivotPreset.Center:
                newOffset = new Vector3(0.5f, 0.5f, 0.5f);
                break;
            case PivotPreset.TopCenter:
                newOffset = new Vector3(0.5f, 1f, 0.5f);
                break;
            case PivotPreset.Custom:
                // Don't change offset for custom
                return;
        }
        
        anchorOffsetProp.vector3Value = newOffset;
    }
    
    private void CheckAndUpdatePresetFromOffset()
    {
        Vector3 offset = anchorOffsetProp.vector3Value;
        
        // Check if offset matches any preset
        if (Mathf.Approximately(offset.x, 0.5f) && Mathf.Approximately(offset.z, 0.5f))
        {
            if (Mathf.Approximately(offset.y, 0f))
            {
                scaleAnchorProp.enumValueIndex = (int)PivotPreset.BottomCenter;
            }
            else if (Mathf.Approximately(offset.y, 0.5f))
            {
                scaleAnchorProp.enumValueIndex = (int)PivotPreset.Center;
            }
            else if (Mathf.Approximately(offset.y, 1f))
            {
                scaleAnchorProp.enumValueIndex = (int)PivotPreset.TopCenter;
            }
            else
            {
                scaleAnchorProp.enumValueIndex = (int)PivotPreset.Custom;
            }
        }
        else
        {
            scaleAnchorProp.enumValueIndex = (int)PivotPreset.Custom;
        }
    }
}
#endif