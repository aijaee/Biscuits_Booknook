using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossPhase2InkWallAttack : MonoBehaviour
{
    [SerializeField] private GameObject inkWallPrefab;
    [SerializeField] private float wallDuration = 5f;
    [SerializeField] private int maxWalls = 2;

    [SerializeField] private RuntimeAnimatorController wallAnimatorController;
    [SerializeField] private string riseState = "Rise";      
    [SerializeField] private float fadeDuration = 0.5f;

    [SerializeField] private float riseDuration = 0.5f;

    [SerializeField] private string fallState = "Fall";    
    [SerializeField] private float fallDuration = 1f;


    [SerializeField] private int wallSegments = 10;
    [SerializeField] private float segmentSpacing = 0.2f; 
    [SerializeField] private Vector2 lineDirection = Vector2.right;

    [SerializeField] private BossGridManager gridManager;
    [SerializeField] private Transform playerTransform; 

    private List<GameObject> activeWalls = new List<GameObject>();

    private void Awake()
    {
        EnsureReferences();
    }
    private void EnsureReferences()
    {
        if (gridManager == null)
            gridManager = FindObjectOfType<BossGridManager>();

        if (playerTransform == null)
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                playerTransform = playerObj.transform;
        }
    }

    public void PerformAttack()
    {
        var boss = GetComponentInParent<BossStatsMovement>();
        if (boss != null && boss.currentHealth <= 0f)
            return;

        if (activeWalls.Count >= maxWalls) return;
        StartCoroutine(BuildWall());
    }

    private IEnumerator BuildWall()
    {
        EnsureReferences();


        int mode = Random.Range(0, 3);
        int segmentsToSpawn = mode == 1 ? 2 : (mode == 2 ? wallSegments : 1);


        Vector3 segDir = new Vector3(lineDirection.x, lineDirection.y, 0f).normalized;

        Vector3 lineVec = new Vector3(-segDir.y, segDir.x, 0f);


        float prefabSize = 1f;
        var sr = inkWallPrefab.GetComponent<SpriteRenderer>();
        if (sr != null)
            prefabSize = sr.bounds.size.x;
        float spacing = prefabSize + segmentSpacing;


        float startOffset = - (segmentsToSpawn - 1) / 2f * spacing;

        for (int i = 0; i < segmentsToSpawn; i++)
        {
            Vector3 pos = (playerTransform != null ? playerTransform.position : transform.position)
                          + lineVec * (startOffset + i * spacing);

            if (gridManager != null)
            {
                var cell = new Vector3Int(
                    Mathf.RoundToInt(pos.x),
                    Mathf.RoundToInt(pos.y),
                    0
                );
                if (!gridManager.Bounds.Contains(cell))
                    continue;
            }

            GameObject w = Instantiate(inkWallPrefab, pos, Quaternion.identity);
            activeWalls.Add(w);

            if (wallAnimatorController != null)
            {
                var animator = w.GetComponent<Animator>() ?? w.AddComponent<Animator>();
                animator.runtimeAnimatorController = wallAnimatorController;
                animator.Play(riseState, 0, 0f);

                StartCoroutine(FreezeAnimation(animator, riseDuration));
            }

            StartCoroutine(RemoveSegment(w));
        }

        yield break;
    }


    private IEnumerator RemoveSegment(GameObject w)
    {
        yield return new WaitForSeconds(wallDuration);

        if (w)
        {
            var c2 = w.GetComponent<Collider2D>(); if (c2) c2.enabled = false;
            var c3 = w.GetComponent<Collider>();   if (c3) c3.enabled = false;

            var animator = w.GetComponent<Animator>();
            if (animator)
            {
                animator.speed = 1f;
                animator.Play(fallState, 0, 0f);

                var clips = animator.GetCurrentAnimatorClipInfo(0);
                float waitTime = clips.Length > 0 ? clips[0].clip.length : fallDuration;
                yield return new WaitForSeconds(waitTime);
            }
            else
            {
                yield return new WaitForSeconds(fallDuration);
            }
        }

        if (w)
        {
            activeWalls.Remove(w);
            Destroy(w);
        }
    }

    private IEnumerator FreezeAnimation(Animator animator, float duration)
    {
        yield return new WaitForSeconds(duration);
        if (!animator) 
            yield break;
        animator.speed = 0f;
    }

    public void ClearAllWalls()
    {
        foreach (var w in activeWalls.ToArray())
            StartCoroutine(RemoveSegment(w));
        activeWalls.Clear();
    }
}
