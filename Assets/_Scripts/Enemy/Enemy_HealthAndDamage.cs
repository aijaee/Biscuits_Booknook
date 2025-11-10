using UnityEngine;
using System.Collections;

public class Enemy_HealthAndDamage : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    private int currentHealth;

    private EnemyController enemyController;
    private EnemyDamageEffects damageEffects;
    private Animator animator;

    private void Start()
    {
        currentHealth = maxHealth;
        enemyController = GetComponent<EnemyController>();
        damageEffects = GetComponent<EnemyDamageEffects>();
        animator = GetComponent<Animator>();
    }

    public void EnemyTakeDamage(int damage, Vector2 hitDirection)
    {
        if (enemyController != null && enemyController.isDead) return;

        int newHealth = currentHealth - damage;
        bool willDie = newHealth <= 0;

        // If enemy survives, apply knockback
        if (!willDie)
        {
            currentHealth = newHealth;
            if (damageEffects != null)
                damageEffects.PlayDamageEffects(hitDirection);
        }
        else
        {
            currentHealth = 0;
            if (damageEffects != null)
                damageEffects.StopAllEffectsImmediately();
            Die();
        }
    }

    private void Die()
    {
        if (enemyController != null && enemyController.isDead) return;

        if (enemyController != null)
            enemyController.isDead = true;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Debug.Log($"{gameObject.name} has died.");

        if (animator != null)
            animator.SetTrigger("Death");

        if (enemyController != null)
        {
            enemyController.isStunned = true;
            enemyController.enabled = false;

            Rigidbody2D rb = enemyController.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }

        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(0.6f);
        yield return StartCoroutine(FadeAndDestroy());
    }

    private IEnumerator FadeAndDestroy()
    {
        SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
        if (sr == null)
        {
            Destroy(gameObject);
            yield break;
        }

        Color orig = sr.color;
        float dur = 0.8f;
        float e = 0f;
        while (e < dur)
        {
            float a = Mathf.Lerp(1f, 0f, e / dur);
            sr.color = new Color(orig.r, orig.g, orig.b, a);
            e += Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }
}