using UnityEngine;

public class PuzzleRoomTester : MonoBehaviour
{
    public GameObject[] puzzleRoomPrefabs; // assign all puzzle room prefabs here
    public float spacing = 25f; // adjust based on room size

    void Start()
    {
        SpawnAllPuzzleRooms();
    }

    void SpawnAllPuzzleRooms()
    {
        if (puzzleRoomPrefabs == null || puzzleRoomPrefabs.Length == 0) return;

        int cols = Mathf.CeilToInt(Mathf.Sqrt(puzzleRoomPrefabs.Length));
        int rows = Mathf.CeilToInt((float)puzzleRoomPrefabs.Length / cols);

        for (int i = 0; i < puzzleRoomPrefabs.Length; i++)
        {
            int x = i % cols;
            int y = i / cols;

            Vector3 pos = new Vector3(x * spacing, -y * spacing, 0);
            Instantiate(puzzleRoomPrefabs[i], pos, Quaternion.identity, transform);
        }
    }
}
