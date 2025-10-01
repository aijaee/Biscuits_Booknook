using System.Collections;
using UnityEngine;

public class PlayerDamageEffects : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public RectTransform hpBarRect;

    private Color originalColor;

    private void Awake()
    {
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    public void PlayDamageEffects()
    {
        Debug.Log("PlayerDamageEffects: Playing damage feedback!");

        if (spriteRenderer != null)
            StartCoroutine(FlashRoutine());

        if (PlayerUIEffects.Instance != null && hpBarRect != null)
            PlayerUIEffects.Instance.PlayDamageUIEffects();
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
}
