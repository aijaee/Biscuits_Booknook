using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Xml;

public class DialogueScriptStart : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI textComponent;    // For the dialogue text
    public Image nameComponent;             // For the character's name
    public Image characterImage;            // For the character's sprite
    public GameObject overworldControls;
    public GameObject startDialogueBox;
    public GameObject crow;
    public GameObject crowRock;

    [Header("Dialogue Settings")]
    public string[] lines;                  // Dialogue lines
    public Sprite[] characterNames;         // Corresponding character names
    public Sprite[] characterSprites;       // Corresponding character sprites
    public float textSpeed;                 // Speed of text 

    private int index;                      // Current dialogue index
    private bool isDialogueActive = false;  // Tracks if dialogue is active

    // Start is called before the first frame update
    void Start()
    {
        if (!PlayerPrefs.HasKey("StartDialogueActivated"))
        {
            StartDialogue();
        }
        else
        {
            Destroy(crow);
            crowRock.SetActive(true);
        }
    }

    void Update()
    {
        if (!isDialogueActive) return; // Skip input if dialogue is not active

        if (Input.GetMouseButtonDown(0)) // Detect left-click
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

    public void StartDialogue()
    {
        // Stop time for the game world
        Time.timeScale = 0;
        isDialogueActive = true;

        // Reset UI and initialize
        startDialogueBox.SetActive(true);
        textComponent.text = string.Empty;
        nameComponent.sprite = null;
        characterImage.sprite = null;
        index = 0;

        UpdateCharacterInfo();
        StartCoroutine(TypeLine());

        PlayerPrefs.SetInt("StartDialogueActivated", 1);
        PlayerPrefs.Save();
    }

    IEnumerator TypeLine()
    {
        foreach (char c in lines[index].ToCharArray())
        {
            textComponent.text += c;
            // Use unscaled time for typing
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

    void EndDialogue()
    {
        // Keep the game world time paused
        Time.timeScale = 0f;

        // Hide dialogue UI
        startDialogueBox.SetActive(false);

        // Activate overworld controls
        if (overworldControls != null)
        {
            overworldControls.SetActive(true);
        }

        isDialogueActive = false;
    }


    void UpdateCharacterInfo()
    {
        // Update the character's name
        if (index < characterNames.Length)
        {
            nameComponent.sprite = characterNames[index];
        }
        else
        {
            nameComponent.sprite = null; // Default to empty if no name is provided
        }

        // Update the character's sprite
        if (index < characterSprites.Length)
        {
            characterImage.sprite = characterSprites[index];
        }
        else
        {
            characterImage.sprite = null; // Default to null if no sprite is provided
        }
    }

    public void CloseControlsOverworld()
    {
        Destroy(crow);
        crowRock.SetActive(true);
        overworldControls.SetActive(false);
        Time.timeScale = 1f;
    }
}
