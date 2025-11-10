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

    private Dictionary<GameObject, List<(Collider2D bossCol, Collider2D wallCol)>> ignoredPairs2D;
    private Dictionary<GameObject, List<(Collider bossCol, Collider wallCol)>> ignoredPairs3D;

    private void Awake()
    {
        EnsureReferences();

        ignoredPairs2D = new Dictionary<GameObject, List<(Collider2D, Collider2D)>>();
        ignoredPairs3D = new Dictionary<GameObject, List<(Collider, Collider)>>();
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

			var wallCols2D = new List<Collider2D>(w.GetComponents<Collider2D>());
			var wallCols3D = new List<Collider>(w.GetComponents<Collider>());
			var boss = GetComponentInParent<BossStatsMovement>();
			var stored2D = new List<(Collider2D bossCol, Collider2D wallCol)>();
			var stored3D = new List<(Collider bossCol, Collider wallCol)>();

			if (boss != null)
			{
				var bossCols2D = boss.GetComponentsInChildren<Collider2D>();
				foreach (var bc in bossCols2D)
				{
					foreach (var wc in wallCols2D)
					{
						if (bc != null && wc != null)
						{
							Physics2D.IgnoreCollision(bc, wc, true);
							stored2D.Add((bc, wc));
						}
					}
				}

				var bossCols3D = boss.GetComponentsInChildren<Collider>();
				foreach (var bc3 in bossCols3D)
				{
					foreach (var wc3 in wallCols3D)
					{
						if (bc3 != null && wc3 != null)
						{
							Physics.IgnoreCollision(bc3, wc3, true);
							stored3D.Add((bc3, wc3));
						}
					}
				}
			}

			if (stored2D.Count > 0) ignoredPairs2D[w] = stored2D;
			if (stored3D.Count > 0) ignoredPairs3D[w] = stored3D;

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


        if (w != null)
        {

            if (ignoredPairs2D.TryGetValue(w, out var pairs2D))
            {
                foreach (var p in pairs2D)
                {
                    if (p.bossCol != null && p.wallCol != null)
                        Physics2D.IgnoreCollision(p.bossCol, p.wallCol, false);
                }
                ignoredPairs2D.Remove(w);
            }

            if (ignoredPairs3D.TryGetValue(w, out var pairs3D))
            {
                foreach (var p in pairs3D)
                {
                    if (p.bossCol != null && p.wallCol != null)
                        Physics.IgnoreCollision(p.bossCol, p.wallCol, false);
                }
                ignoredPairs3D.Remove(w);
            }
        }

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
