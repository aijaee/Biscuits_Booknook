using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BossStatsMovement : MonoBehaviour
{
    public enum BossState { INTRO, MOVING, CASTING, DASHING, STUNNED }

    // new: current state
    private BossState state = BossState.INTRO;

    [Header("Stats")]
    public float maxHealth = 200f;
    public float moveSpeed = 3f;
    public float visionRange = 10f;

    private float currentHealth;
    private Vector3 originalPosition;

    private Transform player;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        originalPosition = transform.position;
    }

    void Update()
    {
        if (currentHealth <= 0f) return;
        
        if (player == null)
        {
            var pgo = GameObject.FindWithTag("Player");
            if (pgo) player = pgo.transform;
        }

        // Decide where to move
        Vector3 targetPos = originalPosition;
        if (player != null && Vector3.Distance(transform.position, player.position) <= visionRange)
            targetPos = player.position;

        // Direct movement toward target
        Vector3 offset = targetPos - transform.position;
        if (offset.sqrMagnitude > 0.01f)
        {
            Vector2 dir = offset.normalized;
            rb.linearVelocity = dir * moveSpeed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    // Call to apply damage
    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            Die();
        }
    }

    private void Die()
    {
        rb.linearVelocity = Vector2.zero;
        // ...existing death logic or notify BossController...
    }

    // For debugging
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);
    }
}