using UnityEngine;
using TMPro;

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
            // Handles both literal line breaks (from typing Enter) and "\n" written in Inspector
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
            var list = rewardDatabase != null ? rewardDatabase.rewards : null;
            var reward = (list != null && list.Count > 0) ? list[Random.Range(0, list.Count)] : null;

            if (reward != null)
            {
                if (spriteRenderer)
                    spriteRenderer.sprite = reward.chestSprite != null ? reward.chestSprite : openedCorrectChestSprite;

                switch (reward.rewardType)
                {
                    case ChestReward.RewardType.Heal:
                        if (playerController != null) playerController.Heal(reward.healAmount);
                        break;
                    case ChestReward.RewardType.Speed:
                        if (playerController != null) playerController.ApplySpeedBoost(reward.speedMultiplier, reward.speedDuration);
                        break;
                    case ChestReward.RewardType.AdditionalDamage:
                        var meleeCtrl = player != null ? player.GetComponent<MeleeAttackController>() : null;
                        if (meleeCtrl != null) meleeCtrl.AddDamageBuff(reward.additionalDamageAmount);
                        break;
                }
            }
            else
            {
                if (spriteRenderer)
                    spriteRenderer.sprite = openedCorrectChestSprite;
            }
        }
        else
        {
            if (explosionEffect)
            {
                GameObject explosion = Instantiate(explosionEffect, transform.position, Quaternion.identity);
                var sr = explosion.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sortingLayerName = "Foreground";
                    sr.sortingOrder = 100;
                }
            }
            if (playerController != null) playerController.TakeDamage(explosionDamage);
            Destroy(gameObject);
        }

        InteractUI.Instance.ShowButton(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
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
}
