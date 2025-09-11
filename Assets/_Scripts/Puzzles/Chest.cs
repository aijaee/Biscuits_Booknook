using UnityEngine;

public class Chest : MonoBehaviour, IInteractable
{
    public bool isCorrectAnswer = false;
    public int rewardAmount = 10;
    public GameObject explosionEffect;
    public Sprite openedCorrectChestSprite;
    public int explosionDamage = 20; 
    public ChestRewardDatabase rewardDatabase;

    private SpriteRenderer spriteRenderer;
    private bool hasBeenOpened = false;
    private bool playerInRange = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
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

                // apply reward effects
                switch (reward.rewardType)
                {
                    case ChestReward.RewardType.Heal:
                        if (playerController != null) playerController.Heal(reward.healAmount);
                        Debug.Log($"You healed for {reward.healAmount} health.");
                        break;
                    case ChestReward.RewardType.Speed:
                        if (playerController != null) playerController.ApplySpeedBoost(reward.speedMultiplier, reward.speedDuration);
                        Debug.Log($"Speed boosted x{reward.speedMultiplier} for {reward.speedDuration} seconds.");
                        break;
                    case ChestReward.RewardType.AdditionalDamage:
                        var meleeCtrl = player != null ? player.GetComponent<MeleeAttackController>() : null;
                        if (meleeCtrl != null) meleeCtrl.AddDamageBuff(reward.additionalDamageAmount);
                        Debug.Log($"Damage increased by {reward.additionalDamageAmount}.");
                        break;
                }
            }
            else
            {
                // set opened chest sprite on paperclip reward
                if (spriteRenderer)
                    spriteRenderer.sprite = openedCorrectChestSprite;

                Debug.Log($"Correct! You gained {rewardAmount} paperclips.");
            }
        }
        else
        {
            // wrong answer explosion
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
