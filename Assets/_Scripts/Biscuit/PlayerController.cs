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

    private RectTransform hpBarRect; // Store the original rect for resizing

    private bool isDead = false;

    private void Awake()
    {
        currentHealth = maxHealth;
        if (deathScreen != null)
            deathScreen.SetActive(false);

        // Cache the RectTransform if using resize mode
        if (hpBarFiller != null)
            hpBarRect = hpBarFiller.GetComponent<RectTransform>();
    }

    private void Start()
    {
        // Ensure the hpBarFiller is assigned by the spawner before updating
        UpdateHPBar();
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        currentHealth -= damage;
        if (currentHealth < 0f) currentHealth = 0f;
        Debug.Log($"{gameObject.name} took {damage} damage. Remaining HP: {currentHealth}");

        UpdateHPBar();

        if (currentHealth <= 0f && !isDead)
        {
            Die();
        }
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
}
