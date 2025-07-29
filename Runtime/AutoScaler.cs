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
    [Tooltip("En büyük eksen kaç metre olsun?")]
    public float targetSize = 1f;
    
    [Tooltip("Alt objelerin renderer'larını da dahil et")]
    public bool includeChildren = true;
    
    [Header("Scale Anchor")]
    [Tooltip("Scale sırasında sabit kalacak nokta")]
    public PivotPreset scaleAnchor = PivotPreset.Center;
    
    [Tooltip("Scale anchor offset (0-1 range)")]
    public Vector3 anchorOffset = new Vector3(0.5f, 0.5f, 0.5f);

#if UNITY_EDITOR
    [HideInInspector] public Vector3 debugSize;   // Yalnızca editörde gösterilecek
#endif

    [ContextMenu("Fit To Target")]
    public void FitToTarget()
    {
        if (targetSize <= 0) return;

        Bounds b = GetCombinedBounds();
        float biggest = Mathf.Max(b.size.x, b.size.y, b.size.z);
        if (biggest < 1e-5f) return;

        // Store world position before scaling
        Vector3 worldPosBefore = transform.position;
        
        // Calculate pivot offset based on preset
        Vector3 pivotOffset = GetPivotOffset();
        
        // Calculate the world position of the desired pivot point before scaling
        Vector3 pivotWorldPos = b.min + Vector3.Scale(b.size, pivotOffset);
        
        // Apply scale
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
        
        Bounds currentBounds = GetCombinedBounds();
        float currentLargest = Mathf.Max(currentBounds.size.x, currentBounds.size.y, currentBounds.size.z);
        
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
        debugSize = b.size;           // metre cinsinden gerçek boyut
#endif
        return b;
    }

#if UNITY_EDITOR
    private void OnValidate() => FitToTarget();

    // Sahnede sarı kutu
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
            float currentLargest = Mathf.Max(b.size.x, b.size.y, b.size.z);
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
