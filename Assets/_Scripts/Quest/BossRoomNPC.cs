using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider2D))]
public class BossRoomNPC : MonoBehaviour, IInteractable
{
    [Header("Quest Settings")]
    public QuestTracker questTracker;

    [Header("Dialogue Settings")]
    public DialogueController dialogueController;
    public DialogueData dialogueBeforeQuest;
    public DialogueData dialogueAfterQuest;

    [Header("Portal Settings")]
    public GameObject portalPrefab;
    public Vector3 portalSpawnOffset = new Vector3(0, -2f, 0);
    public float portalFadeDuration = 1f;

    [Header("Player Upgrade")]
    public PlayerUpgrade upgradeToGive;


    bool playerInRange;
    bool hasSpawnedPortal;

    private IEnumerator Start()
    {
        yield return null; 
        if (questTracker == null)
            questTracker = FindObjectOfType<QuestTracker>();
        if (dialogueController == null)
            dialogueController = FindObjectOfType<DialogueController>();
    }

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

        bool questCompleted = questTracker != null && questTracker.objectives != null &&
                              System.Array.TrueForAll(questTracker.objectives, o => o.currentAmount >= o.targetAmount);

        DialogueData dialogueToUse = questCompleted ? dialogueAfterQuest : dialogueBeforeQuest;

        if (dialogueController != null && dialogueToUse != null)
        {
            dialogueController.OnDialogueComplete += OnDialogueFinished;
            dialogueController.StartDialogue(dialogueToUse);
            InteractUI.Instance.ShowButton(false);
        }
        else
        {
            OnDialogueFinished();
        }
    }

    private void OnDialogueFinished()
    {
        if (dialogueController != null)
            dialogueController.OnDialogueComplete -= OnDialogueFinished;

        bool questCompleted = questTracker != null && questTracker.objectives != null &&
                            System.Array.TrueForAll(questTracker.objectives, o => o.currentAmount >= o.targetAmount);

        if (questCompleted)
        {
            GiveUpgrade();
            StartCoroutine(HandlePortalSpawn());
            StartCoroutine(RemoveNPCAfterDialogue());
        }
        else
        {
            StartCoroutine(HandlePortalSpawn());
        }
    }

    private IEnumerator HandlePortalSpawn()
    {
        if (hasSpawnedPortal) yield break;

        bool questCompleted = questTracker != null && questTracker.objectives != null &&
                              System.Array.TrueForAll(questTracker.objectives, o => o.currentAmount >= o.targetAmount);

        if (portalPrefab != null && questCompleted)
        {
            GameObject portal = Instantiate(portalPrefab, transform.position + portalSpawnOffset, Quaternion.identity);

            if (!portal.TryGetComponent<SpriteRenderer>(out var sr)) yield break;
            TMP_Text tmp = portal.GetComponentInChildren<TMP_Text>();

            Color originalSpriteColor = sr.color;
            Color originalTMPColor = tmp != null ? tmp.color : Color.white;

            sr.color = new Color(originalSpriteColor.r, originalSpriteColor.g, originalSpriteColor.b, 0);
            if (tmp != null)
                tmp.color = new Color(originalTMPColor.r, originalTMPColor.g, originalTMPColor.b, 0);

            float t = 0f;
            while (t < portalFadeDuration)
            {
                t += Time.unscaledDeltaTime;
                float alpha = Mathf.Lerp(0f, 1f, t / portalFadeDuration);
                sr.color = new Color(originalSpriteColor.r, originalSpriteColor.g, originalSpriteColor.b, alpha);
                if (tmp != null)
                    tmp.color = new Color(originalTMPColor.r, originalTMPColor.g, originalTMPColor.b, alpha);
                yield return null;
            }

            sr.color = originalSpriteColor;
            if (tmp != null)
                tmp.color = originalTMPColor;

            hasSpawnedPortal = true;
        }
    }

    private void GiveUpgrade()
    {
        if (upgradeToGive == null) return;

        MeleeAttackController melee = FindObjectOfType<MeleeAttackController>();
        DashController dash = FindObjectOfType<DashController>();

        switch (upgradeToGive.upgradeType)
        {
            case UpgradeType.AddStrongAttack:
                if (melee != null)
                {
                    melee.maxComboStep = 4;
                    PlayerPrefs.SetInt("UnlockedCombo4", 1);
                    PlayerPrefs.Save();
                    Debug.Log("Unlocked 4th attack and saved!");
                }
                break;

            case UpgradeType.ReduceDashCooldown:
                if (dash != null)
                {
                    dash.dashCooldown = Mathf.Max(0.1f, dash.dashCooldown - upgradeToGive.value);
                    PlayerPrefs.SetFloat("DashCooldown", dash.dashCooldown);
                    PlayerPrefs.Save();
                    Debug.Log("Dash cooldown reduced and saved!");
                }
                break;
        }
    }

    private IEnumerator RemoveNPCAfterDialogue()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color originalColor = sr.color;
            float duration = 1f;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float alpha = Mathf.Lerp(originalColor.a, 0f, t / duration);
                sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return null;
            }
        }

        Destroy(gameObject);
    }
}