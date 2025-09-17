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

        if (hpBarFiller != null)
            hpBarOriginalColor = hpBarFiller.color;

        if (vignetteImage == null)
        {
            vignetteImage = GameObject.Find("DmgVignette").GetComponent<Image>();
            Color c = vignetteImage.color;
            c.a = 0f;
            vignetteImage.color = c;
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
        for (int i = 0; i < 6; i++)
        {
            hpBarRect.localPosition = originalPos + (Vector3)Random.insideUnitCircle * 20f;
            yield return new WaitForSeconds(0.05f);
        }
        hpBarRect.localPosition = originalPos;
    }

    private IEnumerator PulseHPBarColor()
    {
        hpBarFiller.color = Color.red;
        yield return new WaitForSeconds(0.15f);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 4f;
            hpBarFiller.color = Color.Lerp(Color.red, hpBarOriginalColor, t);
            yield return null;
        }
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

        // Reset to fully transparent
        c.a = 0f;
        vignetteImage.color = c;
    }
}
