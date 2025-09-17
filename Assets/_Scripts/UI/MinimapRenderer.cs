using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class MinimapRenderer : MonoBehaviour
{
    public RawImage minimapImage;
    public Color floorColor = Color.white;
    public Color wallColor = Color.black;
    public Color playerColor = Color.red;
    public bool showPlayerDot = true;

    // new: only highlight current room
    public bool showOnlyCurrentRoom = false;
    public Color outsideRoomColor = Color.grey;
    private HashSet<Vector2Int> currentRoomTiles;
    private List<HashSet<Vector2Int>> allRoomTiles;

    // assign your player Transform here (world coords == tile coords)
    public Transform player;

    // new: puzzle room icon
    public bool showPuzzleRoomIcons = false;
    public Sprite puzzleRoomIconSprite;
    private List<GameObject> puzzleRoomIcons = new List<GameObject>();
    private List<Vector2Int> puzzleRoomCenters;

    private Texture2D minimapTexture;
    private int width, height;
    private Vector2Int playerPosition;

    // cache for runtime updates
    private HashSet<Vector2Int> cachedFloor;
    private HashSet<Vector2Int> cachedWall;
    private Vector2Int cachedMapSize;

    // 1) Compute wall tiles from floor set
    public HashSet<Vector2Int> CalculateWallPositions(HashSet<Vector2Int> floorTiles)
    {
        var wallTiles = new HashSet<Vector2Int>();
        foreach (var pos in floorTiles)
            foreach (var dir in Direction2D.cardinalDirectionsList)
                if (!floorTiles.Contains(pos + dir))
                    wallTiles.Add(pos + dir);
        return wallTiles;
    }

    // 2) Simplified draw call: floor + auto walls
    public void DrawMinimap(HashSet<Vector2Int> floorTiles, Vector2Int mapSize)
    {
        DrawMinimap(floorTiles, CalculateWallPositions(floorTiles), mapSize);
    }

    // 3) Remember player tile for marker
    public void SetPlayerPosition(Vector2Int pos)
    {
        playerPosition = pos;
    }

    public void SetCurrentRoomTiles(HashSet<Vector2Int> tiles)
    {
        currentRoomTiles = tiles;
    }

    public void SetAllRoomTiles(List<HashSet<Vector2Int>> roomTiles)
    {
        allRoomTiles = roomTiles;
    }

    public void SetPuzzleRoomCenters(List<Vector2Int> centers)
    {
        puzzleRoomCenters = centers;
    }

    // 4) Original draw, now also caches for Update()
    public void DrawMinimap(HashSet<Vector2Int> floorTiles, HashSet<Vector2Int> wallTiles, Vector2Int mapSize)
    {
        width = mapSize.x;
        height = mapSize.y;
        cachedFloor = floorTiles;
        cachedWall = wallTiles;
        cachedMapSize = mapSize;

        minimapTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        minimapTexture.filterMode = FilterMode.Point;

        // Clear
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                minimapTexture.SetPixel(x, y, Color.clear);

        // Draw floor, grey out if not in current room
        foreach (var pos in floorTiles)
        {
            var c = floorColor;
            if (showOnlyCurrentRoom && (currentRoomTiles == null || !currentRoomTiles.Contains(pos)))
                c = outsideRoomColor;
            minimapTexture.SetPixel(pos.x, pos.y, c);
        }

        // Draw walls
        foreach (var pos in wallTiles)
            minimapTexture.SetPixel(pos.x, pos.y, wallColor);

        // Draw player as a red dot if enabled
        if (showPlayerDot &&
            playerPosition.x >= 0 && playerPosition.x < width &&
            playerPosition.y >= 0 && playerPosition.y < height)
        {
            minimapTexture.SetPixel(playerPosition.x, playerPosition.y, playerColor);
        }

        minimapTexture.Apply();
        minimapImage.texture = minimapTexture;

        foreach (var go in puzzleRoomIcons)
            Destroy(go);
        puzzleRoomIcons.Clear();

        if (showPuzzleRoomIcons && puzzleRoomCenters != null && puzzleRoomIconSprite != null)
        {
            var rt = minimapImage.rectTransform;
            foreach (var center in puzzleRoomCenters)
            {
                float xRatio = center.x / (float)width;
                float yRatio = center.y / (float)height;
                float ax = xRatio * rt.rect.width - rt.rect.width * 0.5f;
                float ay = yRatio * rt.rect.height - rt.rect.height * 0.5f;

                var iconGO = new GameObject("PuzzleIcon", typeof(RectTransform), typeof(UnityEngine.UI.Image));
                iconGO.transform.SetParent(minimapImage.transform, false);
                var img = iconGO.GetComponent<UnityEngine.UI.Image>();
                img.sprite = puzzleRoomIconSprite;

                var iconRt = iconGO.GetComponent<RectTransform>();
                iconRt.pivot = new Vector2(0.5f, 0.5f);
                iconRt.anchoredPosition = new Vector2(ax, ay);


                if (allRoomTiles != null)
                {
                    var roomTiles = allRoomTiles.FirstOrDefault(set => set.Contains(center));
                    if (roomTiles != null)
                    {
                        int minX = roomTiles.Min(p => p.x);
                        int maxX = roomTiles.Max(p => p.x);
                        int minY = roomTiles.Min(p => p.y);
                        int maxY = roomTiles.Max(p => p.y);

                        float tileW = (maxX - minX + 1) / (float)width * rt.rect.width;
                        float tileH = (maxY - minY + 1) / (float)height * rt.rect.height;

                        Vector2 spritePx = new Vector2(
                            puzzleRoomIconSprite.rect.width,
                            puzzleRoomIconSprite.rect.height
                        );
                        float scale = Mathf.Min(tileW / spritePx.x, tileH / spritePx.y);
                        iconRt.sizeDelta = spritePx * scale;
                    }
                    else
                    {
                        iconRt.sizeDelta = new Vector2(
                            puzzleRoomIconSprite.rect.width,
                            puzzleRoomIconSprite.rect.height
                        );
                    }
                }

                puzzleRoomIcons.Add(iconGO);
            }
        }
    }

    private void Update()
    {
        if (showPlayerDot && player == null)
        {
            var obj = GameObject.FindGameObjectWithTag("Player");
            if (obj != null) player = obj.transform;
        }

        if (!showPlayerDot || cachedFloor == null || player == null) return;

        Vector2Int worldTile = Vector2Int.RoundToInt(player.position);
        if (worldTile != playerPosition)
        {
            SetPlayerPosition(worldTile);

            // determine which room the player is in
            if (showOnlyCurrentRoom && allRoomTiles != null)
            {
                foreach (var room in allRoomTiles)
                {
                    if (room.Contains(worldTile))
                    {
                        SetCurrentRoomTiles(room);
                        break;
                    }
                }
            }

            DrawMinimap(cachedFloor, cachedWall, cachedMapSize);
        }
    }
}
