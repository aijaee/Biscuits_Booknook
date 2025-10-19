using UnityEngine;

public class BossDefeatedPortalSpawner : MonoBehaviour
{
    [Header("Portal Settings")]
    public GameObject portalPrefab;

    public void SpawnPortal()
    {
        if (portalPrefab == null)
        {
            Debug.LogWarning("No portal prefab assigned.");
            return;
        }

        Instantiate(portalPrefab, new Vector2(15f, 10f), Quaternion.identity);
    }
}