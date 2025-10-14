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

public enum PuzzleType
{
    Implicit,
    Explicit
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
    public PuzzleType PuzzleSubtype;
    public Vector2Int Center => (Vector2Int)Vector3Int.RoundToInt(Bounds.center);

    public RoomData(BoundsInt bounds)
    {
        Bounds = bounds;
        Type = RoomType.Combat; // Default type
    }
}



public class RoomFirstDungeonGenerator : SimpleRandomWalkDungeonGenerator
{

    [Header("Room Spawn Settings")]
    [SerializeField] private int minPuzzleRooms = 5;
    [SerializeField] private int maxPuzzleRooms = 5;
    [SerializeField] private int maxRoomWidthGeneral = 12;
    [SerializeField] private int maxRoomHeightGeneral = 12;


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
    [SerializeField] private int roomSpacing = 2;
    [SerializeField] private bool randomWalkRooms = false;
    [SerializeField] private PlayerSpawner playerSpawner;
    [SerializeField] private MinimapRenderer minimapRenderer; // assign in Inspector

    [SerializeField] private int objectBuffer = 1;
    // [SerializeField] private int minDistanceBetweenPrefabs = 1; // ----- Not used -----
    [SerializeField] private GameObject notePrefab;
    [SerializeField] private Vector2Int noteOffset = new Vector2Int(0, 2);

    [Header("Corridor Settings")]
    [SerializeField, Range(1, 5)] private int corridorWidth = 1;

    [Header("Combat Room Prefabs")]
    [SerializeField] private List<RoomPrefab> combatRoomPrefabs;
    [SerializeField] private int maxPrefabsPerCombatRoom = 3;

    [Header("Puzzle Room Prefabs")]
    [SerializeField] private int maxPrefabsPerPuzzleRoom = 1;
    [Header("Puzzle Room Prefabs - Implicit")]
    [SerializeField] private List<RoomPrefab> implicitPuzzleRoomPrefabs;

    [Header("Puzzle Room Prefabs - Explicit")]
    [SerializeField] private List<RoomPrefab> explicitPuzzleRoomPrefabs;

    private List<RoomData> roomDataList;
    private HashSet<Vector2Int> occupiedTiles = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> prefabOccupiedTiles = new HashSet<Vector2Int>();
    private HashSet<Vector2Int> dungeonCorridors;
    [Header("Boss Room Prefabs")]
    [SerializeField] private List<RoomPrefab> bossRoomPrefabs;
    [SerializeField] private int maxPrefabsPerBossRoom = 1;


    private void Start()
    {
        foreach (var prefab in implicitPuzzleRoomPrefabs)
            prefab.InitializeSize();
        foreach (var prefab in explicitPuzzleRoomPrefabs)
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
            minCombatRoomWidth + roomSpacing * 2,
            minCombatRoomHeight + roomSpacing * 2);

        // Convert raw rooms to RoomData and apply spacing
        roomDataList = rawRooms.Select(bounds =>
        {
            BoundsInt spacedBounds = bounds;
            spacedBounds.xMin += roomSpacing;
            spacedBounds.xMax -= roomSpacing;
            spacedBounds.yMin += roomSpacing;
            spacedBounds.yMax -= roomSpacing;
            return new RoomData(spacedBounds);
        }).ToList();

        // Now split oversized rooms
        roomDataList = SplitOversizedRooms(roomDataList);

        // Sort by distance to start
        roomDataList.Sort((a, b) =>
            Vector2Int.Distance(a.Center, startPosition).CompareTo(Vector2Int.Distance(b.Center, startPosition)));

        if (roomDataList.Count == 0) return;

        roomDataList[0].Type = RoomType.Spawn;
        roomDataList[^1].Type = RoomType.Boss;

        // Assign puzzle rooms
        int totalPuzzleRooms = Random.Range(minPuzzleRooms, maxPuzzleRooms + 1);
        int implicitCount, explicitCount;
        if (Random.value < 0.5f)
        {
            implicitCount = Mathf.Min(2, totalPuzzleRooms);
            explicitCount = Mathf.Min(totalPuzzleRooms - implicitCount, 3);
        }
        else
        {
            explicitCount = Mathf.Min(2, totalPuzzleRooms);
            implicitCount = Mathf.Min(totalPuzzleRooms - explicitCount, 3);
        }

        var shuffleCandidates = roomDataList.GetRange(1, roomDataList.Count - 2);
        Shuffle(shuffleCandidates);

        for (int i = 0; i < implicitCount && i < shuffleCandidates.Count; i++)
        {
            shuffleCandidates[i].Type = RoomType.Puzzle;
            shuffleCandidates[i].PuzzleSubtype = PuzzleType.Implicit;
        }

        for (int i = implicitCount; i < implicitCount + explicitCount && i < shuffleCandidates.Count; i++)
        {
            shuffleCandidates[i].Type = RoomType.Puzzle;
            shuffleCandidates[i].PuzzleSubtype = PuzzleType.Explicit;
        }

        // Ensure rooms meet min size after spacing adjustments
        foreach (var room in roomDataList)
            EnsureRoomMinSize(room);

        // Create floor tiles
        HashSet<Vector2Int> floor = randomWalkRooms
            ? CreateRoomsRandomly(roomDataList)
            : CreateSimpleRooms(roomDataList);

        // Connect rooms with corridors
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

        // Map floor tiles to rooms
        Dictionary<Vector2Int, RoomData> floorToRoom = new();
        foreach (var room in roomDataList)
        {
            var bounds = room.Bounds;
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    var pos = new Vector2Int(x, y);
                    if (floor.Contains(pos))
                        floorToRoom[pos] = room;
                }
            }
        }

        // Create walls
        WallGenerator.CreateWalls(floor, tilemapVisualizer, floorToRoom);

        // --- Initialize GridManager for pathfinding ---
        if (gridManager != null)
            gridManager.Initialize(new Vector2Int(dungeonWidth, dungeonHeight), 1f, unwalkableMask);

        PlacePrefabsInCombatRooms();
        PlacePrefabsInPuzzleRooms();
        PlacePrefabsInBossRooms();

        // Spawn player
        Vector2Int spawnPos = FindClosestFloorTile(roomDataList[0].Center, floor);
        playerSpawner.SpawnPlayer(spawnPos);

        // Minimap
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

        // Skip object placement for now
        foreach (var room in roomDataList)
            Debug.Log($"Room at {room.Center} is a {room.Type} room with bounds {room.Bounds}");

        // Enemy spawns
        if (enemySpawner != null)
        {
            enemySpawner.SetValidSpawnTiles(floor);
            if (floor.Count > 0)
            {
                int enemyCount = enemySpawner.EnemyCount;
                List<Vector2Int> enemySpawns = enemySpawner.GetRandomWalkableTiles(enemyCount);
                enemySpawner.SpawnEnemies(enemySpawns);
            }
        }
    }

    private void PlacePrefabsInPuzzleRooms()
    {
        HashSet<Vector2Int> occupiedTiles = new HashSet<Vector2Int>();
        HashSet<int> usedImplicitPrefabs = new HashSet<int>();
        HashSet<int> usedExplicitPrefabs = new HashSet<int>();

        foreach (var room in roomDataList)
        {
            if (room.Type != RoomType.Puzzle) continue;

            List<RoomPrefab> prefabPool;
            HashSet<int> usedPrefabIndices;

            if (room.PuzzleSubtype == PuzzleType.Implicit)
            {
                prefabPool = implicitPuzzleRoomPrefabs;
                usedPrefabIndices = usedImplicitPrefabs;
            }
            else
            {
                prefabPool = explicitPuzzleRoomPrefabs;
                usedPrefabIndices = usedExplicitPrefabs;
            }

            int placedCount = 0;
            int attempts = 0;
            int maxAttempts = 10 * maxPrefabsPerPuzzleRoom;

            while (placedCount < maxPrefabsPerPuzzleRoom && attempts < maxAttempts)
            {
                attempts++;

                List<int> availableIndices = new List<int>();
                for (int i = 0; i < prefabPool.Count; i++)
                {
                    if (!usedPrefabIndices.Contains(i))
                        availableIndices.Add(i);
                }
                if (availableIndices.Count == 0) break;

                int randomIndex = availableIndices[Random.Range(0, availableIndices.Count)];
                var prefabData = prefabPool[randomIndex];
                Vector2Int prefabSize = prefabData.size;

                Vector2Int roomCenter = room.Center;
                Vector2Int spawnPos = new Vector2Int(
                    roomCenter.x - prefabSize.x / 2,
                    roomCenter.y - prefabSize.y / 2
                );

                if (!IsWithinRoomBounds(room, spawnPos, prefabSize, objectBuffer) || IsAreaOccupied(spawnPos, prefabSize, occupiedTiles))
                    continue;


                usedPrefabIndices.Add(randomIndex);
                MarkAreaOccupied(spawnPos - new Vector2Int(objectBuffer, objectBuffer), 
                                prefabSize + new Vector2Int(objectBuffer * 2, objectBuffer * 2), 
                                occupiedTiles);

                Vector3 worldPos = new Vector3(spawnPos.x, spawnPos.y, 0);
                GameObject roomInstance = Instantiate(prefabData.prefab, worldPos, Quaternion.identity, this.transform);

                var chestTransforms = roomInstance.GetComponentsInChildren<Transform>()
                    .Where(t => t.CompareTag("Chest")).ToList();

                if (chestTransforms.Count > 1)
                {
                    List<Vector3> chestPositions = chestTransforms.Select(c => c.localPosition).ToList();
                    for (int i = 0; i < chestPositions.Count; i++)
                    {
                        int swapIndex = Random.Range(i, chestPositions.Count);
                        (chestPositions[i], chestPositions[swapIndex]) = (chestPositions[swapIndex], chestPositions[i]);
                    }
                    for (int i = 0; i < chestTransforms.Count; i++)
                    {
                        chestTransforms[i].localPosition = chestPositions[i];
                    }
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

        minWidth = Mathf.Max(minWidth, wallBuffer * 2 + 1);
        minHeight = Mathf.Max(minHeight, wallBuffer * 2 + 1);

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

    private bool IsWithinRoomBounds(RoomData room, Vector2Int spawnPos, Vector2Int size, int extraBuffer = 0)
    {
        int totalBuffer = wallBuffer + extraBuffer;
        return spawnPos.x >= room.Bounds.xMin + totalBuffer &&
            spawnPos.y >= room.Bounds.yMin + totalBuffer &&
            spawnPos.x + size.x <= room.Bounds.xMax - totalBuffer &&
            spawnPos.y + size.y <= room.Bounds.yMax - totalBuffer;
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
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();

        foreach (var room in roomList)
        {
            var bounds = room.Bounds;
            HashSet<Vector2Int> roomTiles = new HashSet<Vector2Int>();

            int offsetFloor = Mathf.Min(wallBuffer, (bounds.size.x - 1) / 2); // floor offset for walls
            int offsetFloorY = Mathf.Min(wallBuffer, (bounds.size.y - 1) / 2);

            for (int x = offsetFloor; x < bounds.size.x - offsetFloor; x++)
            {
                for (int y = offsetFloorY; y < bounds.size.y - offsetFloorY; y++)
                {
                    roomTiles.Add((Vector2Int)bounds.min + new Vector2Int(x, y));
                }
            }

            if (roomTiles.Count == 0)
                roomTiles.Add(room.Center);

            floor.UnionWith(roomTiles);
        }

        return floor;
    }

    private HashSet<Vector2Int> CreateRoomsRandomly(List<RoomData> roomList)
    {
        HashSet<Vector2Int> floor = new HashSet<Vector2Int>();

        foreach (var room in roomList)
        {
            var center = room.Center;
            var roomFloor = RunRandomWalk(randomWalkParameters, center);

            int offsetFloorX = Mathf.Min(wallBuffer, (room.Bounds.size.x - 1) / 2);
            int offsetFloorY = Mathf.Min(wallBuffer, (room.Bounds.size.y - 1) / 2);

            foreach (var pos in roomFloor)
            {
                if (pos.x >= room.Bounds.xMin + offsetFloorX && pos.x <= room.Bounds.xMax - offsetFloorX &&
                    pos.y >= room.Bounds.yMin + offsetFloorY && pos.y <= room.Bounds.yMax - offsetFloorY)
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
        prefabOccupiedTiles.Clear();

        foreach (var room in roomDataList)
        {
            if (room.Type != RoomType.Combat) continue;

            Vector2Int roomCenter = room.Center;
            int placedCount = 0;
            int attempts = 0;
            int maxAttempts = 10 * maxPrefabsPerCombatRoom;

            while (placedCount < maxPrefabsPerCombatRoom && attempts < maxAttempts)
            {
                attempts++;
                var prefabData = combatRoomPrefabs[Random.Range(0, combatRoomPrefabs.Count)];
                Vector2Int prefabSize = prefabData.size;

                int spreadRangeX = Mathf.Max(1, (room.Bounds.size.x - prefabSize.x) / 4);
                int spreadRangeY = Mathf.Max(1, (room.Bounds.size.y - prefabSize.y) / 4);

                Vector2Int spawnPos = new Vector2Int(
                    roomCenter.x - prefabSize.x / 2 + Random.Range(-spreadRangeX, spreadRangeX + 1),
                    roomCenter.y - prefabSize.y / 2 + Random.Range(-spreadRangeY, spreadRangeY + 1)
                );

                if (!IsWithinRoomBounds(room, spawnPos, prefabSize, objectBuffer)) continue;

                if (IsAreaOccupiedWithBuffer(spawnPos, prefabSize, objectBuffer, prefabOccupiedTiles)) continue;

                MarkAreaOccupiedWithBuffer(spawnPos, prefabSize, objectBuffer, prefabOccupiedTiles);

                Vector3 worldPos = new Vector3(spawnPos.x, spawnPos.y, 0);
                Instantiate(prefabData.prefab, worldPos, Quaternion.identity, this.transform);

                placedCount++;
            }
        }
    }

    private void PlacePrefabsInBossRooms()
    {
        HashSet<Vector2Int> occupiedTiles = new HashSet<Vector2Int>();

        foreach (var room in roomDataList)
        {
            if (room.Type != RoomType.Boss) continue;

            int placedCount = 0;
            int attempts = 0;
            int maxAttempts = 10 * maxPrefabsPerBossRoom;

            while (placedCount < maxPrefabsPerBossRoom && attempts < maxAttempts)
            {
                attempts++;
                var prefabData = bossRoomPrefabs[Random.Range(0, bossRoomPrefabs.Count)];
                Vector2Int prefabSize = prefabData.size;

                Vector2Int roomCenter = room.Center;
                Vector2Int spawnPos = new Vector2Int(
                    roomCenter.x - prefabSize.x / 2,
                    roomCenter.y - prefabSize.y / 2
                );

                if (!IsWithinRoomBounds(room, spawnPos, prefabSize, objectBuffer) || 
                    IsAreaOccupied(spawnPos, prefabSize, occupiedTiles))
                    continue;

                MarkAreaOccupied(spawnPos - new Vector2Int(objectBuffer, objectBuffer),
                                prefabSize + new Vector2Int(objectBuffer * 2, objectBuffer * 2),
                                occupiedTiles);

                Vector3 worldPos = new Vector3(spawnPos.x, spawnPos.y, 0);
                Instantiate(prefabData.prefab, worldPos, Quaternion.identity, this.transform);

                placedCount++;
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

    private bool IsAreaOccupiedWithBuffer(Vector2Int startPos, Vector2Int size, int buffer, HashSet<Vector2Int> occupied)
    {
        Vector2Int start = new Vector2Int(startPos.x - buffer, startPos.y - buffer);
        Vector2Int totalSize = new Vector2Int(size.x + buffer * 2, size.y + buffer * 2);
        return IsAreaOccupied(start, totalSize, occupied);
    }

    private void MarkAreaOccupiedWithBuffer(Vector2Int startPos, Vector2Int size, int buffer, HashSet<Vector2Int> occupied)
    {
        Vector2Int start = new Vector2Int(startPos.x - buffer, startPos.y - buffer);
        Vector2Int totalSize = new Vector2Int(size.x + buffer * 2, size.y + buffer * 2);
        MarkAreaOccupied(start, totalSize, occupied);
    }
    
    private List<RoomData> SplitOversizedRooms(List<RoomData> rooms)
    {
        List<RoomData> result = new List<RoomData>();

        foreach (var room in rooms)
        {
            Queue<RoomData> queue = new Queue<RoomData>();
            queue.Enqueue(room);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                int width = current.Bounds.size.x;
                int height = current.Bounds.size.y;

                if (width > maxRoomWidthGeneral || height > maxRoomHeightGeneral)
                {
                    // Split along longer axis
                    bool splitHorizontally = width >= height;

                    if (splitHorizontally)
                    {
                        int splitX = current.Bounds.xMin + width / 2;
                        BoundsInt left = new BoundsInt(current.Bounds.xMin, current.Bounds.yMin, 0, splitX - current.Bounds.xMin, height, 1);
                        BoundsInt right = new BoundsInt(splitX, current.Bounds.yMin, 0, current.Bounds.xMax - splitX, height, 1);
                        queue.Enqueue(new RoomData(left));
                        queue.Enqueue(new RoomData(right));
                    }
                    else
                    {
                        int splitY = current.Bounds.yMin + height / 2;
                        BoundsInt bottom = new BoundsInt(current.Bounds.xMin, current.Bounds.yMin, 0, width, splitY - current.Bounds.yMin, 1);
                        BoundsInt top = new BoundsInt(current.Bounds.xMin, splitY, 0, width, current.Bounds.yMax - splitY, 1);
                        queue.Enqueue(new RoomData(bottom));
                        queue.Enqueue(new RoomData(top));
                    }
                }
                else
                {
                    result.Add(current);
                }
            }
        }

        return result;
    }
}
