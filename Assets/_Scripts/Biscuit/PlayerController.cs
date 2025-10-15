using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;

    [Header("UI")]
    public GameObject deathScreen; // Now public for spawner assignment
    public Image hpBarFiller; // Assign the Image component using filler.png

    [Header("Movement")]
    public float moveSpeed = 5f;
    private float originalMoveSpeed;

    [Header("Combat")]
    public int baseDamage = 1;
    private int currentDamage;
    private bool isInvincible = false;
    public float invincibilityDuration = 2f;

    private RectTransform hpBarRect; // Store the original rect for resizing

    private bool isDead = false;

    // NEW: reference to the separate effects handler
    public PlayerDamageEffects damageEffects;

    private void Awake()
    {
        currentHealth = maxHealth;
        if (deathScreen != null)
            deathScreen.SetActive(false);

        // Cache the RectTransform if using resize mode
        if (hpBarFiller != null)
            hpBarRect = hpBarFiller.GetComponent<RectTransform>();

        originalMoveSpeed = moveSpeed;
        currentDamage = baseDamage;

        // AUTO-GET the PlayerDamageEffects if not assigned in Inspector
        if (damageEffects == null)
            damageEffects = GetComponent<PlayerDamageEffects>();

        if (damageEffects == null)
            Debug.LogWarning("PlayerController: no PlayerDamageEffects component found/assigned on the player.");

        // ensure Player and Boss layers don’t collide
        Physics2D.IgnoreLayerCollision(
            LayerMask.NameToLayer("Player"),
            LayerMask.NameToLayer("Boss"),
            true
        );
    }

    public void TakeDamage(int damage)
    {
        if (isDead || isInvincible) return;

        currentHealth -= damage;
        if (currentHealth < 0f) currentHealth = 0f;
        Debug.Log("TakeDamage CALLED");

        UpdateHPBar();

        if (damageEffects != null)
        {
            Debug.Log("Calling PlayDamageEffects");
            damageEffects.PlayDamageEffects();
        }

        // Start invincibility
        StartCoroutine(InvincibilityCoroutine());

        if (currentHealth <= 0f && !isDead)
            Die();
    }

    public void SetInvincible(bool value)
    {
        isInvincible = value;
    }

    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        Debug.Log($"{gameObject.name} has died.");

        if (deathScreen != null)
        {
            deathScreen.SetActive(true);
        }
        else
        {
            Debug.LogWarning("DeathScreen GameObject not found in the scene!");
        }

        this.enabled = false;
        Destroy(gameObject);
    }

    public void UpdateHPBar()
    {
        if (hpBarFiller != null)
        {
            // Only set these once, not every frame, but safe here for runtime assignment
            if (hpBarFiller.type != Image.Type.Filled)
                hpBarFiller.type = Image.Type.Filled;
            if (hpBarFiller.fillMethod != Image.FillMethod.Horizontal)
                hpBarFiller.fillMethod = Image.FillMethod.Horizontal;
            if (hpBarFiller.fillOrigin != (int)Image.OriginHorizontal.Left)
                hpBarFiller.fillOrigin = (int)Image.OriginHorizontal.Left;

            hpBarFiller.fillAmount = Mathf.Clamp01(currentHealth / maxHealth);
        }
        else
        {
            Debug.LogWarning("hpBarFiller is not assigned on PlayerController.");
        }
    }

    public void Dash()
    {
        Debug.Log("Player dashes!");
        // Your dash logic here
    }

    public void PerformAttack()
    {
        Debug.Log("Player attacks!");
        // Your attack logic here
    }

    public void Heal(int amount)
    {
        if (isDead) return;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdateHPBar();
        Debug.Log($"{gameObject.name} healed {amount} HP. Current HP: {currentHealth}");
    }

    public void ApplySpeedBoost(float multiplier, float duration)
    {
        StartCoroutine(SpeedBoostCoroutine(multiplier, duration));
    }

    private IEnumerator SpeedBoostCoroutine(float multiplier, float duration)
    {
        moveSpeed = originalMoveSpeed * multiplier;
        Debug.Log($"Speed boost applied: x{multiplier} for {duration} seconds.");
        yield return new WaitForSeconds(duration);
        moveSpeed = originalMoveSpeed;
        Debug.Log("Speed boost ended.");
    }
}
