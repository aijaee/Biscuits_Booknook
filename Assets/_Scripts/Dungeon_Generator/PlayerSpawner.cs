using UnityEngine;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private OnScreenButton onScreenDashButton;
    [SerializeField] private OnScreenButton onScreenAttackButton;
    [SerializeField] private VariableJoystick variableJoystick;
    [SerializeField] private GameObject deathScreen; // Assign DeathScreen in Inspector
    [SerializeField] private Image hpBarFiller; // Assign HP bar filler Image in Inspector

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
            }
        }

        Rigidbody2D rb = playerInstance.GetComponent<Rigidbody2D>();
        Animator animator = playerInstance.GetComponentInChildren<Animator>();
        JoystickPlayerExample movementScript = playerInstance.GetComponent<JoystickPlayerExample>();

        if (movementScript != null && variableJoystick != null)
        {
            movementScript.variableJoystick = variableJoystick;
        }

        DashController dash = playerInstance.GetComponent<DashController>();
        if (dash != null)
        {
            dash.onScreenDashButton = onScreenDashButton;
            dash.rb = rb;
            dash.animator = animator;
            dash.movementScript = movementScript;
        }

        MeleeAttackController melee = playerInstance.GetComponent<MeleeAttackController>();
        if (melee != null)
        {
            melee.onScreenAttackButton = onScreenAttackButton;
        }

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
