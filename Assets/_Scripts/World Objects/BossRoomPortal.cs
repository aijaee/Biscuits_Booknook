using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class BossRoomPortal : MonoBehaviour, IInteractable
{
    [Header("Boss Portal Settings")]
    public int bossSceneIndex;

    bool playerInRange;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;
        InteractUI.Instance.SetCurrentInteractable(this);
        InteractUI.Instance.ShowButton(true);
    }

    void OnTriggerExit2D(Collider2D other)
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
            LevelTransition.Instance.TransitionToScene(bossSceneIndex);
    }
}
