using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueScriptStartCombat : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI combatTextComponent;  // For the dialogue text
    public Image combatNameComponent;           // For the character's name
    public Image combatCharacterImage;          // For the character's sprite

    [Header("Dialogue Settings")]
    public string[] combatLines;                // Dialogue lines for combat
    public Sprite[] combatCharacterNames;       // Corresponding character names
    public Sprite[] combatCharacterSprites;     // Corresponding character sprites
    public float combatTextSpeed;               // Speed of text animation

    private int combatIndex;                    // Current dialogue index
    private bool isCombatDialogueActive = false; // Tracks if combat dialogue is active
    public GameObject combatManager;            // Reference to the combat manager to start combat

    void Start()
    {
        StartCombatDialogue();
    }

    void Update()
    {
        if (!isCombatDialogueActive) return;

        if (Input.GetMouseButtonDown(0)) // Detect left-click
        {
            if (combatTextComponent.text == combatLines[combatIndex])
            {
                NextCombatLine();
            }
            else
            {
                StopAllCoroutines();
                combatTextComponent.text = combatLines[combatIndex];
            }
        }
    }

    public void StartCombatDialogue()
    {
        Time.timeScale = 0;
        isCombatDialogueActive = true;

        gameObject.SetActive(true);
        combatTextComponent.text = string.Empty;
        combatNameComponent.sprite = null;
        combatCharacterImage.sprite = null;
        combatIndex = 0;

        UpdateCombatCharacterInfo();
        StartCoroutine(TypeCombatLine());
    }

    IEnumerator TypeCombatLine()
    {
        foreach (char c in combatLines[combatIndex].ToCharArray())
        {
            combatTextComponent.text += c;
            yield return new WaitForSecondsRealtime(combatTextSpeed);
        }
    }

    void NextCombatLine()
    {
        if (combatIndex < combatLines.Length - 1)
        {
            combatIndex++;
            combatTextComponent.text = string.Empty;
            UpdateCombatCharacterInfo();
            StartCoroutine(TypeCombatLine());
        }
        else
        {
            EndCombatDialogue();
        }
    }

    void EndCombatDialogue()
    {
        Time.timeScale = 1f; // Resume time

        gameObject.SetActive(false); // Hide dialogue UI
        isCombatDialogueActive = false;

        if (combatManager != null)
        {
            combatManager.SetActive(true); // Activate combat manager to start combat
        }
    }

    void UpdateCombatCharacterInfo()
    {
        if (combatIndex < combatCharacterNames.Length)
        {
            combatNameComponent.sprite = combatCharacterNames[combatIndex];
        }
        else
        {
            combatNameComponent.sprite = null;
        }

        if (combatIndex < combatCharacterSprites.Length)
        {
            combatCharacterImage.sprite = combatCharacterSprites[combatIndex];
        }
        else
        {
            combatCharacterImage.sprite = null;
        }
    }
}
