using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float visionRadius = 5f;
    public LayerMask playerLayerMask;

    [Header("Attack Settings")]
    public int attackDamage = 10;
    public float attackCooldown = 1.0f;
    public bool isStunned = false;
    private bool isKnockedBack = false;
    private Transform target;
    [HideInInspector] public AStarPathfinder pathfinder;
    private List<Vector3> path;
    private Transform playerTransform;
    private Vector3? lastPlayerPosition = null;
    private Vector3? lastPathTargetPosition = null;
    private Rigidbody2D rb;
    private Animator animator;

    private Vector3 originalPosition;
    private float lostSightTimer = 0f;
    private float lostSightThreshold = 3f;
    private bool returningToOrigin = false;

    private float lastAttackTime = -Mathf.Infinity;
    private PlayerController playerController;

    private void Awake()
    {
        pathfinder = GetComponent<AStarPathfinder>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // Store original spawn position
        originalPosition = transform.position;

        // Auto-assign GridManager to AStarPathfinder if not set
        if (pathfinder != null && pathfinder.gridManager == null)
        {
            pathfinder.gridManager = Object.FindFirstObjectByType<GridManager>();
            if (pathfinder.gridManager == null)
                Debug.LogError("EnemyController: No GridManager found in the scene!");
        }

        // Warn if Rigidbody2D is not Kinematic
        if (rb != null && rb.bodyType != RigidbodyType2D.Kinematic)
        {
            Debug.LogWarning($"{gameObject.name}: Rigidbody2D is not Kinematic. Set it to Kinematic for AI movement.");
        }

        // Find PlayerController in the scene (optional: cache for efficiency)
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            playerController = playerObj.GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (isStunned || isKnockedBack)
        {
            if (animator != null)
                animator.SetFloat("Speed", 0f);
            return;
        }

        if (CanSeePlayer())
        {
            lostSightTimer = 0f;
            returningToOrigin = false;

            if (playerTransform != null)
            {
                lastPlayerPosition = playerTransform.position;

                // Only recalculate path if player moved significantly
                bool shouldRepath = false;
                if (path == null || path.Count == 0)
                {
                    shouldRepath = true;
                }
                else if (lastPathTargetPosition == null || Vector3.Distance(lastPathTargetPosition.Value, playerTransform.position) > 0.5f)
                {
                    Vector3 currentPosition = transform.position;
                    currentPosition.z = 0;
                    Vector3 nextPosition = path[0];
                    nextPosition.z = 0;
                    if (Vector3.Distance(currentPosition, nextPosition) < 0.2f)
                    {
                        shouldRepath = true;
                    }
                }

                if (shouldRepath)
                {
                    SetTarget(playerTransform);
                    lastPathTargetPosition = playerTransform.position;
                }
            }
        }
        else
        {
            lostSightTimer += Time.deltaTime;

            // If lost sight but have a last known position, keep moving there
            if (!returningToOrigin && lastPlayerPosition != null &&
                (target == null || (target != null && Vector3.Distance(transform.position, lastPlayerPosition.Value) > 0.1f)))
            {
                if (lastPathTargetPosition == null ||
                    Vector3.Distance(lastPathTargetPosition.Value, lastPlayerPosition.Value) > 0.5f ||
                    path == null || path.Count == 0)
                {
                    SetTarget(null); // Clear direct target
                    if (pathfinder != null)
                    {
                        path = pathfinder.FindPath(transform.position, lastPlayerPosition.Value);
                        lastPathTargetPosition = lastPlayerPosition.Value;
                    }
                }
            }
            // If reached last known position, stop
            else if (!returningToOrigin && lastPlayerPosition != null &&
                    Vector3.Distance(transform.position, lastPlayerPosition.Value) <= 0.1f)
            {
                lastPlayerPosition = null;
                path = null;
                lastPathTargetPosition = null;
            }

            // After 3 seconds of not seeing the player, return to original position
            if (!returningToOrigin && lostSightTimer >= lostSightThreshold)
            {
                returningToOrigin = true;
                SetTarget(null);
                if (pathfinder != null)
                {
                    path = pathfinder.FindPath(transform.position, originalPosition);
                    lastPathTargetPosition = originalPosition;
                }
            }
        }

        // Follow path if available
        if (path != null && path.Count > 0)
        {
            MoveTowardsTarget();
        }

        // Update animation speed (MODIFIED)
        if (animator != null)
        {
            Vector3 velocity = rb != null ? rb.linearVelocity : Vector3.zero;
            animator.SetFloat("Speed", velocity.magnitude);
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (pathfinder != null && target != null)
        {
            path = pathfinder.FindPath(transform.position, target.position);
            if (path == null)
                Debug.LogWarning($"{gameObject.name}: Path is null after FindPath.");
            else if (path.Count == 0)
                Debug.LogWarning($"{gameObject.name}: Path is empty after FindPath.");
            else
                Debug.Log($"{gameObject.name}: Path set with {path.Count} waypoints. First: {path[0]}, Last: {path[path.Count-1]}");
        }
        else
        {
            path = null;
        }
    }

    private void MoveTowardsTarget()
    {
        if (path != null && path.Count > 0)
        {
            Vector3 nextPosition = path[0];
            nextPosition.z = 0;
            Vector3 currentPosition = transform.position;
            currentPosition.z = 0;

            Vector3 moveTo = Vector3.MoveTowards(currentPosition, nextPosition, moveSpeed * Time.deltaTime);

            // Prevent moving through walls: check for collision at moveTo
            Collider2D hit = Physics2D.OverlapCircle(moveTo, 0.2f, pathfinder.gridManager.unwalkableMask);
            if (hit != null)
            {
                Debug.Log($"{gameObject.name}: Blocked by wall at {moveTo}");
                path = null; // Stop path if blocked
                return;
            }

           // Debug.Log($"{gameObject.name} moving from {currentPosition} towards {nextPosition} (moveTo: {moveTo})");

            if (rb != null)
            {
                Vector2 direction = (nextPosition - currentPosition).normalized;
                rb.linearVelocity = direction * moveSpeed;
            }
            else
            {
                transform.position = Vector3.MoveTowards(currentPosition, nextPosition, moveSpeed * Time.deltaTime);
            }

            if (Vector3.Distance(currentPosition, nextPosition) < 0.1f)
            {
                path.RemoveAt(0);
                Debug.Log($"{gameObject.name} reached waypoint, {path.Count} waypoints left.");
            }
        }
        else
        {
            if (rb != null)
            rb.linearVelocity = Vector2.zero;
            Debug.Log($"{gameObject.name}: No path to follow or path is null.");
        }
    }

    public void Attack()
    {
        if (playerController != null && Time.time - lastAttackTime >= attackCooldown)
        {
            playerController.TakeDamage(attackDamage);
            lastAttackTime = Time.time;
        }
    }

    // Call this from Enemy_HealthAndDamage when taking damage
    // public void PlayDamagedAnimation()
    // {
    //     if (animator != null)
    //     {
    //         animator.SetBool("Damaged", true);
    //         StartCoroutine(ResetDamagedAfterDelay(0.5f));
    //     }
    // }

    public void SetKnockbackState(bool state)
    {
        isKnockedBack = state;
        if (state && rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    private System.Collections.IEnumerator ResetDamagedAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (animator != null)
        {
            animator.SetBool("Damaged", false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (playerController == null)
                playerController = collision.GetComponent<PlayerController>();
            Attack();
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (playerController == null)
                playerController = collision.GetComponent<PlayerController>();
            Attack();
        }
    }

    private bool CanSeePlayer()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, visionRadius, playerLayerMask);
        foreach (var hit in hits)
        {
            if (hit != null && hit.CompareTag("Player"))
            {
                playerTransform = hit.transform;
                return true;
            }
        }
        playerTransform = null;
        return false;
    }
}