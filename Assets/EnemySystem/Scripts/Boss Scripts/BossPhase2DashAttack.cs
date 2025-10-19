using UnityEngine;

public class BossPhase2DashAttack : MonoBehaviour
{
    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private float dashDuration = 3f;
    [SerializeField] private float dashDamage = 10f;  
    [SerializeField] private Transform objectsContainer;  
    [SerializeField] private LayerMask wallLayerMask;   
    [SerializeField] private int maxWallHits = 3;     
    [SerializeField] private float playerKnockbackForce = 5f;  
    [SerializeField] private float dashHitRadius = 1f;    
    [SerializeField] private LayerMask playerLayerMask;       
    [SerializeField] private Animator animator;            
    [SerializeField] private SpriteRenderer spriteRenderer;  

    private Rigidbody2D rb;
    private BossStatsMovement bossStats;
    private Transform player;
    private bool isDashing;
    private float dashTimer;
    private Vector2 dashDirection;
    private int wallHitCount;                         
    private int debugDashCount; 
    private int debugWallHitCount;  
    private bool hasHitPlayer;                            

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        bossStats = GetComponent<BossStatsMovement>();
        player = GameObject.FindWithTag("Player")?.transform;

        if (objectsContainer == null)
            objectsContainer = GameObject.Find("Objects")?.transform;

        wallLayerMask = LayerMask.GetMask("Wall");

        if (playerLayerMask.value == 0)
            playerLayerMask = LayerMask.GetMask("Player");

        if (animator == null)
            animator = GetComponent<Animator>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public void PerformAttack()
    {
        if (isDashing) return;
        StartDash();
    }

    private void StartDash()
    {
        debugDashCount++;  
        Debug.Log($"BossPhase2DashAttack: performed dash #{debugDashCount}");
        isDashing = true;
        dashTimer = dashDuration;
        hasHitPlayer = false;  // reset per-dash

        if (player == null)
            player = GameObject.FindWithTag("Player")?.transform;

        dashDirection = player != null
            ? ((Vector2)(player.position - transform.position)).normalized
            : Vector2.right;


        if (spriteRenderer != null)

            spriteRenderer.flipX = dashDirection.x > 0;

        if (animator != null)
        {
            if (Mathf.Abs(dashDirection.x) > Mathf.Abs(dashDirection.y))
                animator.SetTrigger("DashSide");
            else
                animator.SetTrigger("DashFront");
        }

        rb.linearVelocity = dashDirection * dashSpeed;
    }

    void Update()
    {
        if (!isDashing) return;

        dashTimer -= Time.deltaTime;
        if (dashTimer <= 0f)
            EndDash();
        else
            rb.linearVelocity = dashDirection * dashSpeed;  // maintain straight‐line velocity

        TryHitPlayer();  // manual overlap check
    }

    private void TryHitPlayer()
    {
        if (hasHitPlayer) return;
        Collider2D hit = Physics2D.OverlapCircle(transform.position, dashHitRadius, playerLayerMask);
        if (hit != null)
        {
            hasHitPlayer = true;
            // damage
            hit.GetComponent<PlayerController>()?.TakeDamage(Mathf.CeilToInt(dashDamage));
            // knockback
            var rb2d = hit.GetComponent<Rigidbody2D>();
            if (rb2d != null)
            {
                Vector2 dir = (hit.transform.position - transform.position).normalized;
                rb2d.AddForce(dir * playerKnockbackForce, ForceMode2D.Impulse);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (!isDashing) return;

        if (((1 << col.gameObject.layer) & wallLayerMask) != 0)
        {
            debugWallHitCount++;
            Debug.Log($"BossPhase2DashAttack: wall collision #{debugWallHitCount}/{maxWallHits}");
            wallHitCount++;
            if (wallHitCount >= maxWallHits)
            {
                Debug.Log("BossPhase2DashAttack: boss stunned due to wall hits");
                EndDash();
                bossStats.ChangeState(BossStatsMovement.BossState.Stunned);
                wallHitCount = 0;
            }
            return;
        }

        if (objectsContainer != null && col.transform.IsChildOf(objectsContainer))
        {
            Destroy(col.gameObject);
            return;
        }

        if (col.collider.CompareTag("Player"))
        {
            // now handled via TryHitPlayer(), so leave empty
        }
    }

    private void EndDash()
    {
        isDashing = false;
        rb.linearVelocity = Vector2.zero;
        animator.CrossFade("Idle", 0f);
    }
    
    public bool IsDashing { get { return isDashing; } }
}
