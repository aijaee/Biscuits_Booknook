using UnityEngine;
using System.Collections;

public class QuestCollectible : MonoBehaviour, IInteractable
{
    [Header("Quest Data")]
    public QuestTracker questTracker;
    public int objectiveIndex; 
    public int amountToAdd = 1;

    [Header("Dialogue")]
    public DialogueController dialogueController;
    public DialogueData dialogueData;

    [Header("Fade Settings")]
    public float fadeDuration = 1f;

    private bool playerInRange = false;

    public void SetTracker(QuestTracker tracker)
    {
        questTracker = tracker;
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

    public void TryInteract()
    {
        if (!playerInRange) return;

        if (questTracker != null && objectiveIndex >= 0 && objectiveIndex < questTracker.objectives.Length)
        {
            var obj = questTracker.objectives[objectiveIndex];
            int newAmount = obj.currentAmount + amountToAdd;
            questTracker.UpdateObjective(objectiveIndex, newAmount);
        }

        if (dialogueController == null)
            dialogueController = FindObjectOfType<DialogueController>();

        if (dialogueController != null && dialogueData != null)
        {
            dialogueController.OnDialogueComplete += OnDialogueFinished;
            dialogueController.StartDialogue(dialogueData);
            InteractUI.Instance.ShowButton(false);
            return;
        }

        InteractUI.Instance.ShowButton(false);
        StartCoroutine(FadeOutAndDestroy());
    }

    private void OnDialogueFinished()
    {
        if (dialogueController != null)
            dialogueController.OnDialogueComplete -= OnDialogueFinished;

        StartCoroutine(FadeOutAndDestroy());
    }

    private IEnumerator FadeOutAndDestroy()
    {
        foreach (var col in GetComponentsInChildren<Collider2D>())
            col.enabled = false;

        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        float t = 0f;
        Color[] originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            originalColors[i] = renderers[i].color;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            for (int i = 0; i < renderers.Length; i++)
            {
                Color c = originalColors[i];
                c.a = alpha;
                renderers[i].color = c;
            }
            yield return null;
        }

        Destroy(gameObject);
    }
}