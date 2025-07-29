using UnityEngine;

[ExecuteInEditMode]
[DisallowMultipleComponent]
[AddComponentMenu("Utils/Auto Scaler")]
public class AutoScaler : MonoBehaviour
{
    [Tooltip("En büyük eksen kaç metre olsun?")]
    public float targetSize = 1f;
    
    [Tooltip("Alt objelerin renderer'larını da dahil et")]
    public bool includeChildren = true;

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

        float factor = targetSize / biggest;
        transform.localScale *= factor;
    }

    // ----------------------------- helpers
    Bounds GetCombinedBounds()
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
        Gizmos.color = Color.yellow;
        Bounds b = GetCombinedBounds();
        Gizmos.DrawWireCube(b.center, b.size);
    }
#endif
}
