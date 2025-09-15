using UnityEngine;

public class InteractableFloorItem : MonoBehaviour, IInteractable
{
    [Header("Passage Data")]
    public string passageTitle;
    [TextArea(3, 10)] public string passageBody;
    public bool isStartingNote;

    private bool playerInRange = false;

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

    public void TryInteract()
    {
        if (!playerInRange) return;

        PassageMenuUI.Instance.OpenMenu(passageTitle, passageBody, isStartingNote);
        InteractUI.Instance.ShowButton(false);
    }
}
