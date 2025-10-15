using UnityEngine;
using System.Collections;

public class BossPhase1RainAttack : MonoBehaviour
{
    [Header("Rain AOE Settings")]
    public GameObject aoeIndicatorPrefab;
    public GameObject aoePrefab;
    public float aoeInterval = 8f;
    public float aoeWarningTime = 1.5f;
    public float aoeDuration = 4f;
    public float aoeRadius = 2.5f;
    public float aoeDamagePerSecond = 10f;

    float aoeTimer;

    public void PerformAttack()
    {
        if (aoeTimer > 0f)
        {
            aoeTimer -= Time.deltaTime;
            return;
        }
        StartCoroutine(RainAOE());
        aoeTimer = aoeInterval;
    }

    IEnumerator RainAOE()
    {
        // get player transform
        Transform player = GameObject.FindWithTag("Player").transform;
        GameObject ind = null;

        // spawn indicator once
        if (aoeIndicatorPrefab)
        {
            ind = Instantiate(aoeIndicatorPrefab, player.position, Quaternion.identity);
            var sr = ind.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                float spriteDiameter = sr.sprite.bounds.size.x;
                float scaleFactor = (aoeRadius * 2f) / spriteDiameter;
                ind.transform.localScale = Vector3.one * scaleFactor;
            }
            else
            {
                ind.transform.localScale = Vector3.one * aoeRadius * 2f;
            }
        }

        // follow player for the warning duration
        float elapsed = 0f;
        while (elapsed < aoeWarningTime)
        {
            if (ind != null)
                ind.transform.position = player.position;
            elapsed += Time.deltaTime;
            yield return null;
        }

        // determine final target & clean up indicator
        Vector3 finalPos = ind != null ? ind.transform.position : player.position;
        if (ind != null) Destroy(ind);

        // spawn the actual AOE zone
        GameObject zone = aoePrefab != null
            ? Instantiate(aoePrefab, finalPos, Quaternion.identity)
            : new GameObject("RainAOE");
        zone.AddComponent<AOEBehaviour>().Init(finalPos, aoeRadius, aoeDuration, aoeDamagePerSecond);
    }

    // Inner class handles the damage zone
    private class AOEBehaviour : MonoBehaviour
    {
        float duration;
        float dps;
        private Coroutine damageRoutine;

        public void Init(Vector3 pos, float radius, float durationSec, float damagePerSec)
        {
            duration = durationSec;
            dps = damagePerSec;
            transform.position = pos;

            var col = gameObject.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = radius;

            // start lifetime countdown
            StartCoroutine(Life());

            // if player already inside, trigger entry logic
            foreach (var other in Physics2D.OverlapCircleAll(pos, radius))
                if (other.CompareTag("Player"))
                    OnTriggerEnter2D(other);
        }

        IEnumerator Life()
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                yield return null;
            }
            Destroy(gameObject);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (damageRoutine != null || !other.CompareTag("Player")) return;
            var pc = other.GetComponent<PlayerController>();
            if (pc != null)
                damageRoutine = StartCoroutine(DamageLoop(pc));
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player") || damageRoutine == null) return;
            StopCoroutine(damageRoutine);
            damageRoutine = null;
        }

        IEnumerator DamageLoop(PlayerController pc)
        {
            // first tick after one full second
            yield return new WaitForSeconds(1f);
            float elapsed = 1f;

            while (elapsed < duration)
            {
                pc.TakeDamage(Mathf.CeilToInt(dps));
                yield return new WaitForSeconds(1f);
                elapsed += 1f;
            }
        }
    }
}
