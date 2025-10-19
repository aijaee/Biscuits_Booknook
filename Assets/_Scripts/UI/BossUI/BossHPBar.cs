using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BossHPBar : MonoBehaviour
{
    [SerializeField] private Image frameImage;
    [SerializeField] private Image innerImage;
    [SerializeField] private Color invulnerableTint = Color.white;
    [SerializeField] private Color stunnedTint = Color.red;
    [SerializeField] private float slideDistance = 200f;
    [SerializeField] private float slideDuration = 0.5f;

    private RectTransform innerRect;
    private float originalWidth;
    private BossStatsMovement boss;
    private float maxHealth;
    private RectTransform hpBarRect;
    private float prevHealth;
    private Vector3 hiddenPos;
    private Vector3 originalPos;

    void Awake()
    {
        if (innerImage != null)
        {
            innerRect = innerImage.rectTransform;
            originalWidth = innerRect.sizeDelta.x;
            innerImage.color = invulnerableTint;
        }
        hpBarRect = GetComponent<RectTransform>();
        
        originalPos = hpBarRect.localPosition;
        hiddenPos = originalPos - Vector3.up * slideDistance;
        hpBarRect.localPosition = hiddenPos;
    }

    void Update()
    {
        if (innerRect == null) return;

        if (boss == null)
        {
            boss = FindObjectOfType<BossStatsMovement>();
            if (boss != null)
            {
                maxHealth = boss.maxHealth;
                prevHealth = boss.currentHealth;
            }
            else
                return;           
        }

        if (boss.currentHealth < prevHealth)
            StartCoroutine(ShakeBar());
        prevHealth = boss.currentHealth;

        UpdateHPBar();
    }

    private void UpdateHPBar()
    {
        float pct = Mathf.Clamp01(boss.currentHealth / maxHealth);
        innerRect.sizeDelta = new Vector2(originalWidth * pct, innerRect.sizeDelta.y);

        if (boss.CurrentPhase == BossStatsMovement.BossPhase.Phase2
            && boss.CurrentState == BossStatsMovement.BossState.Stunned)
            innerImage.color = stunnedTint;
        else
            innerImage.color = invulnerableTint;
    }

    private IEnumerator ShakeBar()
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

    public IEnumerator ShowBar()
    {
        float elapsed = 0f;
        while (elapsed < slideDuration)
        {
            hpBarRect.localPosition = Vector3.Lerp(hiddenPos, originalPos, elapsed / slideDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        hpBarRect.localPosition = originalPos;
    }
}
