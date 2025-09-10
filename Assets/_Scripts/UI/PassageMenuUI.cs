using UnityEngine;
using TMPro;

public class PassageMenuUI : MonoBehaviour
{
    public static PassageMenuUI Instance { get; private set; }

    public GameObject passageMenuPanel;
    public TMP_Text passageTextUI;
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

    public void OpenMenu(string passage)
    {
        passageTextUI.text = passage;
        passageTextUI.enableAutoSizing = true;
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
