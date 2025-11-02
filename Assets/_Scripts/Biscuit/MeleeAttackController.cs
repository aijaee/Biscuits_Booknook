using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class MeleeAttackController : MonoBehaviour
{
    [Header("Attack Settings")]
    public Transform swingPoint;
    public float swingDistanceFromPivot = 1f;
    public float swingRange = 0.5f;
    public float detectionRange = 3f;
    public int attackDamage = 40;
    public float attackHitDuration = 0.3f;
    public float attackCooldown = 0.6f;
    public float combo3DamageMultiplier = 1.5f;
    public float combo3RangeMultiplier = 10f / 8.26f;
    public float attackBufferTime = 0.2f;
    public float postCombo3Cooldown = 0.5f;

    [Header("References")]
    public LayerMask enemyLayers;
    public GameObject meleeWeapon;
    public Transform swingPivot;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private float lastAttackTime = -Mathf.Infinity;
    private float lastComboTime = -Mathf.Infinity;
    private bool buttonPressed = false;
    private bool attackBuffered = false;
    private Vector2 lastMoveDirection = Vector2.right;
    private bool useMouseAim = false;
    private Vector2 mouseDirection;
    private int comboStep = 0;
    private HashSet<Collider2D> alreadyHitDuringAttack = new HashSet<Collider2D>();

    private void Awake()
    {
        enemyLayers |= LayerMask.GetMask("Boss");
    }

    private void Update()
    {
        HandleInput();

        if (buttonPressed)
        {
            PerformAttack();
            buttonPressed = false;
        }

        if (attackBuffered && Time.time - lastAttackTime >= attackCooldown)
        {
            buttonPressed = true;
            attackBuffered = false;
        }
    }

    private void HandleInput()
    {
        if (Mouse.current != null)
        {
            Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            Vector2 dirToMouse = (mouseWorld - (Vector2)transform.position).normalized;

            if (dirToMouse.sqrMagnitude > 0.01f)
                mouseDirection = dirToMouse;

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                useMouseAim = true;
                OnAttackButtonPressed();
            }
        }

        if (Keyboard.current != null && Keyboard.current.kKey.wasPressedThisFrame)
        {
            useMouseAim = false;
            OnAttackButtonPressed();
        }
    }

    private void OnAttackButtonPressed()
    {
        float timeSinceLastAttack = Time.time - lastAttackTime;

        if (timeSinceLastAttack >= attackCooldown)
        {
            if (Time.time - lastComboTime > 0.8f) comboStep = 0;
            buttonPressed = true;
        }
        else if (timeSinceLastAttack >= attackCooldown - attackBufferTime)
        {
            attackBuffered = true;
        }
    }

    public void UpdateMovementDirection(Vector2 moveInput)
    {
        if (moveInput.sqrMagnitude > 0.01f)
            lastMoveDirection = moveInput.normalized;
    }

    public void PerformAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;

        comboStep = (Time.time - lastComboTime > 0.8f) ? 1 : comboStep + 1;
        if (comboStep > 3) comboStep = 1;

        float damage = attackDamage;
        float aoeRadius = swingRange;
        float swingDistance = swingDistanceFromPivot;
        
        if (comboStep == 3)
            swingDistance *= combo3RangeMultiplier;

        Vector2 direction = useMouseAim ? mouseDirection : GetClosestDirection() ?? lastMoveDirection;
        PositionSwingPoint(direction, swingDistance);

        alreadyHitDuringAttack.Clear();
        ShowWeaponSwing();
        PlayComboAnimation(comboStep);

        lastComboTime = Time.time;
        lastAttackTime = Time.time;

        if (comboStep == 3)
            lastAttackTime += postCombo3Cooldown;

        StartCoroutine(HitEnemiesDuringAttack(damage, aoeRadius));
    }

    private Vector2? GetClosestDirection()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, detectionRange, enemyLayers);
        if (enemies.Length == 0) return null;

        Transform closest = null;
        float minDist = Mathf.Infinity;

        foreach (var e in enemies)
        {
            float dist = Vector2.Distance(transform.position, e.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = e.transform;
            }
        }

        return closest != null ? (Vector2?)(closest.position - transform.position).normalized : null;
    }

    private IEnumerator HitEnemiesDuringAttack(float damage, float aoeRadius)
    {
        float elapsed = 0f;
        while (elapsed < attackHitDuration)
        {
            var hits = Physics2D.OverlapCircleAll(swingPoint.position, aoeRadius, enemyLayers);
            foreach (var enemy in hits)
            {
                if (alreadyHitDuringAttack.Contains(enemy)) continue;

                if (enemy.TryGetComponent<Enemy_HealthAndDamage>(out var health))
                {
                    health.EnemyTakeDamage((int)damage, (enemy.transform.position - transform.position).normalized);
                    alreadyHitDuringAttack.Add(enemy);
                }
                else if (enemy.TryGetComponent<BossStatsMovement>(out var boss) && boss.IsMeleeVulnerable)
                {
                    boss.TakeDamage((int)damage);
                    alreadyHitDuringAttack.Add(enemy);
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private void PositionSwingPoint(Vector2 direction, float distance)
    {
        swingPoint.position = swingPivot.position + (Vector3)(direction * distance);
        swingPoint.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
    }

    private void PlayComboAnimation(int step)
    {
        if (animator == null) return;
        animator.SetTrigger($"Attack{step}");
    }

    private bool IsAttacking()
    {
        return Time.time - lastAttackTime < attackCooldown + 0.2f;
    }

    private void DamageEnemiesInRange()
    {
        float aoeRadius = comboStep == 3 ? swingRange * combo3RangeMultiplier : swingRange;

        Collider2D[] hits = Physics2D.OverlapCircleAll(swingPoint.position, aoeRadius, enemyLayers);
        foreach (var enemy in hits)
        {
            if (alreadyHitDuringAttack.Contains(enemy)) continue;

            int damageToDeal = comboStep == 3 ? Mathf.RoundToInt(attackDamage * combo3DamageMultiplier) : attackDamage;

            if (enemy.TryGetComponent<Enemy_HealthAndDamage>(out var health))
            {
                health.EnemyTakeDamage(damageToDeal, (enemy.transform.position - transform.position).normalized);
                alreadyHitDuringAttack.Add(enemy);
            }
            else if (enemy.TryGetComponent<BossStatsMovement>(out var boss) && boss.IsMeleeVulnerable)
            {
                boss.TakeDamage(damageToDeal);
                alreadyHitDuringAttack.Add(enemy);
            }
        }
    }

    private void ShowWeaponSwing()
    {
        meleeWeapon.SetActive(true);
        Invoke(nameof(HideWeaponSwing), 0.4f);
    }

    private void HideWeaponSwing()
    {
        meleeWeapon.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        if (swingPoint != null)
        {
            Gizmos.color = Color.red;
            float aoeRadius = swingRange;

            #if UNITY_EDITOR
            if (Application.isPlaying)
            {
                aoeRadius = comboStep == 3 ? swingRange * combo3RangeMultiplier : swingRange;
            }
            #endif

            Gizmos.DrawWireSphere(swingPoint.position, aoeRadius);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }

    public void AddDamageBuff(int amount)
    {
        attackDamage += amount;
        Debug.Log($"Damage buff applied: +{amount} damage. Current damage: {attackDamage}.");
    }
}