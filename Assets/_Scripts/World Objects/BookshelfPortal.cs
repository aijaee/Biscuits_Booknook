using UnityEngine;
using System.Collections;

public class BookshelfPortal : MonoBehaviour
{
    [Header("Prefab to swap in when unlocked")]
    public GameObject unlockedPrefab;

    [Header("Fade settings")]
    public float fadeDuration = 0.5f;
    public SpriteRenderer sr;
    private bool isUnlocked = false;

    public void Unlock()
    {
        if (isUnlocked || unlockedPrefab == null) return;
        isUnlocked = true;
        StartCoroutine(FadeAndSwap());
    }

    private IEnumerator FadeAndSwap()
    {
        float t = 0f;
        Color originalColor = sr.color;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        GameObject newBookshelf = Instantiate(
            unlockedPrefab,
            transform.position,
            transform.rotation,
            transform.parent
        );

        Destroy(gameObject);
    }
}
