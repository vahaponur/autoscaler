#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Vahaponur.AutoScaler
{
    [InitializeOnLoad]
    public static class AutoScalerReferenceVisualizer
    {
        private static bool showReferences = false;
        private const string SHOW_REFERENCES_KEY = "AutoScaler_ShowReferences";
        
        private class ReferenceObject
        {
            public string name;
            public float height;
            public Color color;
            public PrimitiveType primitiveType;
            public Vector3 scale;
            
            public ReferenceObject(string name, float height, Color color, PrimitiveType type, Vector3 scale)
            {
                this.name = name;
                this.height = height;
                this.color = color;
                this.primitiveType = type;
                this.scale = scale;
            }
        }
        
        private static readonly List<ReferenceObject> references = new List<ReferenceObject>
        {
            new ReferenceObject("Human (1.8m)", 1.8f, new Color(0.2f, 0.8f, 0.2f, 0.3f), 
                PrimitiveType.Capsule, new Vector3(0.5f, 0.9f, 0.5f)),
                
            new ReferenceObject("10-Story Building (30m)", 30f, new Color(0.5f, 0.5f, 0.5f, 0.3f), 
                PrimitiveType.Cube, new Vector3(10f, 30f, 10f)),
                
            new ReferenceObject("Car (4.5m)", 4.5f, new Color(0.2f, 0.2f, 0.8f, 0.3f), 
                PrimitiveType.Cube, new Vector3(2f, 1.5f, 4.5f)),
                
            new ReferenceObject("Bus (12m)", 12f, new Color(0.8f, 0.8f, 0.2f, 0.3f), 
                PrimitiveType.Cube, new Vector3(2.5f, 3f, 12f)),
                
            new ReferenceObject("Pistol (0.3m)", 0.3f, new Color(0.3f, 0.3f, 0.3f, 0.3f), 
                PrimitiveType.Cylinder, new Vector3(0.05f, 0.3f, 0.05f)),
                
            new ReferenceObject("Sword (1m)", 1f, new Color(0.7f, 0.7f, 0.7f, 0.3f), 
                PrimitiveType.Cube, new Vector3(0.1f, 1f, 0.02f))
        };
        
        static AutoScalerReferenceVisualizer()
        {
            showReferences = EditorPrefs.GetBool(SHOW_REFERENCES_KEY, false);
            SceneView.duringSceneGui += OnSceneGUI;
        }
        
        [MenuItem("Tools/AutoScaler/Toggle Reference Objects", priority = 20)]
        private static void ToggleReferences()
        {
            showReferences = !showReferences;
            EditorPrefs.SetBool(SHOW_REFERENCES_KEY, showReferences);
            SceneView.RepaintAll();
        }
        
        [MenuItem("Tools/AutoScaler/Toggle Reference Objects", true)]
        private static bool ToggleReferencesValidate()
        {
            Menu.SetChecked("Tools/AutoScaler/Toggle Reference Objects", showReferences);
            return true;
        }
        
        private static void OnSceneGUI(SceneView sceneView)
        {
            if (!showReferences) return;
            
            // Get selected objects with AutoScaler
            var selected = Selection.activeGameObject;
            if (selected == null) return;
            
            var scaler = selected.GetComponent<global::AutoScaler>();
            if (scaler == null) return;
            
            // Get object bounds
            Bounds bounds = new Bounds(selected.transform.position, Vector3.zero);
            var renderers = scaler.includeChildren ? 
                selected.GetComponentsInChildren<Renderer>() : 
                new[] { selected.GetComponent<Renderer>() };
                
            if (renderers.Length == 0 || renderers[0] == null) return;
            
            bounds = renderers[0].bounds;
            foreach (var r in renderers)
            {
                if (r != null) bounds.Encapsulate(r.bounds);
            }
            
            // Draw reference objects
            Handles.BeginGUI();
            
            // Draw legend
            GUILayout.BeginArea(new Rect(10, 10, 200, 150));
            GUILayout.BeginVertical("box");
            GUILayout.Label("Reference Objects", EditorStyles.boldLabel);
            
            foreach (var reference in references)
            {
                GUILayout.BeginHorizontal();
                
                // Color box
                var colorRect = GUILayoutUtility.GetRect(16, 16);
                EditorGUI.DrawRect(colorRect, reference.color);
                
                GUILayout.Label($"{reference.name}", GUILayout.Width(150));
                GUILayout.EndHorizontal();
            }
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
            
            Handles.EndGUI();
            
            // Draw 3D reference objects
            foreach (var reference in references)
            {
                DrawReferenceObject(bounds.center, reference);
            }
        }
        
        private static void DrawReferenceObject(Vector3 centerPos, ReferenceObject reference)
        {
            Handles.color = reference.color;
            
            // Position reference object next to the selected object
            Vector3 offset = Vector3.right * (references.IndexOf(reference) * 5f + 5f);
            Vector3 position = centerPos + offset;
            position.y = reference.scale.y / 2f; // Place on ground
            
            Matrix4x4 oldMatrix = Handles.matrix;
            Handles.matrix = Matrix4x4.TRS(position, Quaternion.identity, reference.scale);
            
            // Draw primitive
            switch (reference.primitiveType)
            {
                case PrimitiveType.Cube:
                    Handles.DrawWireCube(Vector3.zero, Vector3.one);
                    break;
                case PrimitiveType.Capsule:
                    // Draw capsule using cylinders and spheres
                    DrawWireCapsule();
                    break;
                case PrimitiveType.Cylinder:
                    // Draw cylinder approximation
                    DrawWireCylinder();
                    break;
            }
            
            Handles.matrix = oldMatrix;
            
            // Draw label
            Handles.Label(position + Vector3.up * (reference.scale.y + 1f), 
                reference.name, EditorStyles.whiteBoldLabel);
        }
        
        private static void DrawWireCapsule()
        {
            // Simple capsule approximation
            float radius = 0.5f;
            float height = 1f - radius * 2f;
            
            // Top sphere
            Handles.DrawWireArc(Vector3.up * height/2f, Vector3.forward, Vector3.right, 360, radius);
            Handles.DrawWireArc(Vector3.up * height/2f, Vector3.right, Vector3.forward, 360, radius);
            
            // Bottom sphere
            Handles.DrawWireArc(Vector3.down * height/2f, Vector3.forward, Vector3.right, 360, radius);
            Handles.DrawWireArc(Vector3.down * height/2f, Vector3.right, Vector3.forward, 360, radius);
            
            // Connecting lines
            Handles.DrawLine(Vector3.right * radius + Vector3.up * height/2f, 
                            Vector3.right * radius - Vector3.up * height/2f);
            Handles.DrawLine(-Vector3.right * radius + Vector3.up * height/2f, 
                            -Vector3.right * radius - Vector3.up * height/2f);
            Handles.DrawLine(Vector3.forward * radius + Vector3.up * height/2f, 
                            Vector3.forward * radius - Vector3.up * height/2f);
            Handles.DrawLine(-Vector3.forward * radius + Vector3.up * height/2f, 
                            -Vector3.forward * radius - Vector3.up * height/2f);
        }
        
        private static void DrawWireCylinder()
        {
            float radius = 0.5f;
            float height = 1f;
            
            // Top circle
            Handles.DrawWireArc(Vector3.up * height/2f, Vector3.up, Vector3.forward, 360, radius);
            
            // Bottom circle
            Handles.DrawWireArc(Vector3.down * height/2f, Vector3.up, Vector3.forward, 360, radius);
            
            // Connecting lines
            Handles.DrawLine(Vector3.right * radius + Vector3.up * height/2f, 
                            Vector3.right * radius - Vector3.up * height/2f);
            Handles.DrawLine(-Vector3.right * radius + Vector3.up * height/2f, 
                            -Vector3.right * radius - Vector3.up * height/2f);
            Handles.DrawLine(Vector3.forward * radius + Vector3.up * height/2f, 
                            Vector3.forward * radius - Vector3.up * height/2f);
            Handles.DrawLine(-Vector3.forward * radius + Vector3.up * height/2f, 
                            -Vector3.forward * radius - Vector3.up * height/2f);
        }
    }
}
#endif