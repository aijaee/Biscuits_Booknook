using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int enemyCount = 10; // Public in Inspector

    private List<GameObject> enemyInstances = new List<GameObject>();

    private void Awake()
    {
        // Add any setup you need here (e.g., find managers, camera, etc.)
    }

    private void Start()
    {
        // Remove gridManager configuration, handled by dungeon generator

        // Example: spawn at tile (0,0) on play
        var spawnTiles = new List<Vector2Int>();
        for (int i = 0; i < enemyCount; i++)
        {
            spawnTiles.Add(new Vector2Int(0, 0)); // Replace with your spawn logic
        }
        SpawnEnemies(spawnTiles);
    }

    public void SpawnEnemies(List<Vector2Int> spawnTiles)
    {
        // Destroy old enemies
        foreach (var enemy in enemyInstances)
        {
            if (enemy != null) Destroy(enemy);
        }
        enemyInstances.Clear();

        foreach (var tile in spawnTiles)
        {
            Vector3 spawnPos = new Vector3(tile.x + 0.5f, tile.y + 0.5f, 0);
            var enemyInstance = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            enemyInstances.Add(enemyInstance);

            // Example: Setup enemy (add more as needed)
            EnemyController enemyController = enemyInstance.GetComponent<EnemyController>();
            // if (enemyController != null) { enemyController.SetTarget(...); }
        }
    }

    public List<Transform> GetEnemyTransforms()
    {
        List<Transform> transforms = new List<Transform>();
        foreach (var enemy in enemyInstances)
        {
            if (enemy != null) transforms.Add(enemy.transform);
        }
        return transforms;
    }

    public int EnemyCount => enemyCount;
}