using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapVisualizer : MonoBehaviour
{
    [SerializeField]
    private Tilemap floorTilemap, wallTilemap;

    [SerializeField]
    private TileBase floorTile, wallTop, wallSideRight, wallSiderLeft, wallBottom, wallFull,
    wallInnerCornerDownLeft, wallInnerCornerDownRight,
    wallDiagonalCornerDownRight, wallDiagonalCornerDownLeft, wallDiagonalCornerUpRight, wallDiagonalCornerUpLeft,
    wallInnerCornerUpLeft, wallInnerCornerUpRight;

    // 🟩 Tracks what wall tile currently exists at a position
    private Dictionary<Vector2Int, TileBase> paintedWalls = new Dictionary<Vector2Int, TileBase>();

    public void PaintFloorTiles(IEnumerable<Vector2Int> floorPositions)
    {
        PaintTiles(floorPositions, floorTilemap, floorTile);
    }

    private void PaintTiles(IEnumerable<Vector2Int> positions, Tilemap tilemap, TileBase tile)
    {
        foreach (var position in positions)
        {
            PaintSingleTile(tilemap, tile, position);
        }
    }

    // 🟩 PASS 1: Paint basic walls first
    internal void PaintSingleBasicWall(Vector2Int position, string binaryType)
    {
        int typeAsInt = Convert.ToInt32(binaryType, 2);
        TileBase tile = null;

        if (WallTypesHelper.wallTop.Contains(typeAsInt))
            tile = wallTop;
        else if (WallTypesHelper.wallSideRight.Contains(typeAsInt))
            tile = wallSideRight;
        else if (WallTypesHelper.wallSideLeft.Contains(typeAsInt))
            tile = wallSiderLeft;
        else if (WallTypesHelper.wallBottom.Contains(typeAsInt))
            tile = wallBottom;
        else if (WallTypesHelper.wallFull.Contains(typeAsInt))
            tile = wallFull;

        if (tile == null && HasDiagonalFloor(binaryType))
            tile = wallFull;

        // if still null, fallback and warn
        if (tile == null)
        {
            Debug.LogWarning($"Unrecognized basic wall pattern '{binaryType}' at {position}, defaulting to wallFull.");
            tile = wallFull;
        }

        PaintSingleTileWithLog(wallTilemap, tile, position);
        paintedWalls[position] = tile;
    }

    private bool HasDiagonalFloor(string binaryType)
    {
        return binaryType[0] == '1' || binaryType[2] == '1' ||
               binaryType[5] == '1' || binaryType[7] == '1';
    }

    // 🟩 PASS 2: Corners — overwrite basic walls if necessary
    internal void PaintSingleCornerWall(Vector2Int position, string binaryType)
    {
        int typeAsInt = Convert.ToInt32(binaryType, 2);
        TileBase tile = null;

        if (WallTypesHelper.wallInnerCornerUpLeft.Contains(typeAsInt))
            tile = wallInnerCornerUpLeft;
        else if (WallTypesHelper.wallInnerCornerUpRight.Contains(typeAsInt))
            tile = wallInnerCornerUpRight;
        else if (WallTypesHelper.wallInnerCornerDownLeft.Contains(typeAsInt))
            tile = wallInnerCornerDownLeft;
        else if (WallTypesHelper.wallInnerCornerDownRight.Contains(typeAsInt))
            tile = wallInnerCornerDownRight;
        else if (WallTypesHelper.wallDiagonalCornerUpLeft.Contains(typeAsInt))
            tile = wallDiagonalCornerUpLeft;
        else if (WallTypesHelper.wallDiagonalCornerUpRight.Contains(typeAsInt))
            tile = wallDiagonalCornerUpRight;
        else if (WallTypesHelper.wallDiagonalCornerDownLeft.Contains(typeAsInt))
            tile = wallDiagonalCornerDownLeft;
        else if (WallTypesHelper.wallDiagonalCornerDownRight.Contains(typeAsInt))
            tile = wallDiagonalCornerDownRight;
        else if (WallTypesHelper.wallBottomEightDirections.Contains(typeAsInt))
            tile = wallBottom;
        else if (WallTypesHelper.wallFullEightDirections.Contains(typeAsInt))
            tile = wallFull;

        if (tile == null)
            return;

        // Check if there’s already a basic wall here
        if (paintedWalls.TryGetValue(position, out var existing))
        {
            // Allow overwrite only if the existing tile is a basic wall
            if (IsBasicWall(existing))
            {
                Debug.Log($"Overwriting {existing.name} with {tile.name} at {position}");
                PaintSingleTileWithLog(wallTilemap, tile, position);
                paintedWalls[position] = tile;
            }
            else
            {
                // Skip overwriting if the existing wall is also special
                return;
            }
        }
        else
        {
            // No wall here yet — paint freely
            PaintSingleTileWithLog(wallTilemap, tile, position);
            paintedWalls[position] = tile;
        }
    }

    private bool IsBasicWall(TileBase tile)
    {
        return tile == wallTop || tile == wallBottom ||
               tile == wallSideRight || tile == wallSiderLeft ||
               tile == wallFull;
    }

    private void PaintSingleTile(Tilemap tilemap, TileBase tile, Vector2Int position)
    {
        var tilePos = tilemap.WorldToCell((Vector3Int)position);
        tilemap.SetTile(tilePos, tile);
    }

    private void PaintSingleTileWithLog(Tilemap tilemap, TileBase tile, Vector2Int position)
    {
        var tilePos = tilemap.WorldToCell((Vector3Int)position);

        if (tilemap.HasTile(tilePos))
        {
            var existing = tilemap.GetTile(tilePos);
            if (existing != null && existing != tile)
                Debug.LogWarning($"Overlap detected at {position}: placing {tile.name} over {existing.name}");
        }

        tilemap.SetTile(tilePos, tile);
    }

    public void Clear()
    {
        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();
        paintedWalls.Clear();
    }

    public bool HasTileAt(Vector2Int position)
    {
        Vector3Int tilePos = wallTilemap.WorldToCell((Vector3Int)position);
        return wallTilemap.HasTile(tilePos);
    }
}
