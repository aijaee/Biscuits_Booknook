using System.Collections.Generic;
using UnityEngine;

public class BossRoomGenerator : AbstractDungeonGenerator
{
    [SerializeField] private Vector2Int roomSize = new Vector2Int(20, 15);
    

    [Header("Prefabs")]
    [SerializeField] private BossSpawner bossSpawner;
    [SerializeField] private PlayerSpawner playerSpawner;
    private void Start()
    {
        GenerateDungeon();
    }

    protected override void RunProceduralGeneration()
    {
        var floor = new HashSet<Vector2Int>();
        Vector2Int bottomLeft = startPosition - new Vector2Int(roomSize.x / 2, roomSize.y / 2);
        for (int x = 0; x < roomSize.x; x++)
            for (int y = 0; y < roomSize.y; y++)
                floor.Add(bottomLeft + new Vector2Int(x, y));

        tilemapVisualizer.Clear();
        tilemapVisualizer.PaintFloorTiles(floor);

        WallGenerator.CreateWalls(floor, tilemapVisualizer, new Dictionary<Vector2Int, RoomData>());

        SpawnBossRoomContents(floor, bottomLeft);
    }

    private void SpawnBossRoomContents(HashSet<Vector2Int> floor, Vector2Int bottomLeft)
    {
        if (bossSpawner != null)
            bossSpawner.SpawnBoss(startPosition, bottomLeft, roomSize);
            
        Vector2Int playerPos = bottomLeft + new Vector2Int(roomSize.x / 2, 1);
        if (playerSpawner != null)
            playerSpawner.SpawnPlayer(playerPos);

        var available = new List<Vector2Int>(floor);
        available.Remove(startPosition);
        available.Remove(playerPos);
    }

}