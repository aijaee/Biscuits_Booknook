using System.Collections.Generic;
using UnityEngine;

public static class WallGenerator
{
    public static void CreateWalls(HashSet<Vector2Int> floorPositions, TilemapVisualizer tilemapVisualizer, Dictionary<Vector2Int, RoomData> floorToRoom)
    {
        var basicWallPositions = FindWallsInDirections(floorPositions, Direction2D.cardinalDirectionsList);
        var cornerWallPositions = FindWallsInDirections(floorPositions, Direction2D.diagonalDirectionsList);

        CreateBasicWall(tilemapVisualizer, basicWallPositions, floorPositions, floorToRoom);
        CreateCornerWalls(tilemapVisualizer, cornerWallPositions, floorPositions, floorToRoom);
    }

    private static void CreateCornerWalls(
    TilemapVisualizer tilemapVisualizer,
    HashSet<Vector2Int> cornerWallPositions,
    HashSet<Vector2Int> floorPositions,
    Dictionary<Vector2Int, RoomData> floorToRoom)
    {
        foreach (var position in cornerWallPositions)
        {
            string neighboursBinaryType = "";
            foreach (var direction in Direction2D.eightDirectionsList)
            {
                var neighbourPosition = position + direction;
                neighboursBinaryType += floorPositions.Contains(neighbourPosition) ? "1" : "0";
            }

            // Try painting the corner
            tilemapVisualizer.PaintSingleCornerWall(position, neighboursBinaryType);

            // 🟡 Debug: if nothing was placed, log what pattern we had
        if (!tilemapVisualizer.HasTileAt(position))
        {
            tilemapVisualizer.PaintSingleCornerWall(position, neighboursBinaryType);
        }
        }
    }

    private static void CreateBasicWall(TilemapVisualizer tilemapVisualizer, HashSet<Vector2Int> basicWallPositions, HashSet<Vector2Int> floorPositions, Dictionary<Vector2Int, RoomData> floorToRoom)
    {
        foreach (var position in basicWallPositions)
        {
            if (IsSharedWall(position, floorPositions, floorToRoom))
                continue; // Skip overlapping or shared walls

            string neighboursBinaryType = "";
            foreach (var direction in Direction2D.cardinalDirectionsList)
            {
                var neighbourPosition = position + direction;
                neighboursBinaryType += floorPositions.Contains(neighbourPosition) ? "1" : "0";
            }

            tilemapVisualizer.PaintSingleBasicWall(position, neighboursBinaryType);
        }
    }

    private static bool IsSharedWall(Vector2Int position, HashSet<Vector2Int> floorPositions, Dictionary<Vector2Int, RoomData> floorToRoom)
    {
        // Check all adjacent floor tiles: if they belong to different rooms, skip wall
        RoomData firstRoom = null;
        bool hasMultipleRooms = false;

        foreach (var dir in Direction2D.cardinalDirectionsList)
        {
            var neighbour = position + dir;
            if (floorToRoom.TryGetValue(neighbour, out var room))
            {
                if (firstRoom == null)
                    firstRoom = room;
                else if (room != firstRoom)
                {
                    hasMultipleRooms = true;
                    break;
                }
            }
        }

        return hasMultipleRooms;
    }

    private static HashSet<Vector2Int> FindWallsInDirections(HashSet<Vector2Int> floorPositions, List<Vector2Int> directionList)
    {
        HashSet<Vector2Int> wallPositions = new HashSet<Vector2Int>();
        foreach (var position in floorPositions)
        {
            foreach (var direction in directionList)
            {
                var neighbourPosition = position + direction;
                if (!floorPositions.Contains(neighbourPosition))
                    wallPositions.Add(neighbourPosition);
            }
        }
        return wallPositions;
    }
}
