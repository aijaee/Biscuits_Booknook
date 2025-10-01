using UnityEngine;
using UnityEngine.UI;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject deathScreen; 
    [SerializeField] private Image hpBarFiller;

    private GameObject playerInstance;
    private CameraFollow cameraFollow;

    private void Awake()
    {
        cameraFollow = FindFirstObjectByType<CameraFollow>();
    }

    public void SpawnPlayer(Vector2Int spawnTile)
    {
        Vector3 spawnPos = new Vector3(spawnTile.x + 0.5f, spawnTile.y + 0.5f, 0);

        // Destroy any existing player in the scene, even if it wasn't spawned by this script
        var existingPlayer = GameObject.FindWithTag("Player");
        if (existingPlayer != null)
        {
            if (Application.isPlaying)
            {
                Destroy(existingPlayer);
            }
            else
            {
                DestroyImmediate(existingPlayer);
            }
        }

        playerInstance = Instantiate(playerPrefab, spawnPos, Quaternion.identity);

        // Set deathScreen and hpBarFiller reference on PlayerController
        var playerController = playerInstance.GetComponent<PlayerController>();
        if (playerController != null)
        {
            if (deathScreen != null)
            {
                playerController.deathScreen = deathScreen;
                deathScreen.SetActive(false);
            }
            if (hpBarFiller != null)
            {
                playerController.hpBarFiller = hpBarFiller;
                playerController.UpdateHPBar();

                // 👉 Add this here:
                var damageEffects = playerInstance.GetComponent<PlayerDamageEffects>();
                if (damageEffects != null && hpBarFiller != null)
                {
                    damageEffects.hpBarRect = hpBarFiller.transform.parent.GetComponent<RectTransform>();
                }
            }
        }

        Rigidbody2D rb = playerInstance.GetComponent<Rigidbody2D>();
        Animator animator = playerInstance.GetComponentInChildren<Animator>();
        JoystickPlayerExample movementScript = playerInstance.GetComponent<JoystickPlayerExample>();

        DashController dash = playerInstance.GetComponent<DashController>();
        if (dash != null)
        {
            dash.rb = rb;
            dash.animator = animator;
            dash.movementScript = movementScript;
        }

        MeleeAttackController melee = playerInstance.GetComponent<MeleeAttackController>();

        // Link camera follow to player
        if (cameraFollow != null)
        {
            cameraFollow.SetTarget(playerInstance.transform);
        }
    }


    public Transform GetPlayerTransform()
    {
        return playerInstance != null ? playerInstance.transform : null;
    }
}
