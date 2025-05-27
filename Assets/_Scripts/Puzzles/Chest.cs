using UnityEngine;

public class PuzzleChest : MonoBehaviour, IInteractable
{
    public bool isCorrectAnswer = false;
    public int rewardAmount = 10;
    public GameObject explosionEffect;
    public Sprite openedCorrectChestSprite;

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

        if (isCorrectAnswer)
        {
            if (spriteRenderer && openedCorrectChestSprite)
                spriteRenderer.sprite = openedCorrectChestSprite;

            Debug.Log($"Correct! You gained {rewardAmount} paperclips.");
        }
        else
        {
            Debug.Log("Wrong! Chest exploded.");
            if (explosionEffect)
                Instantiate(explosionEffect, transform.position, Quaternion.identity);

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
