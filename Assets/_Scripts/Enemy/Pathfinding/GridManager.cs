using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
    public Vector2Int gridSize; // Set by dungeon generator (width, height in cells)
    public float cellSize = 1f; // Set by dungeon generator
    public LayerMask unwalkableMask;
    [SerializeField] private Grid unityGrid; // Optional Unity Grid component

    private Node[,] grid;

    // Call this from your dungeon generator after dungeon is built
    public void Initialize(Vector2Int size, float cellSize, LayerMask unwalkableMask)
    {
        this.gridSize = size;
        this.cellSize = cellSize;
        this.unwalkableMask = unwalkableMask;

        // Try to mirror the dungeon's floor tilemap
        if (BuildGridFromTilemap())
        {
            Debug.Log($"GridManager: Built grid from floor Tilemap ({gridSize.x}×{gridSize.y}).");
            return;
        }

        if (unityGrid != null)
        {
            this.cellSize = unityGrid.cellSize.x;
            Debug.Log($"GridManager: Using Unity Grid component for cell size ({this.cellSize}) and origin ({unityGrid.transform.position})");
        }
        else
        {
            Debug.Log($"GridManager: Using manual cell size ({this.cellSize}) and origin ({transform.position})");
        }
        CreateGrid();
    }

    public void Initialize(Vector2Int size, float cellSize, LayerMask unwalkableMask, Vector3 origin)
    {
        transform.position = origin;
        Initialize(size, cellSize, unwalkableMask);
    }

    // Attempt to build grid based on the floor tiles painted by the dungeon generator
    private bool BuildGridFromTilemap()
    {
        Tilemap floor = GameObject.FindObjectOfType<Tilemap>();
        if (floor == null) return false;

        Vector3 origin = unityGrid != null
            ? unityGrid.transform.position
            : transform.position;
        grid = new Node[gridSize.x, gridSize.y];
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                var tilePos = new Vector3Int(x, y, 0);
                bool walkable = floor.HasTile(tilePos);
                Vector3 worldPoint = origin + new Vector3(
                    x * cellSize + cellSize * 0.5f,
                    y * cellSize + cellSize * 0.5f,
                    0f);
                grid[x, y] = new Node(walkable, worldPoint, x, y);
            }
        }
        return true;
    }

    public void CreateGrid()
    {
        if (gridSize.x <= 0 || gridSize.y <= 0)
        {
            Debug.LogWarning("GridManager: gridSize is not set or is zero. Skipping grid creation.");
            grid = null;
            return;
        }

        grid = new Node[gridSize.x, gridSize.y];
        Vector3 worldBottomLeft = unityGrid != null ? unityGrid.transform.position : transform.position;

        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector3 worldPoint = worldBottomLeft + new Vector3(x * cellSize + cellSize / 2f, y * cellSize + cellSize / 2f, 0);
                bool walkable = Physics2D.OverlapBox(worldPoint, new Vector2(cellSize * 0.9f, cellSize * 0.9f), 0f, unwalkableMask) == null;
                grid[x, y] = new Node(walkable, worldPoint, x, y);
            }
        }
    }

    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        if (grid == null || gridSize.x <= 0 || gridSize.y <= 0)
        {
            Debug.LogError("GridManager: Grid is not initialized or gridSize is invalid.");
            return null;
        }

        Vector3 origin = unityGrid != null ? unityGrid.transform.position : transform.position;
        float usedCellSize = unityGrid != null ? unityGrid.cellSize.x : cellSize;

        int x = Mathf.FloorToInt((worldPosition.x - origin.x) / usedCellSize);
        int y = Mathf.FloorToInt((worldPosition.y - origin.y) / usedCellSize);

        Debug.Log($"NodeFromWorldPoint: Calculated indices x={x}, y={y} for worldPosition={worldPosition}");
        Debug.Log($"NodeFromWorldPoint: Grid size is ({gridSize.x}, {gridSize.y})");

        // Bounds check before accessing grid
        if (x < 0 || x >= gridSize.x || y < 0 || y >= gridSize.y)
        {
            Debug.LogError($"NodeFromWorldPoint: Indices out of bounds! x={x}, y={y}, gridSize=({gridSize.x},{gridSize.y})");
            return null;
        }

        x = Mathf.Clamp(x, 0, gridSize.x - 1);
        y = Mathf.Clamp(y, 0, gridSize.y - 1);

        Debug.Log($"NodeFromWorldPoint: Clamped indices x={x}, y={y}, gridSize=({gridSize.x},{gridSize.y})");

        if (grid == null)
        {
            Debug.LogError("NodeFromWorldPoint: grid is null after checks!");
            return null;
        }

        return grid[x, y];
    }

    public Vector3 WorldPointFromNode(Node node)
    {
        return node.worldPosition;
    }

    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();

        if (grid == null || gridSize.x <= 0 || gridSize.y <= 0)
            return neighbours;

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0)
                    continue;

                int checkX = node.gridX + dx;
                int checkY = node.gridY + dy;

                if (checkX >= 0 && checkX < gridSize.x && checkY >= 0 && checkY < gridSize.y)
                {
                    neighbours.Add(grid[checkX, checkY]);
                }
            }
        }
        return neighbours;
    }

    public Node[,] GetGrid() => grid;

    void OnDrawGizmos()
    {
        if (grid == null) return;
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Gizmos.color = grid[x, y].walkable ? Color.white : Color.red;
                Gizmos.DrawCube(grid[x, y].worldPosition, new Vector3(cellSize * 0.9f, 0.1f, cellSize * 0.9f));
            }
        }
    }
    
    public void MakeAllNodesWalkable()
    {
        if (grid == null) return;
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                grid[x, y].walkable = true;
            }
        }
    }

    // Example: Make a specific node unwalkable
    public void SetNodeUnwalkable(int x, int y)
    {
        if (grid == null) return;
        if (x >= 0 && x < gridSize.x && y >= 0 && y < gridSize.y)
            grid[x, y].walkable = false;
    }
}