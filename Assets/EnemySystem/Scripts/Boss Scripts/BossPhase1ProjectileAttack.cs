using UnityEngine;

public class BossPhase1ProjectileAttack : MonoBehaviour
{
    [Header("Projectile Attack Settings")]
    public ProjectileBehaviour projectilePrefab;  
    public Transform firePoint;
    public float shootInterval = 3f;

    [Header("Projectile Behavior")]
    public float projectileSpeed = 5f;
    public float returnSpeed = 8f;
    public float damage = 20f;
    public float maxProjectileDistance = 20f;

    float timer;

    public void PerformAttack()
    {
        // debug: verify this method is invoked
        Debug.Log($"[{name}] PerformAttack called. Timer={timer:F2}");

        // guard against missing assignments
        if (projectilePrefab == null)
        {
            Debug.LogError($"[{name}] projectilePrefab is not assigned.");
            return;
        }
        if (firePoint == null)
        {
            Debug.LogError($"[{name}] firePoint is not assigned.");
            return;
        }

        if (timer > 0f)
        {
            timer -= Time.deltaTime;
            return;
        }

        var player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null) return;

        // Calculate direction and rotation so projectile faces the player
        Vector2 dir = (player.position - firePoint.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        var proj = Instantiate(
            projectilePrefab,
            firePoint.position,
            Quaternion.Euler(0f, 0f, angle)
        );
        proj.Init(
            owner: transform,
            player: player,
            speed: projectileSpeed,
            returnSpeed: returnSpeed,
            damage: damage,
            maxDistance: maxProjectileDistance
        );

        timer = shootInterval;
    }
}
