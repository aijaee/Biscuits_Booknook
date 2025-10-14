using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class DialogueEntry
{
    public Sprite characterNameImage;
    [TextArea(2, 6)] public string text;
    public DialogueNPC speaker;
    public bool speakerVisible = true;
}

[System.Serializable]
public class DialogueNPC
{
    public string name;
    public Image image;
    public Animator animator;
}

public class DialogueSequenceController : MonoBehaviour
{
    [Header("UI & Settings")]
    public GameObject storySceneRoot;
    public CanvasGroup storyCanvasGroup;
    public Image fadeImage;
    public Image nameImage;
    public TMP_Text dialogueText;
    public Image nextIndicator;
    public List<DialogueNPC> npcs;

    [Header("Typing")]
    public string talkingParam = "isTalking";
    public float fadeDuration = 1f;
    public float typeSpeed = 0.03f;

    [Header("Dialogue")]
    public List<DialogueEntry> dialogueEntries;

    [Header("Optional disable during dialogue")]
    public Behaviour[] disableBehaviours;
    public GameObject[] disableGameObjects;

    [Header("Bookshelf to unlock after dialogue")]
    public BookshelfPortal targetBookshelf;

    private bool isRunning = false;
    public bool IsRunning => isRunning;

    private bool[] behavioursOriginalState;
    private bool[] gameObjectsOriginalState;

    private bool clickPressed = false;

    private Coroutine typingCoroutine = null;
    private bool isTyping = false;
    public System.Action OnDialogueComplete;

    void Update()
    {
        clickPressed = false;

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            clickPressed = true;
        }
    }

    public void StartSequence()
    {
        if (!isRunning)
        {
            storySceneRoot.SetActive(true);
            StartCoroutine(RunSequence());
        }
    }

    private IEnumerator RunSequence()
    {
        isRunning = true;

        // Disable behaviors
        if (disableBehaviours != null)
        {
            behavioursOriginalState = new bool[disableBehaviours.Length];
            for (int i = 0; i < disableBehaviours.Length; i++)
            {
                behavioursOriginalState[i] = disableBehaviours[i] != null && disableBehaviours[i].enabled;
                if (disableBehaviours[i] != null) disableBehaviours[i].enabled = false;
            }
        }

        // Disable GameObjects
        if (disableGameObjects != null)
        {
            gameObjectsOriginalState = new bool[disableGameObjects.Length];
            for (int i = 0; i < disableGameObjects.Length; i++)
            {
                gameObjectsOriginalState[i] = disableGameObjects[i] != null && disableGameObjects[i].activeSelf;
                if (disableGameObjects[i] != null) disableGameObjects[i].SetActive(false);
            }
        }

        if (storyCanvasGroup != null)
        {
            storyCanvasGroup.alpha = 0f;
            storyCanvasGroup.blocksRaycasts = true;
            storyCanvasGroup.interactable = true;
        }

        if (nextIndicator != null)
            nextIndicator.gameObject.SetActive(false);

        foreach (var npc in npcs)
        {
            if (npc.image != null) npc.image.gameObject.SetActive(false);
            if (npc.animator != null) npc.animator.SetBool(talkingParam, false);
        }

        yield return StartCoroutine(FadeCanvas(0f, 1f, fadeDuration));
        yield return StartCoroutine(RunDialogue());
        yield return StartCoroutine(FadeCanvas(1f, 0f, fadeDuration));

        // Unlock bookshelf AFTER dialogue
        if (targetBookshelf != null)
        {
            targetBookshelf.Unlock();
        }

        if (storyCanvasGroup != null)
        {
            storyCanvasGroup.alpha = 0f;
            storyCanvasGroup.blocksRaycasts = false;
            storyCanvasGroup.interactable = false;
        }

        storySceneRoot.SetActive(false);

        // Restore NPCs
        foreach (var npc in npcs)
        {
            if (npc.image != null) npc.image.gameObject.SetActive(false);
            if (npc.animator != null) npc.animator.SetBool(talkingParam, false);
        }

        // Restore behaviors
        if (disableBehaviours != null)
        {
            for (int i = 0; i < disableBehaviours.Length; i++)
                if (disableBehaviours[i] != null) disableBehaviours[i].enabled = behavioursOriginalState[i];
        }

        // Restore GameObjects
        if (disableGameObjects != null)
        {
            for (int i = 0; i < disableGameObjects.Length; i++)
                if (disableGameObjects[i] != null) disableGameObjects[i].SetActive(gameObjectsOriginalState[i]);
        }

        OnDialogueComplete?.Invoke();
        isRunning = false;
    }

    private IEnumerator RunDialogue()
    {
        for (int entryIndex = 0; entryIndex < dialogueEntries.Count; entryIndex++)
        {
            DialogueEntry entry = dialogueEntries[entryIndex];

            dialogueText.text = "";
            if (nextIndicator != null) nextIndicator.gameObject.SetActive(false);
            if (nameImage != null)
            {
                nameImage.sprite = entry.characterNameImage;
                nameImage.enabled = entry.characterNameImage != null;
            }

            foreach (var npc in npcs)
            {
                if (npc.image != null) npc.image.gameObject.SetActive(false);
                if (npc.animator != null) npc.animator.SetBool(talkingParam, false);
            }

            if (entry.speakerVisible && entry.speaker != null)
            {
                if (entry.speaker.image != null) entry.speaker.image.gameObject.SetActive(true);
                if (entry.speaker.animator != null) entry.speaker.animator.SetBool(talkingParam, true);
            }

            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
                isTyping = false;
            }

            isTyping = true;
            typingCoroutine = StartCoroutine(TypeText(entry.text));

            while (isTyping)
            {
                yield return null;
            }

            clickPressed = false;
            yield return null;

            if (entry.speaker != null && entry.speaker.animator != null)
            {
                entry.speaker.animator.SetBool(talkingParam, false);
            }

            if (nextIndicator != null)
                nextIndicator.gameObject.SetActive(true);

            yield return new WaitUntil(() => clickPressed);
            yield return null;
        }
    }

    private IEnumerator TypeText(string line)
    {
        StringBuilder display = new StringBuilder();
        int i = 0;
        int len = line.Length;

        while (i < len)
        {
            if (clickPressed)
            {
                dialogueText.text = line;
                break;
            }

            char c = line[i];

            if (c == '<')
            {
                int tagEnd = line.IndexOf('>', i);
                if (tagEnd == -1)
                {
                    display.Append(c);
                    i++;
                }
                else
                {
                    int count = tagEnd - i + 1;
                    display.Append(line.Substring(i, count));
                    i += count;
                }

                dialogueText.text = display.ToString();
                yield return null;
                continue;
            }
            else
            {
                display.Append(c);
                dialogueText.text = display.ToString();
                i++;

                float elapsed = 0f;
                while (elapsed < typeSpeed)
                {
                    if (clickPressed)
                    {
                        dialogueText.text = line;
                        i = len;
                        break;
                    }
                    elapsed += Time.deltaTime;
                    yield return null;
                }
            }
        }

        isTyping = false;
        typingCoroutine = null;
    }

    private IEnumerator FadeCanvas(float from, float to, float duration)
    {
        float t = 0f;
        Color fadeColor = fadeImage != null ? fadeImage.color : new Color(0, 0, 0, 0);

        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(from, to, Mathf.Clamp01(t / duration));
            if (storyCanvasGroup != null) storyCanvasGroup.alpha = alpha;
            if (fadeImage != null)
            {
                fadeColor.a = alpha;
                fadeImage.color = fadeColor;
            }
            yield return null;
        }

        if (storyCanvasGroup != null) storyCanvasGroup.alpha = to;
        if (fadeImage != null)
        {
            fadeColor.a = to;
            fadeImage.color = fadeColor;
        }
    }
}
