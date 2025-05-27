using UnityEngine;

public class InteractableFloorItem : MonoBehaviour, IInteractable
{
    [TextArea] public string passageText;

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

        PassageMenuUI.Instance.OpenMenu(passageText);
        InteractUI.Instance.ShowButton(false);
    }
}
