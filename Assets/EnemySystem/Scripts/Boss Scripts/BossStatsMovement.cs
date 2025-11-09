using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BossStatsMovement : MonoBehaviour
{
    public enum BossState { Enter, Flying, Grounded, Stunned }

    // new: current state
    [Header("Debug")]
    [SerializeField] private BossState state;

    public enum BossPhase { Phase1, Phase2 }
    [SerializeField] private BossPhase phase;

    [Header("Stats")]
    public float maxHealth = 200f;
    public float currentHealth;
    public float moveSpeed = 3f;
    public float visionRange = 10f;

    [Header("Stun")]
    public float stunDuration = 2f;
    private float stunTimer;

    private Vector3 originalPosition;

    private Transform player;
    private Rigidbody2D rb;

    [Header("Phase-1 Attacks")]
    [SerializeField] private BossPhase1ProjectileAttack projectileAttack;
    [SerializeField] private BossPhase1RainAttack rainAttack;

    [Header("Phase-2 Attacks")]
    [SerializeField] private BossPhase2InkWallAttack inkWallAttack;
    [SerializeField] private BossPhase2DashAttack dashAttack;

    private bool nextDash = true;

    [Header("Cutscene")]                              // new
    [SerializeField] private Animator animator;        // new: boss Animator
    [SerializeField] private float cameraPanDuration = 1f;   
    [SerializeField] private float panHoldDuration = 0.5f;       // new: hold time at top
    [SerializeField] private float animationWaitTime = 1.5f; // new

    private bool hasStartedCutscene = false;   // new
    public System.Action OnCutsceneComplete;

    // new fields for freezing player
    private Rigidbody2D playerRb;
    private RigidbodyConstraints2D originalPlayerConstraints;

    // new: track dash state
    private bool wasDashing = false;

    [Header("Death Fade")]
    [SerializeField] private float fadeDuration = 2f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        originalPosition = transform.position;
        phase = BossPhase.Phase1;  // initialize phase
        moveSpeed = 0f;           // Phase1: no movement
    }

    void Start()
    {
        ChangeState(BossState.Enter);
    }

    void Update()
    {
        if (currentHealth <= 0f) return;

        switch (state)
        {
            case BossState.Enter:   HandleEnter();   break;
            case BossState.Flying:  HandleFlying();  break;
            case BossState.Grounded:HandleGrounded();break;
            case BossState.Stunned: HandleStunned(); break;
        }
    }

    public void ChangeState(BossState newState)
    {
        state = newState;
        stunTimer = (newState == BossState.Stunned) ? stunDuration : 0f;

        if (state == BossState.Stunned && animator != null)
        {
            animator.CrossFade("Stunned", 0f);
        }

        if (state == BossState.Flying && phase == BossPhase.Phase1 && animator != null)
        {
            animator.CrossFade("Idle", 0f);
        }
    }

    private void HandleEnter()
    {
        if (hasStartedCutscene) return;
        hasStartedCutscene = true;

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var controller = player.GetComponent<PlayerController>();
            if (controller != null)
                controller.enabled = false;

            playerRb = player.GetComponent<Rigidbody2D>();            // new
            if (playerRb != null)
            {
                originalPlayerConstraints = playerRb.constraints;     // new
                playerRb.constraints = RigidbodyConstraints2D.FreezeAll; // new
            }
        }

        // start the cutscene
        StartCoroutine(EnterCutsceneCoroutine());
    }

    private void HandleFlying()
    {
        if (phase == BossPhase.Phase1)
        {
            projectileAttack?.PerformAttack();
            rainAttack?.PerformAttack();
        }
    }

    private void HandleGrounded()
    {
        if (phase != BossPhase.Phase2) return;

        // detect end of dash and reset to Idle
        bool currentlyDashing = dashAttack != null && dashAttack.IsDashing;
        if (wasDashing && !currentlyDashing && animator != null)
        {
            animator.CrossFade("Idle", 0f);
        }
        wasDashing = currentlyDashing;

        if (nextDash)
        {
            if (dashAttack != null && !dashAttack.IsDashing)
            {
                dashAttack.PerformAttack();
                nextDash = false;
            }
        }
        else
        {
            inkWallAttack?.PerformAttack();
            nextDash = true;
        }
    }

    private void HandleStunned()
    {
        rb.linearVelocity = Vector2.zero;     
        if (stunTimer > 0f)
        {
            stunTimer -= Time.deltaTime;
        }
        else
        {
            if (phase == BossPhase.Phase1)
                ChangeState(BossState.Flying);
            else
                ChangeState(BossState.Grounded);
        }
    }

    private void MovementLogic()
    {
        rb.linearVelocity = Vector2.zero;      
    }

    public void TakeDamage(float amount, bool applyStun = true)
    {
        currentHealth -= amount;
        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            Die();
        }
        else
        {
            if (applyStun)
                ChangeState(BossState.Stunned);
            CheckPhase();       
        }
    }


    private void CheckPhase()
    {
        float pct = currentHealth / maxHealth;
        if (pct <= 0.45f && phase == BossPhase.Phase1)
        {
            phase = BossPhase.Phase2;
            OnPhase2();
        }
    }

    private void OnPhase2()
    {
        ChangeState(BossState.Grounded);
        moveSpeed = 4f;

        projectileAttack?.StopAllCoroutines();
        rainAttack?.StopAllCoroutines();

        projectileAttack?.ClearAllProjectiles();
        rainAttack?.ClearAllProjectiles();
    }

    private void Die()
    {
        rb.linearVelocity = Vector2.zero;      
        animator.CrossFade("Stunned", 0f);

        projectileAttack?.StopAllCoroutines();
        rainAttack?.StopAllCoroutines();
        inkWallAttack?.StopAllCoroutines();
        inkWallAttack?.ClearAllWalls();    
        dashAttack?.StopAllCoroutines();

        projectileAttack = null;
        rainAttack       = null;
        inkWallAttack    = null;
        dashAttack       = null;

        StartCoroutine(FadeOutAndDestroy());
    }

    private IEnumerator FadeOutAndDestroy()
    {
        var renderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (var sr in renderers)
            sr.color = Color.grey;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            foreach (var sr in renderers)
            {
                var c = sr.color;
                c.a = alpha;
                sr.color = c;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
        // ensure fully transparent
        foreach (var sr in renderers)
        {
            var c = sr.color;
            c.a = 0f;
            sr.color = c;
        }

        var spawner = FindObjectOfType<BossDefeatedPortalSpawner>();
        if (spawner != null)
            spawner.SpawnPortal();

        Destroy(gameObject);
    }

    private IEnumerator EnterCutsceneCoroutine()
    {
        var cam = Camera.main;

        Vector3 originalCamPos = cam.transform.position;
        Vector3 targetCamPos = new Vector3(transform.position.x, transform.position.y, originalCamPos.z);
        float elapsed = 0f;

        // pan camera to boss
        while (cam != null && elapsed < cameraPanDuration)
        {
            cam.transform.position = Vector3.Lerp(originalCamPos, targetCamPos, elapsed / cameraPanDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (cam != null) cam.transform.position = targetCamPos;

        yield return new WaitForSeconds(panHoldDuration);   


        if (animator != null)
            animator.SetTrigger("Enter");


        yield return new WaitForSeconds(animationWaitTime);

        if (animator != null)
        {
            animator.ResetTrigger("Enter");
            animator.CrossFade("Idle", 0f);
        }


        if (cam != null)
            cam.transform.position = originalCamPos;


        var playerRe = GameObject.FindGameObjectWithTag("Player");
        if (playerRe != null)
        {
            var controllerRe = playerRe.GetComponent<PlayerController>();
            if (controllerRe != null)
                controllerRe.enabled = true;

            var rbRe = playerRe.GetComponent<Rigidbody2D>();         // new
            if (rbRe != null)
                rbRe.constraints = originalPlayerConstraints;       // new
        }

        var hpBar = FindObjectOfType<BossHPBar>();
        if (hpBar != null)
            yield return StartCoroutine(hpBar.ShowBar());

        phase = BossPhase.Phase1;
        ChangeState(BossState.Flying);

        OnCutsceneComplete?.Invoke();
    }


    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);
    }

    public bool IsMeleeVulnerable
    {
        get { return phase == BossPhase.Phase2 && state == BossState.Stunned; }
    }

    public BossState CurrentState => state;
    public BossPhase CurrentPhase => phase;
}