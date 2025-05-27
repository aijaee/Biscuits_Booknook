using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueScriptTrigger : MonoBehaviour
{ 

    [Header("UI Components")]
    public TextMeshProUGUI textComponent;
    public Image nameComponent;
    public Image characterImage;
    public GameObject dialogueBoxTriggered;

    [Header("Dialogue Settings")]
    public string[] lines;
    public Sprite[] characterNames;
    public Sprite[] characterSprites;
    public float textSpeed;

    [Header("Unique ID")]
    public string uniqueID; // Assign a unique ID to each trigger

    private int index;
    private bool isDialogueActive = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isDialogueActive && !PlayerPrefs.HasKey("Dialogue_" + uniqueID))
        {
            StartDialogue();
        }
    }

    void StartDialogue()
    {
        Time.timeScale = 0;
        isDialogueActive = true;
        index = 0;
        dialogueBoxTriggered.SetActive(true);
        textComponent.text = string.Empty;
        nameComponent.sprite = null;
        characterImage.sprite = null;
        UpdateCharacterInfo();
        StartCoroutine(TypeLine());

        // Save that this dialogue has been triggered
        PlayerPrefs.SetInt("Dialogue_" + uniqueID, 1);
        PlayerPrefs.Save();
    }

    void Update()
    {
        if (isDialogueActive && Input.GetMouseButtonDown(0))
        {
            if (textComponent.text == lines[index])
            {
                NextLine();
            }
            else
            {
                StopAllCoroutines();
                textComponent.text = lines[index];
            }
        }
    }

    IEnumerator TypeLine()
    {
        foreach (char c in lines[index].ToCharArray())
        {
            textComponent.text += c;
            yield return new WaitForSecondsRealtime(textSpeed);
        }
    }

    void NextLine()
    {
        if (index < lines.Length - 1)
        {
            index++;
            textComponent.text = string.Empty;
            UpdateCharacterInfo();
            StartCoroutine(TypeLine());
        }
        else
        {
            EndDialogue();
        }
    }

    void UpdateCharacterInfo()
    {
        if (index < characterNames.Length)
        {
            nameComponent.sprite = characterNames[index];
        }
        else
        {
            nameComponent.sprite = null;
        }

        if (index < characterSprites.Length)
        {
            characterImage.sprite = characterSprites[index];
        }
        else
        {
            characterImage.sprite = null;
        }
    }

    void EndDialogue()
    {
        Time.timeScale = 1;
        dialogueBoxTriggered.SetActive(false);
        isDialogueActive = false;
        Destroy(gameObject);
    }
}
