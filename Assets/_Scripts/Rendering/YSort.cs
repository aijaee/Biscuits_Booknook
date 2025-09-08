using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class YSort : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        // Multiply by -100 so lower Y = higher sorting order
        spriteRenderer.sortingOrder = -(int)(transform.position.y * 100);
    }
}
