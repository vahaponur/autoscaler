#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace Vahaponur.AutoScaler
{
    [Serializable]
    public class ScalePreset
    {
        public string name;
        public float targetSize;
        public PivotPreset pivotPreset;
        public Vector3 customPivotOffset;
        public bool isDefault;
        
        public ScalePreset(string name, float size, PivotPreset pivot = PivotPreset.None, bool isDefault = false)
        {
            this.name = name;
            this.targetSize = size;
            this.pivotPreset = pivot;
            // Set offset based on pivot preset
            switch (pivot)
            {
                case PivotPreset.BottomCenter:
                    this.customPivotOffset = new Vector3(0.5f, 0f, 0.5f);
                    break;
                case PivotPreset.Center:
                    this.customPivotOffset = new Vector3(0.5f, 0.5f, 0.5f);
                    break;
                case PivotPreset.TopCenter:
                    this.customPivotOffset = new Vector3(0.5f, 1f, 0.5f);
                    break;
                default:
                    this.customPivotOffset = new Vector3(0.5f, 0.5f, 0.5f); // Default to center
                    break;
            }
            this.isDefault = isDefault;
        }
    }
    
    [Serializable]
    public class PresetCollection
    {
        public List<ScalePreset> presets = new List<ScalePreset>();
    }
    
    public static class AutoScalerPresets
    {
        private const string PRESETS_PATH = "ProjectSettings/AutoScalerPresets.json";
        
        private static PresetCollection presetCollection;
        
        public static List<ScalePreset> GetAllPresets()
        {
            LoadPresets();
            return presetCollection.presets;
        }
        
        public static void AddPreset(ScalePreset preset)
        {
            LoadPresets();
            
            // Check if preset with same name exists
            var existing = presetCollection.presets.Find(p => p.name == preset.name);
            if (existing != null)
            {
                if (!EditorUtility.DisplayDialog("Preset Exists", 
                    $"A preset named '{preset.name}' already exists. Replace it?", "Yes", "No"))
                {
                    return;
                }
                presetCollection.presets.Remove(existing);
            }
            
            presetCollection.presets.Add(preset);
            SavePresets();
        }
        
        public static void RemovePreset(string name)
        {
            LoadPresets();
            presetCollection.presets.RemoveAll(p => p.name == name && !p.isDefault);
            SavePresets();
        }
        
        public static void UpdatePreset(string name, float newSize, PivotPreset newPivot, Vector3 newOffset)
        {
            LoadPresets();
            var preset = presetCollection.presets.Find(p => p.name == name);
            if (preset != null)
            {
                preset.targetSize = newSize;
                preset.pivotPreset = newPivot;
                preset.customPivotOffset = newOffset;
                SavePresets();
            }
        }
        
        public static ScalePreset GetPreset(string name)
        {
            LoadPresets();
            return presetCollection.presets.Find(p => p.name == name);
        }
        
        private static void LoadPresets()
        {
            if (presetCollection != null) return;
            
            presetCollection = new PresetCollection();
            
            // Add default presets
            presetCollection.presets.Add(new ScalePreset("Small", 0.5f, PivotPreset.Center, true));
            presetCollection.presets.Add(new ScalePreset("Medium", 1.0f, PivotPreset.Center, true));
            presetCollection.presets.Add(new ScalePreset("Large", 2.0f, PivotPreset.Center, true));
            
            if (File.Exists(PRESETS_PATH))
            {
                try
                {
                    string json = File.ReadAllText(PRESETS_PATH);
                    var loaded = JsonUtility.FromJson<PresetCollection>(json);
                    if (loaded != null && loaded.presets != null)
                    {
                        presetCollection = loaded;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to load AutoScaler presets: {e.Message}");
                }
            }
        }
        
        private static void SavePresets()
        {
            try
            {
                string directory = Path.GetDirectoryName(PRESETS_PATH);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                string json = JsonUtility.ToJson(presetCollection, true);
                File.WriteAllText(PRESETS_PATH, json);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save AutoScaler presets: {e.Message}");
            }
        }
    }
}
#endif