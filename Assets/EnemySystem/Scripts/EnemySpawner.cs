using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int enemyCount = 10; 
    [SerializeField] private GridManager gridManager; 

    private List<GameObject> enemyInstances = new List<GameObject>();

    private HashSet<Vector2Int> validSpawnTiles = null;

    private void Awake()
    {
        if (gridManager == null)
        {
            gridManager = Object.FindFirstObjectByType<GridManager>();
            if (gridManager == null)
                Debug.LogError("EnemySpawner: No GridManager found in the scene!");
        }
    }

    private void Start()
    {
        var spawnTiles = GetRandomWalkableTiles(enemyCount);
        SpawnEnemies(spawnTiles);
    }

    public void SetValidSpawnTiles(HashSet<Vector2Int> validTiles)
    {
        validSpawnTiles = validTiles;
    }

    public List<Vector2Int> GetRandomWalkableTiles(int count, int wallBuffer = 1)
    {
        var tiles = new List<Vector2Int>();
        if (gridManager == null || gridManager.GetGrid() == null)
            return tiles;

        var grid = gridManager.GetGrid();
        int width = gridManager.gridSize.x;
        int height = gridManager.gridSize.y;

        var walkableTiles = new List<Vector2Int>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);

                if (!grid[x, y].walkable || (validSpawnTiles != null && !validSpawnTiles.Contains(pos)))
                    continue;

                bool safe = true;
                for (int dx = -wallBuffer; dx <= wallBuffer && safe; dx++)
                {
                    for (int dy = -wallBuffer; dy <= wallBuffer && safe; dy++)
                    {
                        int nx = x + dx;
                        int ny = y + dy;
                        if (nx < 0 || nx >= width || ny < 0 || ny >= height || !grid[nx, ny].walkable)
                            safe = false;
                    }
                }

                if (safe)
                    walkableTiles.Add(pos);
            }
        }

        if (walkableTiles.Count == 0)
            return tiles;

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
            int x = Mathf.Clamp(tile.x, 0, width - 1);
            int y = Mathf.Clamp(tile.y, 0, height - 1);

            var grid = gridManager.GetGrid();
            if (!grid[x, y].walkable)
                continue;

            Vector3 spawnPos = new Vector3(x + 0.5f, y + 0.5f, 0);
            var enemyInstance = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            enemyInstances.Add(enemyInstance);
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