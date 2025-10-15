using System.Collections.Generic;
using UnityEngine;

public class BossGridManager : MonoBehaviour
{
    [Header("Mirror Settings")]
    [SerializeField] private bool mirrorHorizontally;  // new: mirror the floor map on the X axis

    public BoundsInt Bounds { get; private set; }
    public HashSet<Vector2Int> FloorPositions { get; private set; }
    public HashSet<Vector2Int> WallPositions { get; private set; }

    public void Initialize(HashSet<Vector2Int> floor, Vector2Int bottomLeft, Vector2Int roomSize)
    {
        // start with original floor
        FloorPositions = new HashSet<Vector2Int>(floor);
        if (mirrorHorizontally)
        {
            var orig = new List<Vector2Int>(FloorPositions);
            foreach (var pos in orig)
            {
                int mirroredX = bottomLeft.x + roomSize.x - 1 - (pos.x - bottomLeft.x);
                FloorPositions.Add(new Vector2Int(mirroredX, pos.y));
            }
        }

        // ...existing bounds setup...
        Bounds = new BoundsInt(bottomLeft.x, bottomLeft.y, 0, roomSize.x, roomSize.y, 1);

        // recompute walls from (possibly mirrored) floor
        WallPositions = new HashSet<Vector2Int>();
        foreach (var pos in FloorPositions)
        {
            var dirs = new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
            foreach (var dir in dirs)
            {
                var neighbor = pos + dir;
                if (!FloorPositions.Contains(neighbor) && Bounds.Contains((Vector3Int)neighbor))
                    WallPositions.Add(neighbor);
            }
        }
    }

    public bool IsWalkable(Vector2Int pos)
    {
        return FloorPositions.Contains(pos);
    }

    public List<Vector2Int> GetNeighbors(Vector2Int pos)
    {
        var result = new List<Vector2Int>();
        var dirs = new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (var dir in dirs)
        {
            var n = pos + dir;
            if (IsWalkable(n))
                result.Add(n);
        }
        return result;
    }
}
