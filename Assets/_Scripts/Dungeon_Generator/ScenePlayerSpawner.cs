using UnityEngine;

public class ScenePlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform spawnPoint;

    private void Start()
    {
        SpawnPlayer();
    }

    public void SpawnPlayer()
    {
        if (spawnPoint == null || playerPrefab == null) return;

        var existingPlayer = GameObject.FindWithTag("Player");
        if (existingPlayer != null)
            Destroy(existingPlayer);

        GameObject playerInstance = Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);

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

        CameraFollow cameraFollow = FindFirstObjectByType<CameraFollow>();
        if (cameraFollow != null)
            cameraFollow.SetTarget(playerInstance.transform);
    }
}
