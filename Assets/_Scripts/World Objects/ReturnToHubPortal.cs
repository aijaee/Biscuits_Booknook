using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ReturnToHubPortal : MonoBehaviour, IInteractable
{
    [Header("Hub Settings")]
    [Tooltip("Scene index of your Hub World")]
    public int hubSceneIndex = 0;

    private bool playerInRange = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;
        InteractUI.Instance.SetCurrentInteractable(this);
        InteractUI.Instance.ShowButton(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;
        InteractUI.Instance.SetCurrentInteractable(null);
        InteractUI.Instance.ShowButton(false);
    }

    public void TryInteract()
    {
        if (!playerInRange) return;

        if (LevelTransition.Instance != null)
            LevelTransition.Instance.TransitionToScene(hubSceneIndex);
        else
            Debug.LogWarning("No LevelTransition instance found in the scene!");
    }
}
