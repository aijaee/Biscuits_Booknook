using System.Collections;
using UnityEngine;

public class JoystickPlayerExample : MonoBehaviour
{
    public float speed;
    public VariableJoystick variableJoystick;
    public Rigidbody2D rb;
    public Animator animator;
    public bool isDashing = false;  // Flag to disable movement during dash

    private const float Tolerance = 0.01f;
    private SpriteRenderer spriteRenderer;
    private Vector2 lastDirection = Vector2.down;
    public MeleeAttackController meleeAttackController; // Reference to MeleeAttack


    private void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void FixedUpdate()
    {
        Vector2 direction = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );

        //Debug.Log($"Input direction: {direction}, lastDirection before update: {lastDirection}");

        if (!isDashing)
        {
            rb.linearVelocity = direction * speed;

            if (spriteRenderer != null)
                spriteRenderer.flipX = (direction.x < -0.45f);

            if (direction.sqrMagnitude > Tolerance)
            {
                animator.SetFloat("Horizontal", direction.x);
                animator.SetFloat("Vertical", direction.y);
                animator.SetFloat("Speed", direction.sqrMagnitude);

                lastDirection = direction.normalized;
                //Debug.Log($"Updated lastDirection to {lastDirection}");
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
            animator.SetFloat("Speed", 0f);
            animator.SetFloat("Horizontal", lastDirection.x);
            animator.SetFloat("Vertical", lastDirection.y);

            if (spriteRenderer != null)
                spriteRenderer.flipX = (lastDirection.x < -0.45f);
        }

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
}
