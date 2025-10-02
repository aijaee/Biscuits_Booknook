using System.Collections.Generic;
using UnityEngine;

public class InteractableFloorItem : MonoBehaviour, IInteractable
{
    [Header("Passage Data")]
    public string passageTitle;
    [TextArea(3, 10)] public string passageBody;
    [TextArea(1, 5)] public List<string> manualTitles;
    [TextArea(3, 10)] public List<string> manualPages;
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

        if (manualPages != null && manualPages.Count > 0)
        {
            if (manualTitles != null && manualTitles.Count == manualPages.Count)
            {
                PassageMenuUI.Instance.OpenMenu(manualTitles, manualPages, isStartingNote);
            }
            else
            {
                List<string> titlesFallback = new List<string>();
                for (int i = 0; i < manualPages.Count; i++)
                    titlesFallback.Add(passageTitle);

                PassageMenuUI.Instance.OpenMenu(titlesFallback, manualPages, isStartingNote);
            }
        }
        else
        {
            PassageMenuUI.Instance.OpenMenu(passageTitle, passageBody, isStartingNote);
        }

        InteractUI.Instance.ShowButton(false);
    }
}