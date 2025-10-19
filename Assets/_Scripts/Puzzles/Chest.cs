using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using UI;

public class Chest : MonoBehaviour, IInteractable
{
    [TextArea(2, 5)]
    public string chestText;  

    public bool isCorrectAnswer = false;
    public int rewardAmount = 10;
    public GameObject explosionEffect;
    public Sprite openedCorrectChestSprite;
    public int explosionDamage = 20; 
    public ChestRewardDatabase rewardDatabase;

    private SpriteRenderer spriteRenderer;
    private bool hasBeenOpened = false;
    private bool playerInRange = false;
    private TMP_Text chestTMP;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        chestTMP = GetComponentInChildren<TMP_Text>();

        if (chestTMP != null)
        {
            chestTMP.text = chestText.Replace("\\n", "\n");
        }
    }

    public void TryInteract()
    {
        if (!playerInRange || hasBeenOpened) return;
        hasBeenOpened = true;

        var player = GameObject.FindGameObjectWithTag("Player");
        var playerController = player != null ? player.GetComponent<PlayerController>() : null;

        if (isCorrectAnswer)
        {
            hasBeenOpened = true;

            if (spriteRenderer != null)
                spriteRenderer.sprite = openedCorrectChestSprite;
            var entries = rewardDatabase != null ? rewardDatabase.rewardEntries : null;
            ChestReward reward = null;
            if (entries != null && entries.Count > 0)
            {
                int totalRate = 0;
                foreach (var e in entries) totalRate += e.dropRatePercentage;
                int roll = Random.Range(0, totalRate);
                foreach (var e in entries)
                {
                    if (roll <= e.dropRatePercentage)
                    {
                        reward = e.reward;
                        break;
                    }
                    roll -= e.dropRatePercentage;
                }
            }

            if (reward != null)
            {
                switch (reward.rewardType)
                {
                    case ChestReward.RewardType.Heal:
                        if (playerController != null) playerController.Heal(reward.healAmount);
                        BuffHotbar.Instance.AddBuff(BuffType.Heal, reward.healBuffIcon, reward.healBuffDuration);
                        break;

                    case ChestReward.RewardType.Speed:
                        if (playerController != null)
                            playerController.ApplySpeedBoost(reward.speedMultiplier, reward.speedDuration);
                        BuffHotbar.Instance.AddBuff(
                            BuffType.Speed,
                            reward.speedBuffIcon,
                            reward.speedDuration
                        );
                        Debug.Log($"Speed boosted x{reward.speedMultiplier} for {reward.speedDuration} seconds.");
                        break;

                    case ChestReward.RewardType.AdditionalDamage:
                        var meleeCtrl = player != null ? player.GetComponent<MeleeAttackController>() : null;
                        if (meleeCtrl != null) meleeCtrl.AddDamageBuff(reward.additionalDamageAmount);

                        BuffHotbar.Instance.AddBuff(
                            BuffType.AdditionalDamage,
                            reward.additionalDamageBuffIcon,
                            0f
                        );
                        RuntimeData.Instance.AddAdditionalDamageBuff(reward.additionalDamageBuffIcon);
                        break;
                }
            }

            if (chestTMP != null)
            {
                Color c;
                if (ColorUtility.TryParseHtmlString("#50E970", out c))
                chestTMP.color = c;
            }
            else
            {
                if (spriteRenderer)
                    spriteRenderer.sprite = openedCorrectChestSprite;

                Debug.Log($"Correct! You gained {rewardAmount} paperclips.");
            }
        }
        else
        {
            if (explosionEffect)
            {
                GameObject explosion = Instantiate(explosionEffect, transform.position + new Vector3(0, 0.25f, 0), Quaternion.identity);
                var sr = explosion.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sortingLayerName = "Foreground";
                    sr.sortingOrder = 100;
                }
            }

            if (playerController != null)
                playerController.TakeChestDamage(explosionDamage);

            if (spriteRenderer != null)
                spriteRenderer.enabled = false;
            
            Collider2D[] colliders = GetComponents<Collider2D>();
            foreach (Collider2D col in colliders)
            {
                col.enabled = false;
            }

            if (chestTMP != null)
            {
                chestTMP.color = Color.red;
                StartCoroutine(FadeTMP(chestTMP, 3f));
            }

            Destroy(gameObject, 3f);
        }

        InteractUI.Instance.ShowButton(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasBeenOpened)
        {
            playerInRange = true;
            InteractUI.Instance.SetCurrentInteractable(this);
            InteractUI.Instance.ShowButton(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            InteractUI.Instance.SetCurrentInteractable(null);
            InteractUI.Instance.ShowButton(false);
        }
    }

    private IEnumerator FadeTMP(TMP_Text tmp, float duration)
    {
        Color original = tmp.color;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(original.a, 0, elapsed / duration);
            tmp.color = new Color(original.r, original.g, original.b, alpha);
            yield return null;
        }

        tmp.color = new Color(original.r, original.g, original.b, 0);
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}