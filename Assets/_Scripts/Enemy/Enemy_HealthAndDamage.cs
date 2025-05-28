using UnityEngine;

public class Enemy_HealthAndDamage : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    private int currentHealth;

    private EnemyController enemyController;

    private void Start()
    {
        currentHealth = maxHealth;
        enemyController = GetComponent<EnemyController>();
    }

    public void EnemyTakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage. Remaining HP: {currentHealth}");

        // Play damaged animation if available
        if (enemyController != null)
        {
            enemyController.PlayDamagedAnimation();
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} has died.");
        // Death Anim
        Destroy(gameObject);
    }
}
