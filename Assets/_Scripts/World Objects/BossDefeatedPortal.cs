using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class BossDefeatedPortal : MonoBehaviour, IInteractable
{
    [Header("Hub Return Settings")]
    public int hubSceneIndex = 0;
    public int newStoryStage;

    bool playerInRange;
    bool isInteracting = false;

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
        RuntimeData.Instance.ResetRuntimeData();
    }

    public void TryInteract()
    {
        if (!playerInRange || isInteracting) return;

        isInteracting = true;

        int currentProgress = PlayerPrefs.GetInt("StoryProgress", 0);
        if (currentProgress < newStoryStage)
        {
            PlayerPrefs.SetInt("StoryProgress", newStoryStage);
            PlayerPrefs.Save();
        }

        if (LevelTransition.Instance != null)
        {
            LevelTransition.Instance.TransitionToScene(hubSceneIndex);
        }

        StartCoroutine(ResetInteractionFlag(2f));
    }

    private IEnumerator ResetInteractionFlag(float delay)
    {
        yield return new WaitForSeconds(delay);
        isInteracting = false;
    }
}
