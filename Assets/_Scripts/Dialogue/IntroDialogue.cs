using System.Collections;
using UnityEngine;

public class IntroDialogue : MonoBehaviour
{
    [Header("Dialogue Settings")]
    public DialogueController dialogueController;
    public DialogueData dialogueData;

    [Header("Timing")]
    public float delayAfterTransition = 1f;

    bool hasPlayed;

    void Start()
    {
        if (!hasPlayed)
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

        hasPlayed = true;
        dialogueController.StartDialogue(dialogueData);
    }
}