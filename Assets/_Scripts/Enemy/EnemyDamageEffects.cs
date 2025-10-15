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
    public float knockbackDistance = 0.9f;
    public float knockbackDuration = 0.35f;

    [Header("Shake")]
    public float shakeAmount = 0.08f;
    public float shakeDuration = 0.3f;

    [Header("Stun")]
    public float stunDuration = 0.5f;

    private float animatorOriginalSpeed = 1f;
    private Coroutine damageCoroutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        enemyController = GetComponent<EnemyController>();
        animatorOriginalSpeed = animator != null ? animator.speed : 1f;
    }

    public void PlayDamageEffects(Vector2 hitDir)
    {
        if (damageCoroutine != null)
            StopCoroutine(damageCoroutine);

        damageCoroutine = StartCoroutine(DamageRoutine(hitDir));
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

        Vector2 startPos = rb != null ? rb.position : (Vector2)transform.position;
        Vector2 targetPos = startPos + hitDir.normalized * knockbackDistance;

        float elapsed = 0f;
        while (elapsed < knockbackDuration)
        {
            float t = elapsed / knockbackDuration;
            float smooth = Mathf.SmoothStep(0f, 1f, t);
            Vector2 newPos = Vector2.Lerp(startPos, targetPos, smooth);

            if (rb != null)
                rb.MovePosition(newPos);
            else
                transform.position = newPos;

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (rb != null) rb.MovePosition(targetPos);
        else transform.position = targetPos;

        Vector2 shakeBase = rb != null ? rb.position : (Vector2)transform.position;
        elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            Vector2 offset = Random.insideUnitCircle * shakeAmount;
            Vector2 pos = shakeBase + offset;

            if (rb != null)
                rb.MovePosition(pos);
            else
                transform.position = pos;

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (rb != null) rb.MovePosition(shakeBase);
        else transform.position = shakeBase;

        yield return new WaitForSeconds(stunDuration);

        if (enemyController != null)
        {
            enemyController.isStunned = false;
            enemyController.SetKnockbackState(false);
        }

        damageCoroutine = null;
    }
}
