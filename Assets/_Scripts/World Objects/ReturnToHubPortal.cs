using UnityEngine;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(Collider2D))]
public class HubPortal : MonoBehaviour, IInteractable
{
    public CanvasGroup portalCanvasGroup;
    public TMP_Text portalText;
    public string defaultText = "You need to solve all puzzles first!";
    public string readyText = "Enter the Hub?";

    private void Start()
    {
        UpdatePortalState();
    }

    private void UpdatePortalState()
    {
        if (PuzzleManager.Instance.AllPuzzlesCompleted())
        {
            portalCanvasGroup.alpha = 1f;
            portalText.text = readyText;
            GetComponent<Collider2D>().enabled = true;
        }
        else
        {
            portalCanvasGroup.alpha = 0.75f;
            portalText.text = defaultText;
            GetComponent<Collider2D>().enabled = false;
        }
    }

    public void TryInteract()
    {
        if (!PuzzleManager.Instance.AllPuzzlesCompleted())
            return;

        if (LevelTransition.Instance != null)
            LevelTransition.Instance.TransitionToScene(0);
    }

    private void Update()
    {
        UpdatePortalState();
    }
}
