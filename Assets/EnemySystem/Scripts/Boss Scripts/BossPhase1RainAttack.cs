using UnityEngine;
using System.Collections;
using System.Collections.Generic; 

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
    public float aoeSlowFactor = 0.5f;    // new: slow multiplier applied inside AOE

    float aoeTimer;
    private List<GameObject> activeAreas = new List<GameObject>();  

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
            activeAreas.Add(ind); 
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

        var beh = zone.AddComponent<AOEBehaviour>();
        beh.Init(finalPos, aoeRadius, aoeDuration, aoeDamagePerSecond);

        var slow = zone.AddComponent<SlowZone>();   
        slow.slowFactor = aoeSlowFactor;

        activeAreas.Add(zone);  
    }

    public void ClearAllProjectiles()
    {
        foreach (var go in activeAreas)
            if (go != null) Destroy(go);
        activeAreas.Clear();
    }


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

            StartCoroutine(Life());
            StartCoroutine(PlayAndFadeAnimation());


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

        IEnumerator PlayAndFadeAnimation()
        {
            var animator = GetComponent<Animator>();
            var sr = GetComponent<SpriteRenderer>();
            if (animator == null || sr == null || animator.runtimeAnimatorController == null)
                yield break;

            var clips = animator.runtimeAnimatorController.animationClips;
            if (clips.Length == 0)
                yield break;
            var clip = clips[0];

            float frameTime = 1f / clip.frameRate;
            float animPlayTime = clip.length - frameTime;
            yield return new WaitForSeconds(Mathf.Min(animPlayTime, duration));

            // freeze on penultimate frame
            animator.speed = 0f;

            float remaining = duration - Mathf.Min(animPlayTime, duration);
            if (remaining > 0f)
                yield return new WaitForSeconds(remaining);

            // fade out
            float fadeDuration = 1f;
            float elapsed = 0f;
            Color original = sr.color;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(original.a, 0f, elapsed / fadeDuration);
                sr.color = new Color(original.r, original.g, original.b, alpha);
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
