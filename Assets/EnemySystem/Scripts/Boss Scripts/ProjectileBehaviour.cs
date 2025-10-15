using UnityEngine;

public class ProjectileBehaviour : MonoBehaviour
{
    // outgoing vs returning tint
    [SerializeField] private Color outgoingColor = Color.white;
    [SerializeField] private Color returningColor = Color.red;

    // runtime data
    Transform bossTransform;
    Transform playerTransform;
    float speed, returnSpeed, damage, maxDistance;
    Vector3 spawnPosition;
    bool returning;
    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;

    // assign this in Inspector to the “Wall” layer
    [SerializeField] private LayerMask wallLayerMask;

    // call immediately after Instantiate()
    public void Init(Transform owner, Transform player, float speed, float returnSpeed, float damage, float maxDistance)
    {
        bossTransform   = owner;
        playerTransform = player;
        this.speed      = speed;
        this.returnSpeed= returnSpeed;
        this.damage     = damage;
        this.maxDistance= maxDistance;

        // ensure Rigidbody2D
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
        }
        // ensure continuous detection so fast bullets don't tunnel
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // always ensure a trigger collider
        var col = GetComponent<CircleCollider2D>() ?? gameObject.AddComponent<CircleCollider2D>();
        col.isTrigger = true;

        // get or add a SpriteRenderer for tinting
        spriteRenderer = GetComponent<SpriteRenderer>() ?? gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.color = outgoingColor;

        // launch at player
        Vector2 dir = (playerTransform.position - transform.position).normalized;
        rb.linearVelocity = dir * speed;       // use velocity
        spawnPosition = transform.position;

        // rotate sprite to face player
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void Update()
    {
        if (Vector3.Distance(spawnPosition, transform.position) > maxDistance)
            Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null) return;

        // reflect
        if (!returning && other.GetComponent<Reflector>() != null)
        {
            returning = true;
            if (spriteRenderer != null)
                spriteRenderer.color = returningColor;

            Vector2 back = (bossTransform.position - transform.position).normalized;
            rb.linearVelocity = back * returnSpeed;  // use velocity

            // rotate to face boss
            float angle = Mathf.Atan2(back.y, back.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
            return;
        }

        // hit player outbound
        if (!returning && other.CompareTag("Player"))
        {
            other.GetComponent<PlayerController>()?
                 .TakeDamage(Mathf.CeilToInt(damage));
            Destroy(gameObject);
            return;
        }

        // return to boss
        if (returning && other.transform == bossTransform)
        {
            bossTransform.GetComponent<BossStatsMovement>()?
                         .TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // destroy when hitting objects on the Wall layer
        if (((1 << other.gameObject.layer) & wallLayerMask) != 0)
        {
            Destroy(gameObject);
        }
    }
}
