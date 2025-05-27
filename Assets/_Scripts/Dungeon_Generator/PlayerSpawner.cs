using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    private GameObject playerInstance;

    public void SpawnPlayer(Vector2Int spawnTile)
    {
        Vector3 spawnPos = new Vector3(spawnTile.x + 0.5f, spawnTile.y + 0.5f, 0);

        if (playerInstance == null)
        {
            playerInstance = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        }
        else
        {
            playerInstance.transform.position = spawnPos;
        }
    }
}