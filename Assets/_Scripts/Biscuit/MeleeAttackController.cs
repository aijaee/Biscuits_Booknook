using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;

public class MeleeAttackController : MonoBehaviour
{
    [Header("Attack Settings")]
    public Transform swingPoint;
    public float swingRange = 0.5f;
    public float detectionRange = 3f;
    public int attackDamage = 40;
    public float attackCooldown = 0.6f;

    [Header("References")]
    public LayerMask enemyLayers;
    public GameObject meleeWeapon;
    [SerializeField] public OnScreenButton onScreenAttackButton;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private float lastAttackTime = -Mathf.Infinity;
    private bool buttonPressed = false;

    private Vector2 lastMoveDirection = Vector2.right; // Default direction

    private void Start()
    {
        if (onScreenAttackButton == null)
        {
            Debug.LogError("Assign the OnScreenButton component in Inspector!", this);
            return;
        }
    }

    private void Update()
    {
        if (onScreenAttackButton != null && onScreenAttackButton.control.IsPressed())
        {
            OnAttackButtonPressed();
        }

        if (Keyboard.current != null && Keyboard.current.kKey.wasPressedThisFrame)
            OnAttackButtonPressed();

        if (buttonPressed)
        {
            PerformAttack();
            buttonPressed = false;
        }
    }

    private void OnAttackButtonPressed()
    {
        if (Time.time - lastAttackTime >= attackCooldown)
            buttonPressed = true;
    }

    public void UpdateMovementDirection(Vector2 moveInput)
    {
        if (moveInput.sqrMagnitude > 0.01f)
            lastMoveDirection = moveInput.normalized;
    }

    public void PerformAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;

        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, detectionRange, enemyLayers);
        Transform closestEnemy = GetClosestEnemy(enemies);

        if (closestEnemy != null)
        {
            Vector2 direction = (closestEnemy.position - transform.position).normalized;
            PositionSwingPoint(direction);
        }
        else
        {
            PositionSwingPoint(lastMoveDirection); // Use movement direction
        }

        DamageEnemiesInRange();
        ShowWeaponSwing();
        lastAttackTime = Time.time;
    }

    private Transform GetClosestEnemy(Collider2D[] enemies)
    {
        Transform closest = null;
        float minDistance = Mathf.Infinity;

        foreach (Collider2D enemy in enemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = enemy.transform;
            }
        }
        return closest;
    }

    private void PositionSwingPoint(Vector2 direction)
    {
        swingPoint.localPosition = direction * swingRange;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        swingPoint.localRotation = Quaternion.Euler(0, 0, angle);
    }

    private void DamageEnemiesInRange()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(swingPoint.position, swingRange, enemyLayers);
        foreach (Collider2D enemy in hits)
        {
            if (enemy.TryGetComponent<Enemy_HealthAndDamage>(out var health))
            {
                health.EnemyTakeDamage(attackDamage);
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
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(swingPoint.position, swingRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
