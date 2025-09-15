using UnityEngine;
using TMPro;

public class PassageMenuUI : MonoBehaviour
{
    public static PassageMenuUI Instance { get; private set; }

    [Header("UI References")]
    public GameObject passageMenuPanel;
    public TMP_Text passageTitleUI;
    public TMP_Text passageBodyUI;

    public static bool isOpen;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        passageMenuPanel.SetActive(false);
        isOpen = false;
    }

    public void OpenMenu(string title, string body, bool isStartingNote)
    {
        // Set title and body text
        passageTitleUI.text = title;
        passageBodyUI.text = body;

        // Alignment rules
        if (isStartingNote)
        {
        // Title top-left
        passageTitleUI.alignment = TextAlignmentOptions.Bottom;

        // Body vertically centered but left aligned
        passageBodyUI.alignment = TextAlignmentOptions.MidlineLeft;
        }
        else
        {
        // Title top-center
        passageTitleUI.alignment = TextAlignmentOptions.Bottom;

        // Body centered both horizontally & vertically
        passageBodyUI.alignment = TextAlignmentOptions.Center;
        }

        passageTitleUI.enableAutoSizing = true;
        passageBodyUI.enableAutoSizing = true;
        passageTitleUI.fontSizeMax = 64f;
        passageBodyUI.fontSizeMax = 36f;

        passageMenuPanel.SetActive(true);
        Time.timeScale = 0f;
        isOpen = true;
    }

    public void CloseMenu()
    {
        passageMenuPanel.SetActive(false);
        Time.timeScale = 1f;
        isOpen = false;
    }
}
