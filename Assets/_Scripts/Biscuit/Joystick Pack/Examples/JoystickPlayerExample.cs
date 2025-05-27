using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoystickPlayerExample : MonoBehaviour
{
    public float speed;
    public VariableJoystick variableJoystick;
    public Rigidbody2D rb;
    public Animator animator; // Reference to the Animator component
    private const float Tolerance = 0.01f; // Tolerance for float comparisons
    private SpriteRenderer spriteRenderer;
    private Vector2 lastDirection = Vector2.down;

    public void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void FixedUpdate()
    {
        Vector2 direction = new Vector2(variableJoystick.Horizontal, variableJoystick.Vertical);
        rb.linearVelocity = direction * speed;

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = (direction.x < -0.45f);
        }

        if (direction.sqrMagnitude > Tolerance)
        {
            // Player is moving
            animator.SetFloat("Horizontal", direction.x);
            animator.SetFloat("Vertical", direction.y);
            animator.SetFloat("Speed", direction.sqrMagnitude);

            lastDirection = direction.normalized; // Track direction for idle state
        }
        else
        {
            // Player is idle – use last facing direction
            animator.SetFloat("Speed", 0f);
            animator.SetFloat("Horizontal", lastDirection.x);
            animator.SetFloat("Vertical", lastDirection.y);
        }
    }
}