using UnityEngine;

public class BossSpawner : MonoBehaviour
{
    [Header("Manual Boss Placement")]
    [SerializeField] private bool useManualBossPosition = false;
    [SerializeField] private Vector2 manualBossPosition = Vector2.zero;

    [Header("Boss Prefab")]
    [SerializeField] private GameObject bossPrefab;

    public void SpawnBoss(Vector2Int startPosition, Vector2Int bottomLeft, Vector2Int roomSize)
    {
        Vector3 bossPos;
        if (useManualBossPosition)
            // manualBossPosition interpreted as world‐space tile coords
            bossPos = new Vector3(manualBossPosition.x + 0.5f,
                                  manualBossPosition.y + 0.5f,
                                  -1f);
        else
            // default to dungeon start position (center of room)
            bossPos = new Vector3(startPosition.x + 0.5f,
                                  startPosition.y + 0.5f,
                                  -1f);

        // clamp inside the rectangular room
        float minX = bottomLeft.x + 0.5f;
        float maxX = bottomLeft.x + roomSize.x - 0.5f;
        float minY = bottomLeft.y + 0.5f;
        float maxY = bottomLeft.y + roomSize.y - 0.5f;
        bossPos.x = Mathf.Clamp(bossPos.x, minX, maxX);
        bossPos.y = Mathf.Clamp(bossPos.y, minY, maxY);

        var bossInstance = Instantiate(bossPrefab, bossPos, Quaternion.identity, transform);
        var sr = bossInstance.GetComponent<SpriteRenderer>();
        if (sr != null) sr.sortingOrder = 1;
    }
}
