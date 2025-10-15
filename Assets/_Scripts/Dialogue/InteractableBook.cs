using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class InteractableBook : MonoBehaviour, IInteractable
{
    public DialogueSequenceController dialogueController;
    public BookshelfPortal targetBookshelf;
    public Color highlightColor = Color.white;

    private bool playerInRange = false;
    private SpriteRenderer sr;
    private Color originalColor;
    private bool hasBeenRead = false;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        originalColor = sr.color;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasBeenRead || !other.CompareTag("Player")) return;

        playerInRange = true;
        sr.color = highlightColor;
        InteractUI.Instance.SetCurrentInteractable(this);
        InteractUI.Instance.ShowButton(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;
        sr.color = originalColor;
        InteractUI.Instance.SetCurrentInteractable(null);
        InteractUI.Instance.ShowButton(false);
    }

    public void TryInteract()
    {
        if (hasBeenRead || !playerInRange || dialogueController == null) return;
        if (dialogueController.IsRunning) return;

        hasBeenRead = true;
        InteractUI.Instance.ShowButton(false);

        // Subscribe to unlock bookshelf after dialogue completes
        if (targetBookshelf != null)
        {
            System.Action unlockAction = null;
            unlockAction = () =>
            {
                targetBookshelf.Unlock();
                dialogueController.OnDialogueComplete -= unlockAction; // unsubscribe
            };
            dialogueController.OnDialogueComplete += unlockAction;
        }

        dialogueController.StartSequence();
    }
}
