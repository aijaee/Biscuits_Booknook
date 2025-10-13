using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class ProjectileAOE : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 8f;
    public float lifetime = 5f;

    [Header("AOE Impact")]
    public float aoeRadius = 2f;
    public int damage = 20;
    public LayerMask playerLayerMask;
    public GameObject impactEffect; 

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    // call this right after Instantiate if you want to set direction
    public void Launch(Vector2 direction)
    {
        rb.linearVelocity = direction.normalized * speed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Explode();
    }

    private void Explode()
    {
        if (impactEffect != null)
            Instantiate(impactEffect, transform.position, Quaternion.identity);

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, aoeRadius, playerLayerMask);
        foreach (var h in hits)
            h.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aoeRadius);
    }
}
