using UnityEngine;
using System.Collections;

public class Enemy_HealthAndDamage : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    private int currentHealth;

    private EnemyController enemyController;
    private EnemyDamageEffects damageEffects;

    private void Start()
    {
        currentHealth = maxHealth;
        enemyController = GetComponent<EnemyController>();
        damageEffects = GetComponent<EnemyDamageEffects>();
    }

    public void EnemyTakeDamage(int damage, Vector2 hitDirection)
    {
        currentHealth -= damage;

        if (damageEffects != null)
        {
            damageEffects.PlayDamageEffects(hitDirection);
        }

        Debug.Log($"{gameObject.name} took {damage} damage. Remaining HP: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} has died.");
        if (enemyController != null)
        {
            enemyController.isStunned = true;
            enemyController.enabled = false;
            Rigidbody2D rb = enemyController.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }
        StartCoroutine(FadeAndDestroy());
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
