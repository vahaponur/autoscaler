using UnityEngine;

public enum PivotPreset
{
    None,
    BottomCenter,
    Center,
    TopCenter,
    Custom
}

[ExecuteInEditMode]
[DisallowMultipleComponent]
[AddComponentMenu("Utils/Auto Scaler")]
[HelpURL("https://github.com/vahaponur/autoscaler")]
public class AutoScaler : MonoBehaviour
{
    [Header("Preset")]
    [Tooltip("Quick size presets")]
    public string selectedPresetName = "None";
    
    [Header("Manual Settings")]
    [Tooltip("Target size for the largest axis in meters")]
    public float targetSize = 1f;
    
    [Tooltip("Include renderers from child objects")]
    public bool includeChildren = true;
    
    [Header("Scale Anchor")]
    [Tooltip("Anchor point that remains fixed during scaling")]
    public PivotPreset scaleAnchor = PivotPreset.Center;
    
    [Tooltip("Scale anchor offset (0-1 range)")]
    public Vector3 anchorOffset = new Vector3(0.5f, 0.5f, 0.5f);

#if UNITY_EDITOR
    [HideInInspector] public Vector3 debugSize;   // Only visible in editor
#endif

    [ContextMenu("Fit To Target")]
    public void FitToTarget()
    {
        if (targetSize <= 0) return;

        // Use local bounds for rotation-independent scaling
        Bounds localBounds = GetLocalBounds();
        float biggest = Mathf.Max(localBounds.size.x, localBounds.size.y, localBounds.size.z);
        if (biggest < 1e-5f) return;

        // Store world position before scaling
        Vector3 worldPosBefore = transform.position;
        
        // Calculate pivot offset based on preset
        Vector3 pivotOffset = GetPivotOffset();
        
        // Get world bounds for pivot calculation
        Bounds worldBounds = GetCombinedBounds();
        
        // Calculate the world position of the desired pivot point before scaling
        Vector3 pivotWorldPos = worldBounds.min + Vector3.Scale(worldBounds.size, pivotOffset);
        
        // Apply scale based on local bounds (rotation-independent)
        float factor = targetSize / biggest;
        transform.localScale *= factor;
        
        // If scale anchor is enabled, reposition to keep anchor point in place
        if (scaleAnchor != PivotPreset.None)
        {
            // Get new bounds after scaling
            Bounds newBounds = GetCombinedBounds();
            
            // Calculate where the pivot point is now
            Vector3 newPivotWorldPos = newBounds.min + Vector3.Scale(newBounds.size, pivotOffset);
            
            // Calculate the offset needed to put the pivot back where it was
            Vector3 positionOffset = pivotWorldPos - newPivotWorldPos;
            
            // Apply the offset to maintain pivot position
            transform.position += positionOffset;
        }
    }
    
    Vector3 GetPivotOffset()
    {
        switch (scaleAnchor)
        {
            case PivotPreset.BottomCenter:
                return new Vector3(0.5f, 0f, 0.5f);
            case PivotPreset.Center:
                return new Vector3(0.5f, 0.5f, 0.5f);
            case PivotPreset.TopCenter:
                return new Vector3(0.5f, 1f, 0.5f);
            case PivotPreset.Custom:
                return anchorOffset;
            default:
                return Vector3.zero;
        }
    }

    // ----------------------------- helpers
    public bool IsScaleCorrect()
    {
        if (targetSize <= 0) return true;
        
        // Use local bounds to check scale (rotation-independent)
        Bounds localBounds = GetLocalBounds();
        float currentLargest = Mathf.Max(localBounds.size.x, localBounds.size.y, localBounds.size.z);
        
        // Check if current largest dimension matches target size (with 5% tolerance)
        float tolerance = targetSize * 0.05f;
        return Mathf.Abs(currentLargest - targetSize) < tolerance;
    }
    
    public Bounds GetCombinedBounds()
    {
        Renderer[] rends;
        
        if (includeChildren)
        {
            rends = GetComponentsInChildren<Renderer>();
        }
        else
        {
            var renderer = GetComponent<Renderer>();
            rends = renderer != null ? new[] { renderer } : new Renderer[0];
        }
        
        if (rends.Length == 0) return new Bounds(transform.position, Vector3.zero);

        Bounds b = rends[0].bounds;
        foreach (var r in rends) b.Encapsulate(r.bounds);

#if UNITY_EDITOR
        debugSize = b.size;           // actual size in meters
#endif
        return b;
    }
    
    public Bounds GetLocalBounds()
    {
        Renderer[] rends;
        
        if (includeChildren)
        {
            rends = GetComponentsInChildren<Renderer>();
        }
        else
        {
            var renderer = GetComponent<Renderer>();
            rends = renderer != null ? new[] { renderer } : new Renderer[0];
        }
        
        if (rends.Length == 0) return new Bounds(Vector3.zero, Vector3.zero);
        
        // Calculate bounds in local space
        Bounds localBounds = new Bounds(Vector3.zero, Vector3.zero);
        bool boundsInitialized = false;
        
        foreach (var r in rends)
        {
            if (r == null) continue;
            
            // Get mesh bounds for MeshRenderer
            if (r is MeshRenderer meshRenderer)
            {
                MeshFilter meshFilter = meshRenderer.GetComponent<MeshFilter>();
                if (meshFilter != null && meshFilter.sharedMesh != null)
                {
                    Bounds meshBounds = meshFilter.sharedMesh.bounds;
                    
                    // Transform mesh bounds to this object's local space
                    Vector3 min = meshBounds.min;
                    Vector3 max = meshBounds.max;
                    Vector3[] corners = new Vector3[]
                    {
                        new Vector3(min.x, min.y, min.z),
                        new Vector3(max.x, min.y, min.z),
                        new Vector3(min.x, max.y, min.z),
                        new Vector3(max.x, max.y, min.z),
                        new Vector3(min.x, min.y, max.z),
                        new Vector3(max.x, min.y, max.z),
                        new Vector3(min.x, max.y, max.z),
                        new Vector3(max.x, max.y, max.z)
                    };
                    
                    // Transform corners from mesh space to world space, then to our local space
                    foreach (var corner in corners)
                    {
                        Vector3 worldPoint = r.transform.TransformPoint(corner);
                        Vector3 localPoint = transform.InverseTransformPoint(worldPoint);
                        
                        if (!boundsInitialized)
                        {
                            localBounds = new Bounds(localPoint, Vector3.zero);
                            boundsInitialized = true;
                        }
                        else
                        {
                            localBounds.Encapsulate(localPoint);
                        }
                    }
                }
            }
            else if (r is SkinnedMeshRenderer skinnedRenderer)
            {
                // For skinned mesh renderer, use its local bounds
                Bounds meshBounds = skinnedRenderer.localBounds;
                
                // Transform bounds corners
                Vector3 min = meshBounds.min;
                Vector3 max = meshBounds.max;
                Vector3[] corners = new Vector3[]
                {
                    new Vector3(min.x, min.y, min.z),
                    new Vector3(max.x, min.y, min.z),
                    new Vector3(min.x, max.y, min.z),
                    new Vector3(max.x, max.y, min.z),
                    new Vector3(min.x, min.y, max.z),
                    new Vector3(max.x, min.y, max.z),
                    new Vector3(min.x, max.y, max.z),
                    new Vector3(max.x, max.y, max.z)
                };
                
                foreach (var corner in corners)
                {
                    Vector3 worldPoint = r.transform.TransformPoint(corner);
                    Vector3 localPoint = transform.InverseTransformPoint(worldPoint);
                    
                    if (!boundsInitialized)
                    {
                        localBounds = new Bounds(localPoint, Vector3.zero);
                        boundsInitialized = true;
                    }
                    else
                    {
                        localBounds.Encapsulate(localPoint);
                    }
                }
            }
        }
        
        // Apply current scale to get actual size
        Vector3 scaledSize = Vector3.Scale(localBounds.size, transform.localScale);
        return new Bounds(localBounds.center, scaledSize);
    }

#if UNITY_EDITOR
    private void OnValidate() => FitToTarget();

    // Yellow box in scene view
    private void OnDrawGizmosSelected()
    {
        // Check if scale matches expected
        bool scaleIncorrect = targetSize > 0 && !IsScaleCorrect();
        
        if (scaleIncorrect)
        {
            // Draw red warning box
            Gizmos.color = Color.red;
        }
        else
        {
            // Normal yellow box
            Gizmos.color = Color.yellow;
        }
        
        Bounds b = GetCombinedBounds();
        Gizmos.DrawWireCube(b.center, b.size);
        
        // Draw warning if scale doesn't match
        if (scaleIncorrect)
        {
            // Draw thick red outline
            Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
            Gizmos.DrawCube(b.center, b.size * 1.05f);
            
#if UNITY_EDITOR
            // Draw warning text in scene
            // Use local bounds for rotation-independent size
            Bounds localBounds = GetLocalBounds();
            float currentLargest = Mathf.Max(localBounds.size.x, localBounds.size.y, localBounds.size.z);
            UnityEditor.Handles.color = Color.red;
            UnityEditor.Handles.Label(b.center + Vector3.up * (b.size.y * 0.6f), 
                "⚠️ SIZE MISMATCH!\n" + 
                $"Target: {targetSize:F2}m\n" +
                $"Current: {currentLargest:F2}m", 
                new GUIStyle() 
                { 
                    fontSize = 12, 
                    fontStyle = FontStyle.Bold,
                    normal = new GUIStyleState() { textColor = Color.red },
                    alignment = TextAnchor.MiddleCenter
                });
#endif
        }
    }
#endif
}
