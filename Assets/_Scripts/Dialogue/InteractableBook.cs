using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class InteractableBook : MonoBehaviour, IInteractable
{
    [Header("Dialogue Settings")]
    public DialogueController dialogueController;
    public DialogueData dialogueData;
    public BookshelfPortal targetBookshelf;

    [Header("Visuals")]
    public Color highlightColor = Color.white;

    [Header("Story Progress")]
    public int storyStage = 0;

    private SpriteRenderer sr;
    private Color originalColor;
    private bool playerInRange = false;
    private bool hasBeenRead = false;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        originalColor = sr.color;

        int progress = PlayerPrefs.GetInt("StoryProgress", 0);

        if (progress != storyStage)
        {
            gameObject.SetActive(false);
            return;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || hasBeenRead) return;

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
        if (!playerInRange || hasBeenRead) return;
        if (dialogueController == null || dialogueData == null) return;
        if (PlayerPrefs.GetInt("StoryProgress", 0) != storyStage) return;

        hasBeenRead = true;
        InteractUI.Instance.ShowButton(false);

        dialogueController.OnDialogueComplete += () =>
        {
            if (targetBookshelf != null)
                targetBookshelf.Unlock();

            int currentProgress = PlayerPrefs.GetInt("StoryProgress", 0);
            PlayerPrefs.SetInt("StoryProgress", currentProgress + 1);
            PlayerPrefs.Save();
        };

        dialogueController.StartDialogue(dialogueData);
    }
}
