#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Vahaponur.AutoScaler
{
    public class AutoScalerBatchProcessor : EditorWindow
    {
        // Tab system
        private enum Tab
        {
            Preview,
            ManagePresets,
            CreatePreset
        }
        
        private Tab currentTab = Tab.Preview;
        
        // Scale settings
        private float targetSize = 1f;
        private PivotPreset scaleAnchor = PivotPreset.Center;
        private Vector3 anchorOffset = new Vector3(0.5f, 0.5f, 0.5f);
        private bool includeChildren = true;
        
        // Preset management
        private ScalePreset selectedPreset;
        private Vector2 presetsScrollPos;
        private Vector2 manageScrollPos;
        
        // Create preset
        private string newPresetName = "";
        private Color newPresetColor = new Color(0.5f, 0.7f, 1f);
        
        // Preset editing
        private ScalePreset editingPreset;
        private string editingName;
        private float editingSize;
        private PivotPreset editingPivot;
        private Vector3 editingOffset;
        
        // UI Styles
        private GUIStyle presetButtonStyle;
        private GUIStyle selectedPresetStyle;
        private GUIStyle tabButtonStyle;
        private GUIStyle activeTabStyle;
        
        [MenuItem("Tools/AutoScaler/Batch Process Selected", priority = 10)]
        private static void ShowWindow()
        {
            var window = GetWindow<AutoScalerBatchProcessor>("AutoScaler Batch");
            window.minSize = new Vector2(450, 500);
        }
        
        private void OnEnable()
        {
            // Initialize with first preset if available
            var presets = AutoScalerPresets.GetAllPresets();
            if (presets.Count > 0)
            {
                selectedPreset = presets[0];
                ApplyPresetSettings(selectedPreset);
            }
        }
        
        private void InitStyles()
        {
            if (presetButtonStyle == null)
            {
                presetButtonStyle = new GUIStyle(GUI.skin.button);
                presetButtonStyle.alignment = TextAnchor.MiddleLeft;
                presetButtonStyle.padding = new RectOffset(10, 10, 10, 10);
                presetButtonStyle.fontSize = 12;
            }
            
            if (selectedPresetStyle == null)
            {
                selectedPresetStyle = new GUIStyle(presetButtonStyle);
                // Create a blue background texture
                Texture2D blueTex = new Texture2D(1, 1);
                blueTex.SetPixel(0, 0, new Color(0.2f, 0.5f, 0.9f));
                blueTex.Apply();
                selectedPresetStyle.normal.background = blueTex;
                selectedPresetStyle.normal.textColor = Color.white;
                selectedPresetStyle.hover.background = blueTex;
                selectedPresetStyle.hover.textColor = Color.white;
                selectedPresetStyle.focused.background = blueTex;
                selectedPresetStyle.focused.textColor = Color.white;
                selectedPresetStyle.active.background = blueTex;
                selectedPresetStyle.active.textColor = Color.white;
            }
            
            if (tabButtonStyle == null)
            {
                tabButtonStyle = new GUIStyle(GUI.skin.button);
                tabButtonStyle.fixedHeight = 25;
            }
            
            if (activeTabStyle == null)
            {
                activeTabStyle = new GUIStyle(tabButtonStyle);
                activeTabStyle.fixedHeight = 25;
                // Create a blue background texture
                Texture2D blueTex = new Texture2D(1, 1);
                blueTex.SetPixel(0, 0, new Color(0.2f, 0.5f, 0.9f));
                blueTex.Apply();
                activeTabStyle.normal.background = blueTex;
                activeTabStyle.normal.textColor = Color.white;
                activeTabStyle.hover.background = blueTex;
                activeTabStyle.hover.textColor = Color.white;
                activeTabStyle.focused.background = blueTex;
                activeTabStyle.focused.textColor = Color.white;
                activeTabStyle.active.background = blueTex;
                activeTabStyle.active.textColor = Color.white;
            }
        }
        
        private void OnGUI()
        {
            InitStyles();
            
            EditorGUILayout.LabelField("AutoScaler Batch Processor", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // Tab buttons
            DrawTabs();
            
            EditorGUILayout.Space();
            
            // Tab content
            switch (currentTab)
            {
                case Tab.Preview:
                    DrawPreviewTab();
                    break;
                case Tab.ManagePresets:
                    DrawManagePresetsTab();
                    break;
                case Tab.CreatePreset:
                    DrawCreatePresetTab();
                    break;
            }
        }
        
        private void DrawTabs()
        {
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Preview", currentTab == Tab.Preview ? activeTabStyle : tabButtonStyle, GUILayout.Width(140)))
            {
                currentTab = Tab.Preview;
            }
            
            if (GUILayout.Button("Manage Presets", currentTab == Tab.ManagePresets ? activeTabStyle : tabButtonStyle, GUILayout.Width(140)))
            {
                currentTab = Tab.ManagePresets;
            }
            
            if (GUILayout.Button("Create Preset", currentTab == Tab.CreatePreset ? activeTabStyle : tabButtonStyle, GUILayout.Width(140)))
            {
                currentTab = Tab.CreatePreset;
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        private void DrawPreviewTab()
        {
            // Current settings display
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Current Settings", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Target Size:", GUILayout.Width(100));
            EditorGUILayout.LabelField($"{targetSize:F1}m", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Scale Anchor:", GUILayout.Width(100));
            EditorGUILayout.LabelField(GetAnchorIcon(scaleAnchor) + " " + scaleAnchor.ToString(), EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Include Children:", GUILayout.Width(100));
            EditorGUILayout.LabelField(includeChildren ? "Yes" : "No", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            // Quick presets
            EditorGUILayout.LabelField("Presets", EditorStyles.boldLabel);
            
            var presets = new List<ScalePreset>(AutoScalerPresets.GetAllPresets());
            
            presetsScrollPos = EditorGUILayout.BeginScrollView(presetsScrollPos, GUILayout.Height(200));
            
            // Draw presets in a grid
            int columns = 2;
            int rows = Mathf.CeilToInt(presets.Count / (float)columns);
            
            for (int row = 0; row < rows; row++)
            {
                EditorGUILayout.BeginHorizontal();
                
                for (int col = 0; col < columns; col++)
                {
                    int index = row * columns + col;
                    if (index < presets.Count)
                    {
                        DrawPresetButton(presets[index]);
                        if (col < columns - 1)
                            GUILayout.Space(10); // Horizontal spacing between buttons
                    }
                    else
                    {
                        GUILayout.FlexibleSpace();
                    }
                }
                
                EditorGUILayout.EndHorizontal();
                
                if (row < rows - 1)
                    GUILayout.Space(10); // Vertical spacing between rows
            }
            
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.Space();
            
            // Selection info and apply button
            DrawApplySection();
        }
        
        private void DrawPresetButton(ScalePreset preset)
        {
            bool isSelected = selectedPreset != null && selectedPreset.name == preset.name;
            
            EditorGUILayout.BeginVertical(GUILayout.Width(190));
            
            // Color bar
            var colorRect = GUILayoutUtility.GetRect(190, 3);
            EditorGUI.DrawRect(colorRect, GetPresetColor(preset));
            
            // Button content
            string anchorInfo = preset.pivotPreset == PivotPreset.Custom 
                ? $"Custom ({preset.customPivotOffset.x:F1}, {preset.customPivotOffset.y:F1}, {preset.customPivotOffset.z:F1})"
                : preset.pivotPreset.ToString();
            
            string buttonText = $"{GetAnchorIcon(preset.pivotPreset)} {preset.name}\n{preset.targetSize:F1}m | {anchorInfo}";
            if (preset.isDefault)
            {
                buttonText += " 🔒";
            }
            
            if (GUILayout.Button(buttonText, isSelected ? selectedPresetStyle : presetButtonStyle, GUILayout.Width(190), GUILayout.Height(60)))
            {
                selectedPreset = preset;
                ApplyPresetSettings(preset);
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawManagePresetsTab()
        {
            EditorGUILayout.HelpBox("Manage your scale presets. Built-in presets cannot be deleted but can be updated.", MessageType.Info);
            EditorGUILayout.Space();
            
            var presets = new List<ScalePreset>(AutoScalerPresets.GetAllPresets());
            
            manageScrollPos = EditorGUILayout.BeginScrollView(manageScrollPos);
            
            foreach (var preset in presets)
            {
                DrawPresetManagementRow(preset);
                EditorGUILayout.Space(5);
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        private void DrawPresetManagementRow(ScalePreset preset)
        {
            EditorGUILayout.BeginVertical("box");
            
            // Header row
            EditorGUILayout.BeginHorizontal();
            
            // Color indicator
            var colorRect = GUILayoutUtility.GetRect(5, 20);
            EditorGUI.DrawRect(colorRect, GetPresetColor(preset));
            
            // Name and lock icon
            string displayName = preset.isDefault ? $"🔒 {preset.name}" : preset.name;
            EditorGUILayout.LabelField(displayName, EditorStyles.boldLabel, GUILayout.Width(150));
            
            // Size
            EditorGUILayout.LabelField($"Size: {preset.targetSize:F1}m", GUILayout.Width(80));
            
            // Anchor
            EditorGUILayout.LabelField($"{GetAnchorIcon(preset.pivotPreset)} {preset.pivotPreset}", GUILayout.Width(120));
            
            // Buttons
            if (GUILayout.Button("Update", GUILayout.Width(60)))
            {
                StartEditingPreset(preset);
            }
            
            GUI.enabled = !preset.isDefault;
            if (GUILayout.Button("Delete", GUILayout.Width(60)))
            {
                if (EditorUtility.DisplayDialog("Delete Preset", 
                    $"Delete preset '{preset.name}'?", "Yes", "No"))
                {
                    AutoScalerPresets.RemovePreset(preset.name);
                    if (selectedPreset != null && selectedPreset.name == preset.name)
                    {
                        selectedPreset = null;
                    }
                }
            }
            GUI.enabled = true;
            
            EditorGUILayout.EndHorizontal();
            
            // Edit form (if this preset is being edited)
            if (editingPreset != null && editingPreset.name == preset.name)
            {
                DrawEditForm();
            }
            
            EditorGUILayout.EndVertical();
        }
        
        private void StartEditingPreset(ScalePreset preset)
        {
            editingPreset = preset;
            editingName = preset.name;
            editingSize = preset.targetSize;
            editingPivot = preset.pivotPreset;
            editingOffset = preset.customPivotOffset;
        }
        
        private void DrawEditForm()
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Edit Preset", EditorStyles.boldLabel);
            
            editingSize = EditorGUILayout.FloatField("Target Size (m):", editingSize);
            
            EditorGUI.BeginChangeCheck();
            editingPivot = (PivotPreset)EditorGUILayout.EnumPopup("Scale Anchor:", editingPivot);
            if (EditorGUI.EndChangeCheck())
            {
                UpdateEditingOffsetForPreset(editingPivot);
            }
            
            if (editingPivot == PivotPreset.Custom)
            {
                editingOffset = EditorGUILayout.Vector3Field("Anchor Offset:", editingOffset);
            }
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Save Changes", GUILayout.Height(25)))
            {
                AutoScalerPresets.UpdatePreset(editingPreset.name, editingSize, editingPivot, editingOffset);
                editingPreset = null;
                
                // Update current settings if this was the selected preset
                if (selectedPreset != null && selectedPreset.name == editingName)
                {
                    selectedPreset = AutoScalerPresets.GetPreset(editingName);
                    ApplyPresetSettings(selectedPreset);
                }
            }
            
            if (GUILayout.Button("Cancel", GUILayout.Height(25)))
            {
                editingPreset = null;
            }
            
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
        
        private void DrawCreatePresetTab()
        {
            EditorGUILayout.HelpBox("Create a new preset with the current scale settings.", MessageType.Info);
            EditorGUILayout.Space();
            
            // Preview current settings
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Current Settings Preview", EditorStyles.boldLabel);
            
            EditorGUILayout.LabelField($"Target Size: {targetSize:F1}m");
            EditorGUILayout.LabelField($"Scale Anchor: {GetAnchorIcon(scaleAnchor)} {scaleAnchor}");
            EditorGUILayout.LabelField($"Include Children: {(includeChildren ? "Yes" : "No")}");
            
            if (scaleAnchor == PivotPreset.Custom)
            {
                EditorGUILayout.LabelField($"Anchor Offset: {anchorOffset}");
            }
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space();
            
            // New preset form
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("New Preset Details", EditorStyles.boldLabel);
            
            newPresetName = EditorGUILayout.TextField("Preset Name:", newPresetName);
            newPresetColor = EditorGUILayout.ColorField("Color:", newPresetColor);
            
            EditorGUILayout.Space();
            
            GUI.enabled = !string.IsNullOrEmpty(newPresetName) && targetSize > 0;
            if (GUILayout.Button("Create Preset", GUILayout.Height(30)))
            {
                var newPreset = new ScalePreset(newPresetName, targetSize, scaleAnchor);
                newPreset.customPivotOffset = anchorOffset;
                AutoScalerPresets.AddPreset(newPreset);
                
                // Switch to preview tab and select the new preset
                currentTab = Tab.Preview;
                selectedPreset = newPreset;
                
                // Reset form
                newPresetName = "";
                newPresetColor = new Color(0.5f, 0.7f, 1f);
                
                Debug.Log($"Created preset '{newPreset.name}'");
            }
            GUI.enabled = true;
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawApplySection()
        {
            int selectedCount = Selection.gameObjects.Length;
            
            EditorGUILayout.BeginVertical("box");
            
            // Selection info
            MessageType msgType = selectedCount > 0 ? MessageType.Info : MessageType.Warning;
            string message = selectedCount > 0 
                ? $"{selectedCount} object{(selectedCount > 1 ? "s" : "")} selected" 
                : "No objects selected";
            
            EditorGUILayout.HelpBox(message, msgType);
            
            // Compatibility check
            if (selectedCount > 0)
            {
                int compatibleCount = CountCompatibleObjects();
                if (compatibleCount < selectedCount)
                {
                    EditorGUILayout.HelpBox(
                        $"Only {compatibleCount} of {selectedCount} objects have renderers and can be scaled.", 
                        MessageType.Warning
                    );
                }
            }
            
            // Apply button
            GUI.enabled = selectedCount > 0 && targetSize > 0;
            
            Color oldColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.3f, 0.7f, 0.3f);
            
            if (GUILayout.Button("Apply to Selected Objects", GUILayout.Height(35)))
            {
                ProcessSelectedObjects();
            }
            
            GUI.backgroundColor = oldColor;
            GUI.enabled = true;
            
            EditorGUILayout.EndVertical();
        }
        
        private void ProcessSelectedObjects()
        {
            var selectedObjects = Selection.gameObjects;
            int processedCount = 0;
            
            Undo.SetCurrentGroupName("Batch Scale Objects");
            int undoGroup = Undo.GetCurrentGroup();
            
            foreach (var obj in selectedObjects)
            {
                if (obj == null) continue;
                
                // Check if object has renderer (directly or in children)
                bool hasRenderer = includeChildren ? 
                    obj.GetComponentInChildren<Renderer>() != null :
                    obj.GetComponent<Renderer>() != null;
                    
                if (!hasRenderer) continue;
                
                // Get or add AutoScaler component
                var scaler = obj.GetComponent<global::AutoScaler>();
                if (scaler == null)
                {
                    scaler = Undo.AddComponent<global::AutoScaler>(obj);
                }
                
                // Record for undo
                Undo.RecordObject(scaler, "Batch Scale");
                Undo.RecordObject(obj.transform, "Batch Scale Transform");
                
                // Apply settings
                scaler.targetSize = targetSize;
                scaler.includeChildren = includeChildren;
                scaler.scaleAnchor = scaleAnchor;
                scaler.anchorOffset = anchorOffset;
                
                // Execute scaling
                scaler.FitToTarget();
                
                processedCount++;
            }
            
            Undo.CollapseUndoOperations(undoGroup);
            
            string presetInfo = selectedPreset != null ? $" using preset '{selectedPreset.name}'" : "";
            Debug.Log($"Batch scaled {processedCount} objects to {targetSize}m{presetInfo}");
        }
        
        private void ApplyPresetSettings(ScalePreset preset)
        {
            targetSize = preset.targetSize;
            scaleAnchor = preset.pivotPreset;
            anchorOffset = preset.customPivotOffset;
        }
        
        private int CountCompatibleObjects()
        {
            int count = 0;
            foreach (var obj in Selection.gameObjects)
            {
                if (obj == null) continue;
                
                bool hasRenderer = includeChildren ? 
                    obj.GetComponentInChildren<Renderer>() != null :
                    obj.GetComponent<Renderer>() != null;
                    
                if (hasRenderer) count++;
            }
            return count;
        }
        
        private void UpdateAnchorOffsetForPreset(PivotPreset preset)
        {
            switch (preset)
            {
                case PivotPreset.BottomCenter:
                    anchorOffset = new Vector3(0.5f, 0f, 0.5f);
                    break;
                case PivotPreset.Center:
                    anchorOffset = new Vector3(0.5f, 0.5f, 0.5f);
                    break;
                case PivotPreset.TopCenter:
                    anchorOffset = new Vector3(0.5f, 1f, 0.5f);
                    break;
                case PivotPreset.Custom:
                    // Don't change offset for custom
                    break;
            }
        }
        
        private void UpdateEditingOffsetForPreset(PivotPreset preset)
        {
            switch (preset)
            {
                case PivotPreset.BottomCenter:
                    editingOffset = new Vector3(0.5f, 0f, 0.5f);
                    break;
                case PivotPreset.Center:
                    editingOffset = new Vector3(0.5f, 0.5f, 0.5f);
                    break;
                case PivotPreset.TopCenter:
                    editingOffset = new Vector3(0.5f, 1f, 0.5f);
                    break;
                case PivotPreset.Custom:
                    // Don't change offset for custom
                    break;
            }
        }
        
        private string GetAnchorIcon(PivotPreset preset)
        {
            switch (preset)
            {
                case PivotPreset.BottomCenter:
                    return "⬇";
                case PivotPreset.Center:
                    return "⬛";
                case PivotPreset.TopCenter:
                    return "⬆";
                case PivotPreset.Custom:
                    return "✦";
                default:
                    return "○";
            }
        }
        
        private Color GetPresetColor(ScalePreset preset)
        {
            // Generate a color based on preset name for consistency
            int hash = preset.name.GetHashCode();
            Random.InitState(hash);
            return new Color(
                Random.Range(0.3f, 0.8f),
                Random.Range(0.3f, 0.8f),
                Random.Range(0.3f, 0.8f)
            );
        }
    }
}
#endif