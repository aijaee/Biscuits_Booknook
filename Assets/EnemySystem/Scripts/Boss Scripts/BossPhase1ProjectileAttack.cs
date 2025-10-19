using UnityEngine;
using System.Collections.Generic;  

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
    private List<GameObject> activeProjectiles = new List<GameObject>(); 

    public void PerformAttack()
    {

        Debug.Log($"[{name}] PerformAttack called. Timer={timer:F2}");

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

        activeProjectiles.Add(proj.gameObject); 
        timer = shootInterval;
    }

    public void ClearAllProjectiles()
    {
        foreach (var go in activeProjectiles)
            if (go != null) Destroy(go);
        activeProjectiles.Clear();
    }
}
