#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Vahaponur.AutoScaler
{
    public class AutoScalerSettings : ScriptableObject
    {
        private const string SETTINGS_FOLDER = "Assets/AutoScalerSettings";
        private const string SETTINGS_PATH = "Assets/AutoScalerSettings/AutoScalerSettings.asset";
        private const string JSON_PATH = "ProjectSettings/AutoScalerSettings.json";
        
        [System.Serializable]
        private class SettingsData
        {
            public bool autoAttachEnabled = true;
        }
        
        private static bool? cachedAutoAttachEnabled;
        
        public static bool AutoAttachEnabled
        {
            get
            {
                if (!cachedAutoAttachEnabled.HasValue)
                {
                    LoadSettings();
                }
                return cachedAutoAttachEnabled.Value;
            }
            set
            {
                cachedAutoAttachEnabled = value;
                SaveSettings();
            }
        }
        
        private static void LoadSettings()
        {
            if (File.Exists(JSON_PATH))
            {
                try
                {
                    string json = File.ReadAllText(JSON_PATH);
                    var data = JsonUtility.FromJson<SettingsData>(json);
                    cachedAutoAttachEnabled = data.autoAttachEnabled;
                }
                catch
                {
                    cachedAutoAttachEnabled = true; // Default value
                }
            }
            else
            {
                cachedAutoAttachEnabled = true; // Default value
            }
        }
        
        private static void SaveSettings()
        {
            var data = new SettingsData { autoAttachEnabled = cachedAutoAttachEnabled.Value };
            string json = JsonUtility.ToJson(data, true);
            
            // Ensure ProjectSettings directory exists
            string directory = Path.GetDirectoryName(JSON_PATH);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            File.WriteAllText(JSON_PATH, json);
        }
    }
    
    public static class AutoScalerMenu
    {
        private const string MENU_PATH = "Tools/AutoScaler/Auto-Attach Enabled";
        private const string GITIGNORE_MENU_PATH = "Tools/AutoScaler/Add Settings to .gitignore";
        
        [MenuItem(MENU_PATH, priority = 1)]
        private static void ToggleAutoAttach()
        {
            AutoScalerSettings.AutoAttachEnabled = !AutoScalerSettings.AutoAttachEnabled;
        }
        
        [MenuItem(MENU_PATH, true)]
        private static bool ToggleAutoAttachValidate()
        {
            Menu.SetChecked(MENU_PATH, AutoScalerSettings.AutoAttachEnabled);
            return true;
        }
        
        [MenuItem(GITIGNORE_MENU_PATH, priority = 2)]
        private static void AddToGitIgnore()
        {
            string gitignorePath = ".gitignore";
            string lineToAdd = "ProjectSettings/AutoScalerSettings.json";
            
            if (!File.Exists(gitignorePath))
            {
                Debug.LogWarning("No .gitignore file found in project root. Please create one manually if needed.");
                return;
            }
            
            // Check if already exists
            string content = File.ReadAllText(gitignorePath);
            if (content.Contains(lineToAdd))
            {
                Debug.Log("AutoScalerSettings.json is already in .gitignore");
                return;
            }
            
            // Add to gitignore
            File.AppendAllText(gitignorePath, "\n" + lineToAdd + "\n");
            Debug.Log("Added AutoScalerSettings.json to .gitignore");
            
            // Refresh AssetDatabase to show changes
            AssetDatabase.Refresh();
        }
    }
}
#endif