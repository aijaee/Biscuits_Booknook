using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.OnScreen;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerController))]
public class DashController : MonoBehaviour
{
    [Header("Dash Settings")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    [Header("Input")]
    public OnScreenButton onScreenDashButton;

    [Header("References")]
    public Rigidbody2D rb;
    public Animator animator;
    public JoystickPlayerExample movementScript;

    private bool isDashing = false;
    private float lastDashTime = -Mathf.Infinity;
    private Vector2 dashDirection;

    private PlayerController playerController;

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (playerController == null)
            playerController = GetComponent<PlayerController>();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (PlayerPrefs.HasKey("DashCooldown"))
            dashCooldown = PlayerPrefs.GetFloat("DashCooldown");
    }

    private void Update()
    {
        // On-screen button
        if (onScreenDashButton != null &&
            (onScreenDashButton.control as ButtonControl)?.isPressed == true)
        {
            TryDash();
        }

        // Keyboard input
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            TryDash();
        }
    }

    private void TryDash()
    {
        if (Time.time - lastDashTime >= dashCooldown && !isDashing)
        {
            dashDirection = movementScript != null ? movementScript.GetLastDirection() : Vector2.right;

            if (dashDirection.sqrMagnitude <= 0.01f)
                dashDirection = Vector2.right;

            StartCoroutine(PerformDash());
        }
    }

    private IEnumerator PerformDash()
    {
        isDashing = true;
        lastDashTime = Time.time;

        // Apply invincibility during dash
        if (playerController != null)
            playerController.SetInvincible(true);

        if (movementScript != null)
            movementScript.isDashing = true;

        if (animator != null)
            animator.SetBool("IsDashing", true);

        float startTime = Time.time;

        while (Time.time < startTime + dashDuration)
        {
            rb.linearVelocity = dashDirection * dashSpeed;
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;
        isDashing = false;

        if (movementScript != null)
            movementScript.isDashing = false;

        if (animator != null)
            animator.SetBool("IsDashing", false);

        // Remove invincibility after dash ends
        if (playerController != null)
            playerController.SetInvincible(false);
    }
}
