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

    [Header("Tilt")]
    public float tiltAngle = 28f;
    public float tiltDuration = 0.35f;

    [Header("Shake")]
    public float shakeAmount = 0.08f;
    public float shakeDuration = 0.3f;

    [Header("Timing")]
    public float totalReactionTime = 1f;

    private float animatorOriginalSpeed = 1f;
    private Vector3 originalLocalEuler;
    private Coroutine damageCoroutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        enemyController = GetComponent<EnemyController>();

        animatorOriginalSpeed = animator != null ? animator.speed : 1f;
        originalLocalEuler = transform.localEulerAngles;
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
            animator.speed = 0f;

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

            float sign = hitDir.x >= 0f ? -1f : 1f;
            float angle = Mathf.Lerp(0f, tiltAngle * sign, smooth);
            transform.localEulerAngles = new Vector3(0f, 0f, angle);

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (rb != null)
            rb.MovePosition(targetPos);
        else
            transform.position = targetPos;

        if (animator != null)
            animator.speed = animatorOriginalSpeed;

        elapsed = 0f;
        while (elapsed < tiltDuration)
        {
            float t = elapsed / tiltDuration;
            float currentAngle = Mathf.Lerp(transform.localEulerAngles.z, originalLocalEuler.z, t);
            transform.localEulerAngles = new Vector3(0f, 0f, currentAngle);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localEulerAngles = originalLocalEuler;

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

        if (rb != null)
            rb.MovePosition(shakeBase);
        else
            transform.position = shakeBase;

        float spent = knockbackDuration + tiltDuration + shakeDuration;
        float remainder = Mathf.Max(0f, totalReactionTime - spent);
        if (remainder > 0f)
            yield return new WaitForSeconds(remainder);

        if (enemyController != null)
        {
            enemyController.isStunned = false;
            enemyController.SetKnockbackState(false);
        }

        damageCoroutine = null;
    }
}
