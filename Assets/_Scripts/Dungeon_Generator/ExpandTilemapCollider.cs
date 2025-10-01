using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap), typeof(TilemapCollider2D))]
public class ExpandTilemapCollider : MonoBehaviour
{
    [Tooltip("How many tiles to expand around each collider tile.")]
    public int expandBy = 1;

    private Tilemap tilemap;

    void Start()
    {
        tilemap = GetComponent<Tilemap>();
        ExpandColliderTiles();
    }

    private void ExpandColliderTiles()
    {
        BoundsInt bounds = tilemap.cellBounds;
        TileBase[] allTiles = tilemap.GetTilesBlock(bounds);

        // Create a copy of current tiles so we don’t overwrite during iteration
        TileBase[,] originalTiles = new TileBase[bounds.size.x, bounds.size.y];
        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                originalTiles[x, y] = allTiles[x + y * bounds.size.x];
            }
        }

        // Expand each non-null tile
        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                TileBase tile = originalTiles[x, y];
                if (tile != null)
                {
                    int worldX = bounds.xMin + x;
                    int worldY = bounds.yMin + y;

                    // Place tiles around in a square expansion
                    for (int dx = -expandBy; dx <= expandBy; dx++)
                    {
                        for (int dy = -expandBy; dy <= expandBy; dy++)
                        {
                            Vector3Int newPos = new Vector3Int(worldX + dx, worldY + dy, 0);
                            if (tilemap.GetTile(newPos) == null)
                                tilemap.SetTile(newPos, tile);
                        }
                    }
                }
            }
        }
    }
}
