using UnityEngine;
using System.Collections.Generic;

public class AStarPathfinder : MonoBehaviour
{
    public GridManager gridManager;

    void Awake()
    {
        // Auto-assign gridManager if not set and present on the same GameObject
        if (gridManager == null)
        {

            gridManager = GetComponent<GridManager>();
            if (gridManager == null)
            {
                // fallback: find any in the scene
                gridManager = FindObjectOfType<GridManager>();
                if (gridManager == null)
                    Debug.LogWarning("AStarPathfinder: No GridManager found in scene.");
            }
        }
    }

    public List<Vector3> FindPath(Vector3 startWorldPos, Vector3 targetWorldPos)
    {
        Debug.Log($"AStarPathfinder: FindPath called from {startWorldPos} to {targetWorldPos}");

        if (gridManager == null)
        {
            Debug.LogError("AStarPathfinder: GridManager not assigned.");
            return null;
        }

        Node startNode = gridManager.NodeFromWorldPoint(startWorldPos);
        Node targetNode = gridManager.NodeFromWorldPoint(targetWorldPos);

        Debug.Log($"AStarPathfinder: StartNode {(startNode != null ? startNode.worldPosition.ToString() : "null")}, walkable: {startNode?.walkable}, TargetNode {(targetNode != null ? targetNode.worldPosition.ToString() : "null")}, walkable: {targetNode?.walkable}");

        if (startNode == null || targetNode == null)
        {
            Debug.LogWarning("AStarPathfinder: Start or Target node is null.");
            return null;
        }

        if (!targetNode.walkable)
        {
            Debug.LogWarning("AStarPathfinder: Target node is not walkable.");
            return null;
        }
        if (!startNode.walkable)
        {
            Debug.LogWarning("AStarPathfinder: Start node is not walkable, overriding as walkable for pathfinding.");
            startNode.walkable = true;
        }

        Debug.Log("AStarPathfinder: Passed walkable checks, starting pathfinding loop.");

        // Reset node costs for all nodes before pathfinding
        Node[,] grid = gridManager.GetGrid();
        if (grid != null)
        {
            int width = grid.GetLength(0);
            int height = grid.GetLength(1);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    grid[x, y].gCost = int.MaxValue;
                    grid[x, y].hCost = 0;
                    grid[x, y].parent = null;
                }
            }
        }

        startNode.gCost = 0;

        var openSet = new List<Node> { startNode };
        var closedSet = new HashSet<Node>();

        while (openSet.Count > 0)
        {
            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost ||
                    (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
            {
                var path = RetracePath(startNode, targetNode);
                if (path == null || path.Count == 0)
                {
                    Debug.LogWarning("AStarPathfinder: Path found but empty after retrace.");
                }
                else
                {
                    Debug.Log($"AStarPathfinder: Path found with {path.Count} waypoints. First: {path[0]}, Last: {path[path.Count-1]}");
                }
                return path;
            }

            foreach (Node neighbour in gridManager.GetNeighbours(currentNode))
            {
                if (!neighbour.walkable || closedSet.Contains(neighbour))
                    continue;

                int newCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                if (newCostToNeighbour < neighbour.gCost)
                {
                    neighbour.gCost = newCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = currentNode;

                    if (!openSet.Contains(neighbour))
                        openSet.Add(neighbour);
                }
            }
        }
        Debug.LogWarning("AStarPathfinder: No path found.");
        return null;
    }

    List<Vector3> RetracePath(Node startNode, Node endNode)
    {
        if (endNode == null)
            return null;

        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
            if (currentNode == null)
                return null; // No valid path
        }
        path.Add(startNode); // Optionally include start node
        path.Reverse();

        List<Vector3> waypoints = new List<Vector3>();
        foreach (var node in path)
        {
            Vector3 pos = node.worldPosition;
            pos.z = 0; // Ensure Z=0 for 2D
            waypoints.Add(pos);
        }

        return waypoints;
    }

    int GetDistance(Node a, Node b)
    {
        int dx = Mathf.Abs(a.gridX - b.gridX);
        int dy = Mathf.Abs(a.gridY - b.gridY);
        if (dx > dy)
            return 14 * dy + 10 * (dx - dy);
        return 14 * dx + 10 * (dy - dx);
    }
}