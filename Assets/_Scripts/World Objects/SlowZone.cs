using UnityEngine;

public class SlowZone : MonoBehaviour
{
    [Header("Slow Settings")]
    [Range(0f, 1f)]
    public float slowFactor = 0.5f;
    public float walkAnimSamples = 10f;
    public float originalAnimFPS = 21f;
    public float animLerpSpeed = 5f;

    private JoystickPlayerExample player;
    private Animator playerAnim;
    private bool insideZone = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.GetComponent<JoystickPlayerExample>();
            playerAnim = other.GetComponentInChildren<Animator>();

            if (player != null)
                player.ModifySpeed(slowFactor);

            insideZone = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (player != null)
                player.ResetSpeed();

            if (playerAnim != null)
                playerAnim.speed = 1f;

            insideZone = false;
            player = null;
            playerAnim = null;
        }
    }

    private void Update()
    {
        if (!insideZone || playerAnim == null)
            return;

        float moveParam = playerAnim.GetFloat("Speed");

        float targetSpeed = (moveParam > 0.1f)
            ? walkAnimSamples / originalAnimFPS
            : 1f;

        playerAnim.speed = Mathf.Lerp(playerAnim.speed, targetSpeed, Time.deltaTime * animLerpSpeed);
    }
}
