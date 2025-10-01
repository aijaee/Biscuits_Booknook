using UnityEngine;
using System.Collections.Generic;

public class InfiniteParallax : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private GameObject backgroundPrefab;
    [SerializeField] private Vector2 parallaxMultiplier = new Vector2(0.5f, 0.5f);

    private Vector3 lastCameraPosition;
    private Vector2 tileSize;
    private List<Transform> tiles = new List<Transform>();

    // How many tiles around the camera to keep (1 = 3x3 grid, 2 = 5x5, etc.)
    [SerializeField] private int gridRadius = 1;

    private void Start()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        lastCameraPosition = cameraTransform.position;

        // Get tile size from prefab
        var sr = backgroundPrefab.GetComponent<SpriteRenderer>();
        tileSize = sr.bounds.size;

        // Spawn initial grid
        for (int x = -gridRadius; x <= gridRadius; x++)
        {
            for (int y = -gridRadius; y <= gridRadius; y++)
            {
                Vector3 spawnPos = new Vector3(
                    x * tileSize.x,
                    y * tileSize.y,
                    backgroundPrefab.transform.position.z
                );

                GameObject tile = Instantiate(backgroundPrefab, spawnPos, Quaternion.identity, transform);
                tiles.Add(tile.transform);
            }
        }
    }

    private void LateUpdate()
    {
        // Apply parallax (delta-based, like your working script)
        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;
        transform.position += new Vector3(
            deltaMovement.x * parallaxMultiplier.x,
            deltaMovement.y * parallaxMultiplier.y,
            0f
        );
        lastCameraPosition = cameraTransform.position;

        // Recycle tiles when camera moves
        foreach (Transform tile in tiles)
        {
            Vector3 diff = cameraTransform.position - tile.position;

            // If camera goes too far right
            if (diff.x > tileSize.x * (gridRadius + 0.5f))
                tile.position += new Vector3(tileSize.x * (gridRadius * 2 + 1), 0f, 0f);

            // If too far left
            if (diff.x < -tileSize.x * (gridRadius + 0.5f))
                tile.position -= new Vector3(tileSize.x * (gridRadius * 2 + 1), 0f, 0f);

            // If too far up
            if (diff.y > tileSize.y * (gridRadius + 0.5f))
                tile.position += new Vector3(0f, tileSize.y * (gridRadius * 2 + 1), 0f);

            // If too far down
            if (diff.y < -tileSize.y * (gridRadius + 0.5f))
                tile.position -= new Vector3(0f, tileSize.y * (gridRadius * 2 + 1), 0f);
        }
    }
}
