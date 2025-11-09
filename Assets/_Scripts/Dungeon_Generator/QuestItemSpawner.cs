using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class QuestItemSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject[] questItemPrefabs;
    [SerializeField] private RoomFirstDungeonGenerator dungeonGenerator;
    [SerializeField] private QuestTracker questTracker;

    [Header("Spawn Settings")]
    [SerializeField] private int minItems = 4;
    [SerializeField] private int maxItems = 4;
    [SerializeField] private bool spawnOnlyOnce = true;
    [SerializeField] private bool autoSpawnOnGeneration = true;
    [Tooltip("Parent for spawned items. If null, items are placed at scene root.")]
    [SerializeField] private Transform spawnParent = null;

    [Header("Collision Check")]
    [SerializeField] private float overlapRadius = 0.5f;
    [SerializeField] private LayerMask obstacleMask;

    private bool hasSpawned = false;

    private void OnEnable()
    {
        if (dungeonGenerator == null)
            dungeonGenerator = FindObjectOfType<RoomFirstDungeonGenerator>();

        if (questTracker == null)
            questTracker = FindObjectOfType<QuestTracker>();

        if (dungeonGenerator != null && autoSpawnOnGeneration)
            dungeonGenerator.OnGenerationComplete += HandleGenerationComplete;
    }

    private void OnDisable()
    {
        if (dungeonGenerator != null && autoSpawnOnGeneration)
            dungeonGenerator.OnGenerationComplete -= HandleGenerationComplete;
    }

    private void HandleGenerationComplete()
    {
        TrySpawnQuestItems();
    }

    public void TrySpawnQuestItems()
    {
        if (spawnOnlyOnce && hasSpawned)
            return;

        if (questItemPrefabs == null || questItemPrefabs.Length == 0)
            return;

        if (dungeonGenerator == null)
            return;

        var rooms = dungeonGenerator.GetRooms();
        if (rooms == null || rooms.Count == 0)
            return;

        var eligibleRooms = rooms.Where(r => r.Type == RoomType.Combat).ToList();
        if (eligibleRooms.Count == 0)
            return;

        int desiredCount = Mathf.Clamp(Random.Range(minItems, maxItems + 1), 0, Mathf.Min(eligibleRooms.Count, questItemPrefabs.Length));
        if (desiredCount == 0)
        {
            hasSpawned = true;
            return;
        }

        Shuffle(eligibleRooms);
        List<GameObject> availablePrefabs = new List<GameObject>(questItemPrefabs);
        Shuffle(availablePrefabs);

        for (int i = 0; i < desiredCount; i++)
        {
            var room = eligibleRooms[i];
            Vector2Int cell = room.Center;
            Vector3 spawnPos = FindFreePositionNear(cell);

            if (spawnPos == Vector3.positiveInfinity)
                continue;

            GameObject prefab = availablePrefabs[i];
            GameObject spawned = Instantiate(prefab, spawnPos, Quaternion.identity, spawnParent);

            QuestCollectible collectible = spawned.GetComponent<QuestCollectible>();
            if (collectible != null && questTracker != null)
                collectible.SetTracker(questTracker);

            spawned.name = $"QuestItem_{prefab.name}_({cell.x},{cell.y})";
        }

        hasSpawned = true;
    }

    private Vector3 FindFreePositionNear(Vector2Int cell)
    {
        for (int attempt = 0; attempt < 10; attempt++)
        {
            Vector3 pos = new Vector3(
                cell.x + Random.Range(-1.5f, 1.5f),
                cell.y + Random.Range(-1.5f, 1.5f),
                0f
            );

            Collider2D hit = Physics2D.OverlapCircle(pos, overlapRadius, obstacleMask);
            if (hit == null)
                return pos;
        }

        return Vector3.positiveInfinity;
    }

    private void Shuffle<T>(IList<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = Random.Range(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}