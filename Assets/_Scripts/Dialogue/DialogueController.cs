using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueController : MonoBehaviour
{
    [Header("Portrait Settings")]
    public PortraitController portraitController;
    public Vector2 portraitFinalPosition = new Vector2(0f, 60f);
    private float previousTimeScale = 1f;

    public GameObject dialogueRoot;
    public CanvasGroup canvasGroup;
    public Image fadeImage;
    public Image speakerNameImage;
    private Sprite previousNameImage = null;
    private Sprite previousPortraitSprite = null;
    public TMP_Text dialogueText;
    public Image nextIndicator;
    public float fadeDuration = 0.5f;
    public float typeSpeed = 0.03f;
    public bool IsRunning { get; private set; }

    private JoystickPlayerExample playerMovement;
    private MeleeAttackController playerMeleeAttack;
    private AStarPathfinder[] pathfinders;
    private List<AStarPathfinder> disabledPathfinders = new List<AStarPathfinder>();
    private bool clickPressed = false;
    private bool skipRequested = false;
    private Coroutine typingCoroutine = null;
    private bool isTyping = false;
    private Animator dialoguePortraitAnimator;

    public System.Action OnDialogueComplete;

    private void Awake()
    {
        playerMeleeAttack = FindObjectOfType<MeleeAttackController>();
        playerMovement = FindObjectOfType<JoystickPlayerExample>();
        pathfinders = FindObjectsOfType<AStarPathfinder>();

        if (speakerNameImage != null && speakerNameImage.GetComponent<CanvasGroup>() == null)
            speakerNameImage.gameObject.AddComponent<CanvasGroup>();

        if (dialogueText != null && dialogueText.GetComponent<CanvasGroup>() == null)
            dialogueText.gameObject.AddComponent<CanvasGroup>();

        if (portraitController != null && portraitController.GetComponent<CanvasGroup>() == null)
            portraitController.gameObject.AddComponent<CanvasGroup>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
        {
            clickPressed = true;
            if (isTyping) skipRequested = true;
        }
    }

    public void StartDialogue(DialogueData data)
    {
        PauseGameForDialogue();

        if (IsRunning || data == null) return;

        if (playerMovement != null)
            playerMovement.enabled = false;

        if (playerMeleeAttack != null)
            playerMeleeAttack.enabled = false;

        disabledPathfinders.Clear();
        if (pathfinders != null)
        {
            foreach (var p in pathfinders)
                if (p.enabled)
                {
                    p.enabled = false;
                    disabledPathfinders.Add(p);
                }
        }

        dialogueRoot?.SetActive(true);
        StartCoroutine(RunDialogueSequence(data));
    }

    private IEnumerator RunDialogueSequence(DialogueData data)
    {
        IsRunning = true;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }

        dialogueText.text = "";
        if (speakerNameImage != null)
        {
            speakerNameImage.sprite = null;
            speakerNameImage.enabled = false;
        }
        nextIndicator?.gameObject.SetActive(false);
        portraitController?.SetCharacter(null, null);

        if (canvasGroup != null)
            yield return StartCoroutine(FadeCanvas(0f, 1f, fadeDuration));

        for (int i = 0; i < data.lines.Length; i++)
        {
            var line = data.lines[i];

            if (portraitController != null)
            {
                portraitController.SetCharacter(line.portraitSprite, line.animatorController);

                CanvasGroup portraitGroup = portraitController.GetComponent<CanvasGroup>();
                if (portraitGroup == null)
                    portraitGroup = portraitController.gameObject.AddComponent<CanvasGroup>();

                RectTransform portraitRect = portraitController.GetComponent<RectTransform>();
                bool spriteChanged = previousPortraitSprite != line.portraitSprite;

                if (line.walkIn)
                {
                    clickPressed = false;
                    skipRequested = false;
                    isTyping = false;
                    yield return StartCoroutine(WalkInCharacter());
                    previousPortraitSprite = line.portraitSprite;
                }
                else if (spriteChanged)
                {
                    portraitRect.anchoredPosition = portraitFinalPosition;
                    portraitGroup.alpha = 0f;

                    float t = 0f;
                    float fadeDurationLocal = 0.5f;
                    while (t < fadeDurationLocal)
                    {
                        t += Time.unscaledDeltaTime;
                        portraitGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeDurationLocal);
                        yield return null;
                    }
                    portraitGroup.alpha = 1f;

                    previousPortraitSprite = line.portraitSprite;
                }
            }

            if (speakerNameImage != null)
                yield return StartCoroutine(FadeNameImage(line.nameImage, 0.35f, true));

            // Start typing the current line
            dialogueText.text = "";
            isTyping = true;
            skipRequested = false;

            if (portraitController != null && portraitController.portraitAnimator != null)
                portraitController.portraitAnimator.SetBool("isTalking", true);

            typingCoroutine = StartCoroutine(TypeText(line.text));
            yield return new WaitUntil(() => !isTyping);

            if (portraitController != null && portraitController.portraitAnimator != null)
                portraitController.portraitAnimator.SetBool("isTalking", false);

            // Show next indicator and wait for click
            nextIndicator?.gameObject.SetActive(true);
            clickPressed = false;
            yield return new WaitUntil(() => clickPressed);
            nextIndicator?.gameObject.SetActive(false);

            // Play open box animation if flagged for the NEXT line
            if (line.playOpenBoxAnimation && portraitController != null)
            {
                Animator animator = portraitController.GetComponent<Animator>();
                if (animator != null)
                {
                    animator.SetTrigger("openBox");
                    // wait until animation finishes
                    AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
                    float animDuration = state.length;
                    yield return new WaitForSecondsRealtime(animDuration);
                }
            }

            // Walk-out if flagged
            if (line.walkOut && portraitController != null)
            {
                clickPressed = false;
                skipRequested = false;
                yield return StartCoroutine(WalkOutCharacter());
            }
        }

        if (canvasGroup != null)
            yield return StartCoroutine(FadeCanvas(1f, 0f, fadeDuration));

        dialogueRoot?.SetActive(false);

        foreach (var p in disabledPathfinders)
            if (p != null)
                p.enabled = true;

        disabledPathfinders.Clear();

        ResumeGameAfterDialogue();
        previousNameImage = null;
        previousPortraitSprite = null;
        IsRunning = false;
        OnDialogueComplete?.Invoke();
    }

    private IEnumerator TypeText(string line)
    {
        StringBuilder display = new StringBuilder();
        int i = 0;
        int len = line.Length;

        while (i < len)
        {
            if (skipRequested)
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

            display.Append(c);
            dialogueText.text = display.ToString();
            i++;

            float elapsed = 0f;
            while (elapsed < typeSpeed)
            {
                if (skipRequested)
                {
                    dialogueText.text = line;
                    i = len;
                    break;
                }
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            yield return null;
        }

        isTyping = false;
        typingCoroutine = null;
        skipRequested = false;
    }

    private IEnumerator FadeCanvas(float from, float to, float duration)
    {
        if (canvasGroup == null && fadeImage == null) yield break;

        float t = 0f;
        Color fadeColor = fadeImage != null ? fadeImage.color : new Color(0, 0, 0, 0);

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(from, to, Mathf.Clamp01(t / duration));
            if (canvasGroup != null) canvasGroup.alpha = alpha;
            if (fadeImage != null)
            {
                fadeColor.a = alpha;
                fadeImage.color = fadeColor;
            }
            yield return null;
        }

        if (canvasGroup != null) canvasGroup.alpha = to;
        if (fadeImage != null)
        {
            fadeColor.a = to;
            fadeImage.color = fadeColor;
        }
    }

    private IEnumerator WalkInCharacter()
    {
        RectTransform portraitRect = portraitController.GetComponent<RectTransform>();
        if (portraitRect == null) yield break;

        Canvas parentCanvas = portraitRect.GetComponentInParent<Canvas>();
        RectTransform canvasRect = parentCanvas.GetComponent<RectTransform>();

        Vector2 finalPos = portraitFinalPosition;
        Vector2 startPos = finalPos + new Vector2(-canvasRect.rect.width, 0f);
        portraitRect.anchoredPosition = startPos;

        CanvasGroup portraitGroup = portraitRect.GetComponent<CanvasGroup>();
        if (portraitGroup == null)
            portraitGroup = portraitRect.gameObject.AddComponent<CanvasGroup>();
        portraitGroup.alpha = 0f;

        float t = 0f;
        float duration = 2.5f;

        float bobAmplitude = 75f;
        float bobFrequency = 20f;
        float rotAmplitude = 4f;
        float baseY = finalPos.y;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float eased = Mathf.SmoothStep(0f, 1f, t / duration);
            float bobOffset = Mathf.Sin(t * bobFrequency) * bobAmplitude * (1 - eased);
            float rotOffset = Mathf.Sin(t * bobFrequency * 0.8f) * rotAmplitude * (1 - eased);

            Vector2 currentPos = Vector2.Lerp(startPos, finalPos, eased);
            portraitRect.anchoredPosition = new Vector2(currentPos.x, baseY + bobOffset);
            portraitRect.localRotation = Quaternion.Euler(0f, 0f, rotOffset);

            portraitGroup.alpha = eased;
            yield return null;
        }

        portraitRect.anchoredPosition = finalPos;
        portraitRect.localRotation = Quaternion.identity;
        portraitGroup.alpha = 1f;
    }

    private IEnumerator WalkOutCharacter()
    {
        RectTransform portraitRect = portraitController.GetComponent<RectTransform>();
        if (portraitRect == null) yield break;

        Canvas parentCanvas = portraitRect.GetComponentInParent<Canvas>();
        RectTransform canvasRect = parentCanvas.GetComponent<RectTransform>();

        Vector2 startPos = portraitFinalPosition;
        Vector2 endPos = startPos + new Vector2(canvasRect.rect.width, 0f);

        CanvasGroup portraitGroup = portraitRect.GetComponent<CanvasGroup>();
        if (portraitGroup == null)
            portraitGroup = portraitRect.gameObject.AddComponent<CanvasGroup>();

        float t = 0f;
        float duration = 2.5f;

        float bobAmplitude = 75f;
        float bobFrequency = 20f;
        float rotAmplitude = 4f;
        float baseY = startPos.y;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float eased = Mathf.SmoothStep(0f, 1f, t / duration);
            float intensity = eased;
            float bobOffset = Mathf.Sin(t * bobFrequency) * bobAmplitude * intensity;
            float rotOffset = Mathf.Sin(t * bobFrequency * 0.9f) * rotAmplitude * intensity;

            Vector2 currentPos = Vector2.Lerp(startPos, endPos, eased);
            portraitRect.anchoredPosition = new Vector2(currentPos.x, baseY + bobOffset);
            portraitRect.localRotation = Quaternion.Euler(0f, 0f, rotOffset);

            portraitGroup.alpha = 1f - eased;
            yield return null;
        }

        portraitRect.anchoredPosition = endPos;
        portraitRect.localRotation = Quaternion.identity;
        portraitGroup.alpha = 0f;
    }

    private IEnumerator FadeNameImage(Sprite newSprite, float duration, bool fadeText = true)
    {
        if (speakerNameImage == null) yield break;

        if (previousNameImage == newSprite) yield break;
        previousNameImage = newSprite;

        CanvasGroup nameGroup = speakerNameImage.GetComponent<CanvasGroup>();
        if (nameGroup == null)
            nameGroup = speakerNameImage.gameObject.AddComponent<CanvasGroup>();

        CanvasGroup textGroup = null;
        if (fadeText && dialogueText != null)
        {
            textGroup = dialogueText.GetComponent<CanvasGroup>();
            if (textGroup == null)
                textGroup = dialogueText.gameObject.AddComponent<CanvasGroup>();
        }

        float t = 0f;
        float startAlphaName = nameGroup.alpha;
        float startAlphaText = textGroup != null ? textGroup.alpha : 1f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(startAlphaName, 0f, t / duration);
            nameGroup.alpha = alpha;
            if (textGroup != null)
                textGroup.alpha = Mathf.Lerp(startAlphaText, 0f, t / duration);
            yield return null;
        }

        speakerNameImage.sprite = newSprite;
        speakerNameImage.enabled = newSprite != null;

        t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            nameGroup.alpha = Mathf.Lerp(0f, 1f, t / duration);
            yield return null;
        }
        nameGroup.alpha = 1f;

        if (textGroup != null)
            textGroup.alpha = 1f;
    }

    private void PauseGameForDialogue()
    {
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        if (portraitController != null && portraitController.portraitAnimator != null)
        {
            dialoguePortraitAnimator = portraitController.portraitAnimator;
            dialoguePortraitAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
        }
    }

    private void ResumeGameAfterDialogue()
    {
        Time.timeScale = previousTimeScale;

        if (dialoguePortraitAnimator != null)
            dialoguePortraitAnimator.updateMode = AnimatorUpdateMode.Normal;

        dialoguePortraitAnimator = null;
    }
}
