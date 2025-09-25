using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDamageEffects : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer spriteRenderer;
    public RectTransform hpBarRect;
    [SerializeField] private Image hpBarFiller;
    [SerializeField] private Image vignetteImage;

    private Color originalColor;
    private Color hpBarOriginalColor;

    private void Awake()
    {
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        if (hpBarFiller == null)
        {
            hpBarFiller = GameObject.Find("Filler")?.GetComponent<Image>();
            if (hpBarFiller != null)
                hpBarOriginalColor = hpBarFiller.color;
            else
                Debug.LogWarning("PlayerDamageEffects: HP bar filler not found!");
        }
        else
        {
            hpBarOriginalColor = hpBarFiller.color;
        }

        if (vignetteImage == null)
        {
            vignetteImage = GameObject.Find("DmgVignette")?.GetComponent<Image>();
            if (vignetteImage != null)
            {
                Color c = vignetteImage.color;
                c.a = 0f;
                vignetteImage.color = c;
            }
        }
    }

    public void PlayDamageEffects()
    {
        Debug.Log("PlayerDamageEffects: Playing damage feedback!");

        if (spriteRenderer != null)
            StartCoroutine(FlashRoutine());

        if (hpBarRect != null)
            StartCoroutine(ShakeHPBar());

        if (hpBarFiller != null)
            StartCoroutine(PulseHPBarColor());

        if (vignetteImage != null)
            StartCoroutine(FlashVignette());
    }

    private IEnumerator FlashRoutine()
    {
        Color c = spriteRenderer.color;

        for (int i = 0; i < 5; i++)
        {
            c.a = 0.3f;
            spriteRenderer.color = c;
            yield return new WaitForSeconds(0.1f);

            c.a = 1f;
            spriteRenderer.color = c;
            yield return new WaitForSeconds(0.1f);
        }

        spriteRenderer.color = originalColor;
    }

    private IEnumerator ShakeHPBar()
    {
        Vector3 originalPos = hpBarRect.localPosition;
        float duration = 0.3f;
        float magnitude = 40f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            Vector2 offset = Random.insideUnitCircle * magnitude;
            hpBarRect.localPosition = Vector3.Lerp(
                hpBarRect.localPosition,
                originalPos + (Vector3)offset,
                Time.deltaTime * 20f
            );

            elapsed += Time.deltaTime;
            yield return null;
        }
        hpBarRect.localPosition = originalPos;
    }

    private IEnumerator PulseHPBarColor()
    {
        if (hpBarFiller == null) yield break;

        Color flashColor;
        if (ColorUtility.TryParseHtmlString("#FFC7C7", out flashColor))
        {
            flashColor.a = hpBarOriginalColor.a;
            hpBarFiller.color = flashColor;
        }
        else
        {
            Debug.LogWarning("Invalid hex color for HP bar flash.");
            yield break;
        }

        yield return new WaitForSeconds(0.15f);
        hpBarFiller.color = hpBarOriginalColor;
    }


    private IEnumerator FlashVignette()
    {
        float maxAlpha = 0.4f;
        float fadeInDuration = 0.1f;
        float fadeOutDuration = 0.4f;

        Color c = vignetteImage.color;

        // Fade in
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(0f, maxAlpha, elapsed / fadeInDuration);
            vignetteImage.color = c;
            yield return null;
        }

        c.a = maxAlpha;
        vignetteImage.color = c;

        // Fade out
        elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(maxAlpha, 0f, elapsed / fadeOutDuration);
            vignetteImage.color = c;
            yield return null;
        }

        c.a = 0f;
        vignetteImage.color = c;
    }
}
