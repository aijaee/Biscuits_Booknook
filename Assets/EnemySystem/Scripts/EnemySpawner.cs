using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int enemyCount = 10; // Public in Inspector
    [SerializeField] private GridManager gridManager; // Assign in Inspector or find at runtime

    private List<GameObject> enemyInstances = new List<GameObject>();

    // Store valid spawn tiles (floor tiles) set by the dungeon generator
    private HashSet<Vector2Int> validSpawnTiles = null;

    private void Awake()
    {
        // Add any setup you need here (e.g., find managers, camera, etc.)
        if (gridManager == null)
        {
            gridManager = Object.FindFirstObjectByType<GridManager>();
            if (gridManager == null)
                Debug.LogError("EnemySpawner: No GridManager found in the scene!");
        }
    }

    private void Start()
    {
        // Remove gridManager configuration, handled by dungeon generator

        // Example: spawn at tile (0,0) on play
        var spawnTiles = GetRandomWalkableTiles(enemyCount);
        SpawnEnemies(spawnTiles);
    }

    // Call this from the dungeon generator after floor is generated
    public void SetValidSpawnTiles(HashSet<Vector2Int> validTiles)
    {
        validSpawnTiles = validTiles;
    }

    // Returns a list of random walkable tile positions within the grid and valid floor tiles
    public List<Vector2Int> GetRandomWalkableTiles(int count)
    {
        var tiles = new List<Vector2Int>();
        if (gridManager == null || gridManager.GetGrid() == null)
        {
            Debug.LogError("EnemySpawner: GridManager or grid is not initialized.");
            return tiles;
        }

        var grid = gridManager.GetGrid();
        int width = gridManager.gridSize.x;
        int height = gridManager.gridSize.y;

        var walkableTiles = new List<Vector2Int>();
        // Only consider tiles that are walkable AND in validSpawnTiles (the actual floor)
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (grid[x, y].walkable && validSpawnTiles != null && validSpawnTiles.Contains(pos))
                    walkableTiles.Add(pos);
            }
        }

        if (walkableTiles.Count == 0)
        {
            Debug.LogWarning("EnemySpawner: No walkable floor tiles found!");
            return tiles;
        }

        // Shuffle and pick random tiles
        for (int i = 0; i < count && walkableTiles.Count > 0; i++)
        {
            int idx = Random.Range(0, walkableTiles.Count);
            tiles.Add(walkableTiles[idx]);
            walkableTiles.RemoveAt(idx);
        }
        return tiles;
    }

    public void SpawnEnemies(List<Vector2Int> spawnTiles)
    {
        // Destroy old enemies
        foreach (var enemy in enemyInstances)
        {
            if (enemy != null) Destroy(enemy);
        }
        enemyInstances.Clear();

        if (gridManager == null || gridManager.GetGrid() == null)
        {
            Debug.LogError("EnemySpawner: GridManager or grid is not initialized.");
            return;
        }
        int width = gridManager.gridSize.x;
        int height = gridManager.gridSize.y;

        foreach (var tile in spawnTiles)
        {
            // Clamp tile positions to grid bounds to prevent out-of-bounds spawns
            int x = Mathf.Clamp(tile.x, 0, width - 1);
            int y = Mathf.Clamp(tile.y, 0, height - 1);

            // Only spawn if the tile is walkable
            var grid = gridManager.GetGrid();
            if (!grid[x, y].walkable)
                continue;

            Vector3 spawnPos = new Vector3(x + 0.5f, y + 0.5f, 0);
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