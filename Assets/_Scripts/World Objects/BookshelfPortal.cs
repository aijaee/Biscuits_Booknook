using UnityEngine;
using System.Collections;

public class BookshelfPortal : MonoBehaviour
{
    [Header("Prefab to swap in when unlocked")]
    public GameObject unlockedPrefab;

    [Header("Fade settings")]
    public float fadeDuration = 0.5f;
    public SpriteRenderer sr;

    [Header("Story Progress Requirement")]
    public int requiredStoryStage = 0;

    private bool isUnlocked = false;

    private string PlayerPrefKey => $"BookshelfUnlocked_{gameObject.name}";

    private void Awake()
    {
        int progress = PlayerPrefs.GetInt("StoryProgress", 0);
        if (progress < requiredStoryStage)
        {
            PlayerPrefs.SetInt(PlayerPrefKey, 0);
            PlayerPrefs.Save();
        }

        if (progress >= requiredStoryStage || PlayerPrefs.GetInt(PlayerPrefKey, 0) == 1)
        {
            UnlockInstant();
        }
    }


    public void Unlock()
    {
        if (isUnlocked || unlockedPrefab == null) return;
        isUnlocked = true;

        PlayerPrefs.SetInt(PlayerPrefKey, 1);
        PlayerPrefs.Save();

        StartCoroutine(FadeAndSwap());
    }

    private void UnlockInstant()
    {
        if (isUnlocked) return;
        isUnlocked = true;

        if (unlockedPrefab != null)
        {
            Instantiate(
                unlockedPrefab,
                transform.position,
                transform.rotation,
                transform.parent
            );
        }

        Destroy(gameObject);
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

        if (unlockedPrefab != null)
        {
            Instantiate(
                unlockedPrefab,
                transform.position,
                transform.rotation,
                transform.parent
            );
        }

        Destroy(gameObject);
    }
}
