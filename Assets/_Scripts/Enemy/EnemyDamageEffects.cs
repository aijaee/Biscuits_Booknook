using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class EnemyDamageEffects : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator animator;
    private EnemyController enemyController;

    [Header("Knockback")]
    public float knockbackForce = 8f;
    public float knockbackDuration = 0.35f;

    [Header("Shake")]
    public float shakeAmount = 0.08f;
    public float shakeDuration = 0.3f;

    [Header("Stun")]
    public float stunDuration = 0.5f;

    private Coroutine damageCoroutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        enemyController = GetComponent<EnemyController>();
    }

    public void PlayDamageEffects(Vector2 hitDir)
    {
        if (damageCoroutine != null)
            StopCoroutine(damageCoroutine);

        damageCoroutine = StartCoroutine(DamageRoutine(hitDir));
    }

    public void StopAllEffectsImmediately()
    {
        if (damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
            damageCoroutine = null;
        }

        if (enemyController != null)
        {
            enemyController.isStunned = false;
            enemyController.SetKnockbackState(false);
        }
    }

    private IEnumerator DamageRoutine(Vector2 hitDir)
    {
        if (enemyController != null)
        {
            enemyController.isStunned = true;
            enemyController.SetKnockbackState(true);
        }

        if (animator != null)
            animator.SetTrigger("Damaged");

        if (rb != null)
            rb.AddForce(hitDir.normalized * knockbackForce, ForceMode2D.Impulse);

        yield return new WaitForSeconds(knockbackDuration);

        rb.linearVelocity = Vector2.zero;

        float elapsed = 0f;
        Vector2 shakeBase = rb.position;
        while (elapsed < shakeDuration)
        {
            Vector2 offset = Random.insideUnitCircle * shakeAmount;
            rb.MovePosition(shakeBase + offset);
            elapsed += Time.deltaTime;
            yield return null;
        }

        rb.MovePosition(shakeBase);

        yield return new WaitForSeconds(stunDuration);

        if (enemyController != null)
        {
            enemyController.isStunned = false;
            enemyController.SetKnockbackState(false);
        }

        damageCoroutine = null;
    }
}