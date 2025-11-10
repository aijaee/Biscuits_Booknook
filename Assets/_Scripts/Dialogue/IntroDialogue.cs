using System.Collections;
using UnityEngine;

public class IntroDialogue : MonoBehaviour
{
    [Header("Dialogue Settings")]
    public DialogueController dialogueController;
    public DialogueData dialogueData;

    [Header("Timing")]
    public float delayAfterTransition = 1f;

    [Header("Dialogue ID")]
    public string dialogueID;

    void Start()
    {
        if (string.IsNullOrEmpty(dialogueID))
            dialogueID = name;

        if (!PlayerPrefs.HasKey(dialogueID))
            StartCoroutine(StartIntro());
    }

    IEnumerator StartIntro()
    {
        yield return new WaitForSeconds(delayAfterTransition);

        var boss = FindObjectOfType<BossStatsMovement>();
        if (boss != null)
        {
            bool cutsceneDone = false;
            boss.OnCutsceneComplete += () => cutsceneDone = true;
            yield return new WaitUntil(() => cutsceneDone);
        }

        if (dialogueController == null || dialogueData == null)
            yield break;

        dialogueController.OnDialogueComplete += MarkDialogueComplete;
        dialogueController.StartDialogue(dialogueData);
    }

    void MarkDialogueComplete()
    {
        dialogueController.OnDialogueComplete -= MarkDialogueComplete;
        PlayerPrefs.SetInt(dialogueID, 1);
        PlayerPrefs.Save();
    }
}