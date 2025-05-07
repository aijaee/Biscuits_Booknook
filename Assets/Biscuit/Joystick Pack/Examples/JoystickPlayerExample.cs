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

    private void FixedUpdate()
    {
        Vector2 direction = new Vector2(variableJoystick.Horizontal, variableJoystick.Vertical);
        rb.linearVelocity = direction * speed;

        // Update animation parameters
        animator.SetFloat("Horizontal", direction.x);
        Debug.Log($"Set Horizontal to {direction.x}");

        animator.SetFloat("Vertical", direction.y);
        Debug.Log($"Set Vertical to {direction.y}");

        animator.SetFloat("Speed", direction.sqrMagnitude);
        Debug.Log($"Set Speed to {direction.sqrMagnitude}");

        // Use tolerance to check if the player is idle
        if (direction.sqrMagnitude < Tolerance)
        {
            animator.SetFloat("Speed", 0f); // Player is idle
        }
        else
        {
            animator.SetFloat("Speed", direction.sqrMagnitude); // Player is moving
        }
    }
}