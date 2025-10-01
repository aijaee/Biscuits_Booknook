using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIEffects : MonoBehaviour
{
    public static PlayerUIEffects Instance;

    private RectTransform hpBarRect;
    private Image hpBarFiller;
    private Image hpBarFrame;
    private Image vignetteImage;

    [SerializeField] private Sprite damageFrameSprite;

    private Color hpBarOriginalColor;
    private Sprite hpBarOriginalSprite;

    private void Awake()
    {
        Instance = this;

        // This script is on HPBar itself
        hpBarRect = GetComponent<RectTransform>();

        var fillerTransform = transform.Find("Filler");
        if (fillerTransform != null)
            hpBarFiller = fillerTransform.GetComponent<Image>();

        var frameTransform = transform.Find("Frame");
        if (frameTransform != null)
            hpBarFrame = frameTransform.GetComponent<Image>();

        // Auto-find vignette anywhere in the Canvas
        if (vignetteImage == null)
        {
            var vignetteObj = GameObject.Find("DmgVignette");
            if (vignetteObj != null)
                vignetteImage = vignetteObj.GetComponent<Image>();
        }

        if (hpBarFiller != null)
            hpBarOriginalColor = hpBarFiller.color;

        if (hpBarFrame != null)
            hpBarOriginalSprite = hpBarFrame.sprite;

        if (vignetteImage != null)
        {
            Color c = vignetteImage.color;
            c.a = 0f;
            vignetteImage.color = c;
        }
    }

    public void PlayDamageUIEffects()
    {
        if (hpBarRect != null)
            StartCoroutine(ShakeHPBar());

        if (hpBarFiller != null)
            StartCoroutine(PulseHPBarColor());

        if (vignetteImage != null)
            StartCoroutine(FlashVignette());

        if (hpBarFrame != null && damageFrameSprite != null)
            StartCoroutine(FlashHPBarFrame());
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
        Color flashColor;
        if (ColorUtility.TryParseHtmlString("#FFC7C7", out flashColor))
        {
            flashColor.a = hpBarOriginalColor.a;
            hpBarFiller.color = flashColor;
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

    private IEnumerator FlashHPBarFrame()
    {
        hpBarFrame.sprite = damageFrameSprite;
        yield return new WaitForSeconds(0.3f);
        hpBarFrame.sprite = hpBarOriginalSprite;
    }
}
