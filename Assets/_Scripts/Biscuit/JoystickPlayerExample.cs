using System.Collections;
using UnityEngine;

public class JoystickPlayerExample : MonoBehaviour
{
    [Header("Movement")]
    public float baseSpeed = 5f;   // Normal movement speed
    private float currentSpeed;    // Modified speed (e.g., slowed in zones)
    public Rigidbody2D rb;
    public bool isDashing = false;  // Disable movement during dash

    [Header("Animation")]
    public Animator animator;
    private SpriteRenderer spriteRenderer;
    private Vector2 lastDirection = Vector2.down;
    private const float Tolerance = 0.01f;

    [Header("Combat")]
    public MeleeAttackController meleeAttackController; // Reference to melee attack controller

    private void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        currentSpeed = baseSpeed;
    }

    private void FixedUpdate()
    {
        Vector2 direction = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );

        if (!isDashing)
        {
            // Normalize direction to avoid diagonal speed boost
            rb.linearVelocity = direction.normalized * currentSpeed;

            if (spriteRenderer != null)
                spriteRenderer.flipX = (direction.x < -0.45f);

            if (direction.sqrMagnitude > Tolerance)
            {
                animator.SetFloat("Horizontal", direction.x);
                animator.SetFloat("Vertical", direction.y);
                animator.SetFloat("Speed", direction.sqrMagnitude);

                lastDirection = direction.normalized;
            }
            else
            {
                animator.SetFloat("Speed", 0f);
                animator.SetFloat("Horizontal", lastDirection.x);
                animator.SetFloat("Vertical", lastDirection.y);
            }
        }
        else
        {
            // Lock animations to last facing direction during dash
            animator.SetFloat("Speed", 0f);
            animator.SetFloat("Horizontal", lastDirection.x);
            animator.SetFloat("Vertical", lastDirection.y);

            if (spriteRenderer != null)
                spriteRenderer.flipX = (lastDirection.x < -0.45f);
        }

        // Pass direction to melee controller
        if (meleeAttackController != null)
        {
            meleeAttackController.UpdateMovementDirection(lastDirection);
        }
        else
        {
            Debug.LogWarning("MeleeAttackController reference is null!");
        }
    }

    public Vector2 GetLastDirection()
    {
        return lastDirection;
    }

    // --- SlowZone support ---
    public void ModifySpeed(float factor)
    {
        currentSpeed = baseSpeed * factor;
    }

    public void ResetSpeed()
    {
        currentSpeed = baseSpeed;
    }
}
