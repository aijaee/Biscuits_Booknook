using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BossController : MonoBehaviour
{
    [Header("Projectile AOE")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 8f;
    public float aoeRadius = 2f;
    public int aoeDamage = 20;
    public LayerMask playerLayerMask;  

    [Header("Ink Rain")]
    public GameObject inkDropPrefab;
    public int rainCount = 10;
    public float rainInterval = 0.2f;
    public float rainHeight = 8f;

    [Header("Dash Attack")]
    public float dashSpeed = 12f;
    public int hitsToStun = 3;

    [Header("Stun")]
    public float stunDuration = 4f;
    public Collider2D vulnerableHitbox;

    private Rigidbody2D rb;
    private int wallHits = 0;

    private enum State { Idle, Attacking, Dashing, Stunned }
    private State state = State.Idle;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        vulnerableHitbox.enabled = false;
    }

    private void Start()
    {
        StartCoroutine(StateMachine());
    }

    private IEnumerator StateMachine()
    {
        while (true)
        {
            switch (state)
            {
                case State.Idle:
                    yield return new WaitForSeconds(2f);
                    ChooseAttack();
                    break;
                case State.Attacking:
                case State.Dashing:
                case State.Stunned:
                    yield return null;
                    break;
            }
        }
    }

    private void ChooseAttack()
    {
        int atk = Random.Range(0, 3);
        if (atk == 0) StartCoroutine(ProjectileAOE());
        else if (atk == 1) StartCoroutine(InkRain());
        else StartCoroutine(DashAttack());
    }

    private IEnumerator ProjectileAOE()
    {
        state = State.Attacking;
        // fire a configured projectile and launch downward
        var projGO = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        var proj = projGO.GetComponent<ProjectileAOE>();
        // apply boss settings
        proj.playerLayerMask = playerLayerMask;
        proj.damage           = aoeDamage;
        proj.aoeRadius        = aoeRadius;
        proj.speed            = projectileSpeed;
        proj.Launch(Vector2.down);

        // wait for impact + cooldown
        yield return new WaitForSeconds(1f);
        yield return new WaitForSeconds(1f);
        state = State.Idle;
    }

    private IEnumerator InkRain()
    {
        state = State.Attacking;
        for (int i = 0; i < rainCount; i++)
        {
            Vector3 pos = transform.position + Vector3.up * rainHeight
                          + Vector3.right * Random.Range(-5f, 5f);
            Instantiate(inkDropPrefab, pos, Quaternion.identity);
            yield return new WaitForSeconds(rainInterval);
        }
        yield return new WaitForSeconds(1f);
        state = State.Idle;
    }

    private IEnumerator DashAttack()
    {
        state = State.Dashing;
        wallHits = 0;
        rb.linearVelocity = Vector2.right * dashSpeed;
        // will bounce in OnCollisionEnter2D
        yield return new WaitUntil(() => state != State.Dashing);
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (state == State.Dashing && col.gameObject.CompareTag("Wall"))
        {
            wallHits++;
            // invert velocity on bounce
            rb.linearVelocity = Vector2.Reflect(rb.linearVelocity, col.contacts[0].normal);
            if (wallHits >= hitsToStun)
                StartCoroutine(StunRoutine());
        }
    }

    private IEnumerator StunRoutine()
    {
        state = State.Stunned;
        rb.linearVelocity = Vector2.zero;
        vulnerableHitbox.enabled = true;
        yield return new WaitForSeconds(stunDuration);
        vulnerableHitbox.enabled = false;
        state = State.Idle;
    }
}
