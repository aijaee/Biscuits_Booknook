using UnityEngine;

public class YSort : MonoBehaviour
{
    [Tooltip("Optional: Override the pivot (e.g. Shadow). If left empty, auto-detected or fallback to self.")]
    public Transform pivot;

    private SpriteRenderer sr;

    void Awake()
    {
        // Auto-detect a pivot if none is assigned
        if (pivot == null)
        {
            Transform shadow = transform.Find("Shadow");
            if (shadow != null)
                pivot = shadow; // Use shadow if available
            else
                pivot = transform; // Otherwise, use this object
        }

        // Try to find the renderer on this object
        sr = GetComponent<SpriteRenderer>();

        // If not found, look in children (covers cases like Fish → Body)
        if (sr == null)
            sr = GetComponentInChildren<SpriteRenderer>();
    }

    void LateUpdate()
    {
        if (sr != null && pivot != null)
        {
            sr.sortingOrder = -(int)(pivot.position.y * 100);
        }
    }
}
