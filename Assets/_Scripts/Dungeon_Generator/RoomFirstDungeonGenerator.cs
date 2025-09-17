using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Random = UnityEngine.Random;

public enum RoomType
{
    Spawn,
    Combat,
    Puzzle,
    Boss
}

[Serializable]
public class RoomPrefab
{
    public GameObject prefab;
    [HideInInspector]
    public Vector2Int size;

    public void InitializeSize()
    {
        if (prefab == null) return;

        GameObject instance = GameObject.Instantiate(prefab, new Vector3(10000, 10000, 0), Quaternion.identity);
        instance.SetActive(false);

        Bounds bounds = new Bounds();

        Collider2D collider = instance.GetComponentInChildren<Collider2D>();
        if (collider != null)
        {
            bounds = collider.bounds;
        }
        else
        {
            Renderer renderer = instance.GetComponentInChildren<Renderer>();
            if (renderer != null)
                bounds = renderer.bounds;
        }

        size = new Vector2Int(
            Mathf.CeilToInt(bounds.size.x),
            Mathf.CeilToInt(bounds.size.y)
        );

        GameObject.DestroyImmediate(instance);
    }
}


public class RoomData
{
    public BoundsInt Bounds;
    public RoomType Type;
    public Vector2Int Center => (Vector2Int)Vector3Int.RoundToInt(Bounds.center);

    public RoomData(BoundsInt bounds)
    {
        Bounds = bounds;
        Type = RoomType.Combat; // Default type
    }
}



public class RoomFirstDungeonGenerator : SimpleRandomWalkDungeonGenerator
{

    [Header("Room Size Constraints")]
    [SerializeField] private int minCombatRoomWidth = 4, minCombatRoomHeight = 4;
    [SerializeField] private int minPuzzleRoomWidth = 6, minPuzzleRoomHeight = 6;
    [SerializeField] private int minBossRoomWidth = 8, minBossRoomHeight = 8;

    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private GridManager gridManager; 
    [SerializeField] private LayerMask unwalkableMask; 


    [Header("Dungeon Settings")]
    [SerializeField] private int dungeonWidth = 20, dungeonHeight = 20;
    [SerializeField][Range(0, 10)] private int offset = 1;  
    [SerializeField] private int wallBuffer = 1;
    [SerializeField] private bool randomWalkRooms = false;
    [SerializeField] private PlayerSpawner playerSpawner;
    [SerializeField] private MinimapRenderer minimapRenderer; // assign in Inspector

    [SerializeField] private int objectBuffer = 1;
    [SerializeField] private int minDistanceBetweenPrefabs = 1;
    [SerializeField] private GameObject notePrefab;
    [SerializeField] private Vector2Int noteOffset = new Vector2Int(0,2);

    [Header("Corridor Settings")]
    [SerializeField, Range(1, 5)] private int corridorWidth = 1;

    [Header("Combat Room Prefabs")]
    [SerializeField] private List<RoomPrefab> combatRoomPrefabs;
    [SerializeField] private int maxPrefabsPerCombatRoom = 3;

    [Header("Puzzle Room Prefabs")]
    [SerializeField] private List<RoomPrefab> puzzleRoomPrefabs;
    [SerializeField] private int maxPrefabsPerPuzzleRoom = 2;

    private List<RoomData> roomDataList;
    private HashSet<Vector2Int> occupiedTiles = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> prefabOccupiedTiles = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> dungeonCorridors;

    private void Start()
    {
        foreach (var prefab in puzzleRoomPrefabs)
            prefab.InitializeSize();
        foreach (var prefab in combatRoomPrefabs)
            prefab.InitializeSize();

        RunProceduralGeneration();
    }


    protected override void RunProceduralGeneration()
    {
        CreateRooms();
    }

    private void CreateRooms()
    {
        tilemapVisualizer.Clear();

        var rawRooms = ProceduralGenerationAlgorithms.BinarySpacePartitioning(
            new BoundsInt((Vector3Int)startPosition, new Vector3Int(dungeonWidth, dungeonHeight, 0)),
            minCombatRoomWidth, minCombatRoomHeight);

        roomDataList = new List<RoomData>();
        foreach (var bounds in rawRooms)
            roomDataList.Add(new RoomData(bounds));

        roomDataList.Sort((a, b) =>
            Vector2Int.Distance(a.Center, startPosition).CompareTo(Vector2Int.Distance(b.Center, startPosition)));

        if (roomDataList.Count == 0) return;

        roomDataList[0].Type = RoomType.Spawn;
        roomDataList[^1].Type = RoomType.Boss;

        int puzzleCount = Mathf.Clamp(Random.Range(2, 2), 0, roomDataList.Count - 2);
        var shuffleCandidates = roomDataList.GetRange(1, roomDataList.Count - 2);
        Shuffle(shuffleCandidates);
        for (int i = 0; i < puzzleCount; i++)
            shuffleCandidates[i].Type = RoomType.Puzzle;

        foreach (var room in roomDataList)
            EnsureRoomMinSize(room);

        HashSet<Vector2Int> floor = randomWalkRooms
            ? CreateRoomsRandomly(roomDataList)
            : CreateSimpleRooms(roomDataList);

        List<Vector2Int> roomCenters = roomDataList.ConvertAll(r => r.Center);
        HashSet<Vector2Int> corridors = ConnectRooms(roomCenters);
        HashSet<Vector2Int> thickCorridors = new HashSet<Vector2Int>();
        int halfWidth = corridorWidth / 2;
        foreach (var pos in corridors)
        {
            for (int dx = -halfWidth; dx <= halfWidth; dx++)
            {
                for (int dy = -halfWidth; dy <= halfWidth; dy++)
                {
                    thickCorridors.Add(pos + new Vector2Int(dx, dy));
                }
            }
        }
        floor.UnionWith(thickCorridors);

        tilemapVisualizer.PaintFloorTiles(floor);
        WallGenerator.CreateWalls(floor, tilemapVisualizer);

        // --- Initialize GridManager for pathfinding ---
        if (gridManager != null)
        {
            gridManager.Initialize(new Vector2Int(dungeonWidth, dungeonHeight), 1f, unwalkableMask);
        }
        else
        {
            Debug.LogWarning("GridManager reference not set in RoomFirstDungeonGenerator!");
        }

        {
            Vector2Int spawnPos = FindClosestFloorTile(roomDataList[0].Center, floor);
            playerSpawner.SpawnPlayer(spawnPos);

            // Minimap integration
            if (minimapRenderer != null)
            {
                var roomTilesList = new List<HashSet<Vector2Int>>();
                foreach (var room in roomDataList)
                {
                    var tiles = randomWalkRooms
                        ? CreateRoomsRandomly(new List<RoomData> { room })
                        : CreateSimpleRooms(new List<RoomData> { room });
                    roomTilesList.Add(tiles);
                }
                minimapRenderer.SetAllRoomTiles(roomTilesList);
                minimapRenderer.SetCurrentRoomTiles(roomTilesList[0]);
                var puzzleCenters = roomDataList
                    .Where(r => r.Type == RoomType.Puzzle)
                    .Select(r => r.Center)
                    .ToList();
                minimapRenderer.SetPuzzleRoomCenters(puzzleCenters);
                minimapRenderer.showPuzzleRoomIcons = true;

                minimapRenderer.SetPlayerPosition(spawnPos);
                minimapRenderer.DrawMinimap(floor, new Vector2Int(dungeonWidth, dungeonHeight));
            }
        }

          if (notePrefab != null)
        {
            Vector2Int playerTile = FindClosestFloorTile(roomDataList[0].Center, floor);
            Vector2Int spawnTile = playerTile + noteOffset;

            RoomData playerRoom = roomDataList[0];
            spawnTile.x = Mathf.Clamp(spawnTile.x, playerRoom.Bounds.xMin + wallBuffer, playerRoom.Bounds.xMax - wallBuffer);
            spawnTile.y = Mathf.Clamp(spawnTile.y, playerRoom.Bounds.yMin + wallBuffer, playerRoom.Bounds.yMax - wallBuffer);

            Instantiate(notePrefab, new Vector3(spawnTile.x, spawnTile.y, 0), Quaternion.identity, this.transform);
        }

        PlacePrefabsInCombatRooms();
        PlacePrefabsInPuzzleRooms();

        foreach (var room in roomDataList)
        {
            Debug.Log($"Room at {room.Center} is a {room.Type} room with bounds {room.Bounds}");
        }

        // --- Set valid spawn tiles for EnemySpawner ---
        if (enemySpawner != null)
        {
            enemySpawner.SetValidSpawnTiles(floor);
        }

        // --- Spawn enemies ---
        if (enemySpawner != null && floor.Count > 0)
        {
            int enemyCount = enemySpawner.EnemyCount;
            List<Vector2Int> enemySpawns = enemySpawner.GetRandomWalkableTiles(enemyCount);
            enemySpawner.SpawnEnemies(enemySpawns);
        }
    }

    private void PlacePrefabsInPuzzleRooms()
    {
        HashSet<Vector2Int> occupiedTiles = new HashSet<Vector2Int>();

        foreach (var room in roomDataList)
        {
            if (room.Type != RoomType.Puzzle) continue;

            int placedCount = 0;
            int attempts = 0;
            int maxAttempts = 10 * maxPrefabsPerPuzzleRoom;

            // Keep track of used puzzle prefab indices for this room or overall
            HashSet<int> usedPrefabIndices = new HashSet<int>();

            while (placedCount < maxPrefabsPerPuzzleRoom && attempts < maxAttempts)
            {
                attempts++;

                // Get list of available prefabs (not used yet)
                List<int> availableIndices = new List<int>();
                for (int i = 0; i < puzzleRoomPrefabs.Count; i++)
                {
                    if (!usedPrefabIndices.Contains(i))
                        availableIndices.Add(i);
                }

                // No more unique prefabs available
                if (availableIndices.Count == 0)
                    break;

                // Pick a random available prefab index
                int randomIndex = availableIndices[Random.Range(0, availableIndices.Count)];
                var prefabData = puzzleRoomPrefabs[randomIndex];
                Vector2Int prefabSize = prefabData.size;

                Vector2Int roomCenter = room.Center;
                Vector2Int spawnPos = new Vector2Int(
                    roomCenter.x - prefabSize.x / 2,
                    roomCenter.y - prefabSize.y / 2
                );

                if (!IsWithinRoomBounds(room, spawnPos, prefabSize) || IsAreaOccupied(spawnPos, prefabSize, occupiedTiles))
                    continue;

                // Mark prefab index as used to prevent reuse
                usedPrefabIndices.Add(randomIndex);
        
                MarkAreaOccupied(spawnPos, prefabSize, occupiedTiles);
                Vector3 worldPos = new Vector3(spawnPos.x, spawnPos.y, 0);
                GameObject roomInstance = Instantiate(prefabData.prefab, worldPos, Quaternion.identity, this.transform);
                // Randomize chest positions
                var chestTransforms = roomInstance.GetComponentsInChildren<Transform>()
                    .Where(t => t.CompareTag("Chest")).ToList();
                List<Vector2> placedChestPositions = new List<Vector2>();
                foreach (var chest in chestTransforms)
                {
                    Vector2 pos;
                    int chestAttempts = 0;
                    do
                    {
                        float localX = Random.Range(wallBuffer, prefabSize.x - wallBuffer);
                        float localY = Random.Range(wallBuffer, prefabSize.y - wallBuffer);
                        pos = new Vector2(localX, localY);
                        chestAttempts++;
                    } while (placedChestPositions.Any(p => Vector2.Distance(p, pos) < minDistanceBetweenPrefabs) && chestAttempts < 10);
                    placedChestPositions.Add(pos);
                    chest.localPosition = new Vector3(pos.x, pos.y, chest.localPosition.z);
                }

                placedCount++;
            }
        }
    }


    private void EnsureRoomMinSize(RoomData room)
    {
        int minWidth = minCombatRoomWidth;
        int minHeight = minCombatRoomHeight;

        switch (room.Type)
        {
            case RoomType.Boss:
                minWidth = minBossRoomWidth;
                minHeight = minBossRoomHeight;
                break;
            case RoomType.Puzzle:
                minWidth = minPuzzleRoomWidth;
                minHeight = minPuzzleRoomHeight;
                break;
        }

        int widthDiff = minWidth - room.Bounds.size.x;
        int heightDiff = minHeight - room.Bounds.size.y;

        if (widthDiff > 0 || heightDiff > 0)
        {
            int newWidth = Mathf.Clamp(room.Bounds.size.x + widthDiff, minWidth, dungeonWidth);
            int newHeight = Mathf.Clamp(room.Bounds.size.y + heightDiff, minHeight, dungeonHeight);
            Vector3Int newMin = room.Bounds.min - new Vector3Int(widthDiff / 2, heightDiff / 2, 0);
            newMin.x = Mathf.Clamp(newMin.x, startPosition.x, startPosition.x + dungeonWidth - newWidth);
            newMin.y = Mathf.Clamp(newMin.y, startPosition.y, startPosition.y + dungeonHeight - newHeight);
            room.Bounds = new BoundsInt(newMin, new Vector3Int(newWidth, newHeight, 0));
        }
    }

    private bool IsWithinRoomBounds(RoomData room, Vector2Int spawnPos, Vector2Int size)
    {
        return spawnPos.x >= room.Bounds.xMin + wallBuffer &&
        spawnPos.y >= room.Bounds.yMin + wallBuffer &&
        spawnPos.x + size.x <= room.Bounds.xMax - wallBuffer &&
        spawnPos.y + size.y <= room.Bounds.yMax - wallBuffer;
    }


    private Vector2Int FindClosestFloorTile(Vector2Int center, HashSet<Vector2Int> floorTiles)
    {
        Vector2Int closest = center;
        float minDist = float.MaxValue;
        foreach (var floorTile in floorTiles)
        {
            float dist = Vector2Int.Distance(floorTile, center);
            if (dist < minDist)
            {
                minDist = dist;
                closest = floorTile;
            }
        }
        return closest;
    }

    private HashSet<Vector2Int> CreateSimpleRooms(List<RoomData> roomList)
    {
        HashSet<Vector2Int> floor = new();
        foreach (var room in roomList)
        {
            var bounds = room.Bounds;
            for (int x = offset; x < bounds.size.x - offset; x++)
            {
                for (int y = offset; y < bounds.size.y - offset; y++)
                {
                    floor.Add((Vector2Int)bounds.min + new Vector2Int(x, y));
                }
            }
        }
        return floor;
    }

    private HashSet<Vector2Int> CreateRoomsRandomly(List<RoomData> roomList)
    {
        HashSet<Vector2Int> floor = new();
        foreach (var room in roomList)
        {
            var center = room.Center;
            var roomFloor = RunRandomWalk(randomWalkParameters, center);
            foreach (var pos in roomFloor)
            {
                if (pos.x >= room.Bounds.xMin + offset && pos.x <= room.Bounds.xMax - offset &&
                    pos.y >= room.Bounds.yMin + offset && pos.y <= room.Bounds.yMax - offset)
                {
                    floor.Add(pos);
                }
            }
        }
        return floor;
    }

    private HashSet<Vector2Int> ConnectRooms(List<Vector2Int> roomCenters)
    {
        HashSet<Vector2Int> corridors = new();
        var currentRoomCenter = roomCenters[Random.Range(0, roomCenters.Count)];
        roomCenters.Remove(currentRoomCenter);

        while (roomCenters.Count > 0)
        {
            Vector2Int closest = FindClosestPointTo(currentRoomCenter, roomCenters);
            roomCenters.Remove(closest);
            HashSet<Vector2Int> newCorridor = CreateCorridor(currentRoomCenter, closest);
            currentRoomCenter = closest;
            corridors.UnionWith(newCorridor);
        }
        return corridors;
    }

    private HashSet<Vector2Int> CreateCorridor(Vector2Int currentRoomCenter, Vector2Int destination)
    {
        HashSet<Vector2Int> corridor = new();
        var position = currentRoomCenter;
        corridor.Add(position);

        while (position.y != destination.y)
        {
            position += (destination.y > position.y) ? Vector2Int.up : Vector2Int.down;
            corridor.Add(position);
        }

        while (position.x != destination.x)
        {
            position += (destination.x > position.x) ? Vector2Int.right : Vector2Int.left;
            corridor.Add(position);
        }
        return corridor;
    }

    private Vector2Int FindClosestPointTo(Vector2Int currentRoomCenter, List<Vector2Int> roomCenters)
    {
        Vector2Int closest = Vector2Int.zero;
        float distance = float.MaxValue;
        foreach (var position in roomCenters)
        {
            float currentDistance = Vector2.Distance(position, currentRoomCenter);
            if (currentDistance < distance)
            {
                distance = currentDistance;
                closest = position;
            }
        }
        return closest;
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int rand = Random.Range(i, list.Count);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }

    private void PlacePrefabsInCombatRooms()
    {
        HashSet<Vector2Int> occupiedTiles = new HashSet<Vector2Int>();

        foreach (var room in roomDataList)
        {
            if (room.Type != RoomType.Combat) continue;

            int placedCount = 0;
            int attempts = 0;
            int maxAttempts = 10 * maxPrefabsPerCombatRoom;

            while (placedCount < maxPrefabsPerCombatRoom && attempts < maxAttempts)
            {
                attempts++;
                var prefabData = combatRoomPrefabs[Random.Range(0, combatRoomPrefabs.Count)];
                Vector2Int prefabSize = prefabData.size;

                int minX = room.Bounds.xMin + wallBuffer;
                int maxX = room.Bounds.xMax - wallBuffer - prefabSize.x + 1;
                int minY = room.Bounds.yMin + wallBuffer;
                int maxY = room.Bounds.yMax - wallBuffer - prefabSize.y + 1;

                if (maxX < minX || maxY < minY) break;

                bool placed = false;
                int placementAttempts = 0;

                while (!placed && placementAttempts < 5)
                {
                    placementAttempts++;
                    int spawnX = Random.Range(minX, maxX + 1);
                    int spawnY = Random.Range(minY, maxY + 1);
                    Vector2Int spawnPos = new Vector2Int(spawnX, spawnY);

                    if (!IsAreaOccupied(spawnPos, prefabSize, occupiedTiles))
                    {
                        MarkAreaOccupied(spawnPos, prefabSize, occupiedTiles);
                        Vector3 worldPos = new Vector3(spawnX, spawnY, 0);
                        Instantiate(prefabData.prefab, worldPos, Quaternion.identity, this.transform);
                        placedCount++;
                        placed = true;
                    }
                }

                if (!placed) break;
            }
        }
    }

    private bool IsAreaOccupied(Vector2Int startPos, Vector2Int size, HashSet<Vector2Int> occupied)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                if (occupied.Contains(new Vector2Int(startPos.x + x, startPos.y + y)))
                    return true;
            }
        }
        return false;
    }

    private void MarkAreaOccupied(Vector2Int startPos, Vector2Int size, HashSet<Vector2Int> occupied)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                occupied.Add(new Vector2Int(startPos.x + x, startPos.y + y));
            }
        }
    }
}
