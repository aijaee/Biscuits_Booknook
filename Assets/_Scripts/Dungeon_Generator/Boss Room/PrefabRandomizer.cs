using UnityEngine;
using System.Collections.Generic;

public class PrefabRandomizer : MonoBehaviour
{
    [Header("Blob Objects")]
    public Transform blobContainer;
    public List<GameObject> blobPrefabs;

    [Header("Water Objects")]
    public Transform waterContainer;
    public List<GameObject> waterPrefabs;

    [Header("Barrel / Boxes")]
    public Transform barrelContainer;
    public List<GameObject> barrelPrefabs;

    [Header("Seaweed Objects")]
    public Transform seaweedContainer;
    public List<GameObject> seaweedPrefabs;

    [Header("Bubbles")]
    public Transform bubblesContainer;
    public List<GameObject> bubblesPrefabs;

    void Start()
    {
        ReplaceObjects(blobContainer, blobPrefabs);
        ReplaceObjects(waterContainer, waterPrefabs);
        ReplaceObjects(barrelContainer, barrelPrefabs);
        ReplaceObjects(seaweedContainer, seaweedPrefabs);
        ReplaceObjects(bubblesContainer, bubblesPrefabs);
    }

    void ReplaceObjects(Transform container, List<GameObject> prefabs)
    {
        if (container == null || prefabs == null || prefabs.Count == 0)
            return;

        foreach (Transform child in container)
        {
            Vector3 pos = child.position;
            Quaternion rot = child.rotation;
            DestroyImmediate(child.gameObject);
            GameObject prefab = prefabs[Random.Range(0, prefabs.Count)];
            Instantiate(prefab, pos, rot, container);
        }
    }
}