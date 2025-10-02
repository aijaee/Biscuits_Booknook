using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PassageMenuUI : MonoBehaviour
{
    public static PassageMenuUI Instance { get; private set; }

    [Header("UI References")]
    public GameObject passageMenuPanel;
    public TMP_Text passageTitleUI;
    public TMP_Text passageBodyUI;
    public Button nextButton;
    public Button previousButton;
    public TMP_Text pageCounterUI;

    public static bool isOpen;

    [Header("Manual Pages")]
    public bool manualPages = true;
    [TextArea(3, 10)]
    public List<string> titles;
    public List<string> pages;
    [TextArea(1, 5)]

    private int currentPageIndex = 0;

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

        if (nextButton != null)
            nextButton.onClick.AddListener(NextPage);
        if (previousButton != null)
            previousButton.onClick.AddListener(PreviousPage);
    }

    public void OpenMenu(string title, string body, bool isStartingNote = false)
    {
        manualPages = false;
        pages = new List<string> { body };
        titles = new List<string> { title };
        currentPageIndex = 0;

        UpdatePageContent();
        UpdateNavigationButtons();
        UpdatePageCounter();
        SetAlignment(isStartingNote);

        passageTitleUI.enableAutoSizing = true;
        passageBodyUI.enableAutoSizing = true;
        passageTitleUI.fontStyle = FontStyles.Bold;
        passageTitleUI.fontSizeMax = 64f;
        passageBodyUI.fontSizeMax = 36f;

        passageMenuPanel.SetActive(true);
        Time.timeScale = 0f;
        isOpen = true;
    }

    public void OpenMenu(List<string> titlesList, List<string> bodyPages, bool isStartingNote = false)
    {
        manualPages = true;
        pages = bodyPages;

        if (titlesList != null && titlesList.Count == bodyPages.Count)
            titles = titlesList;
        else
            titles = new List<string>(new string[bodyPages.Count]);

        currentPageIndex = 0;

        UpdatePageContent();
        UpdateNavigationButtons();
        UpdatePageCounter();
        SetAlignment(isStartingNote);

        passageTitleUI.enableAutoSizing = true;
        passageBodyUI.enableAutoSizing = true;
        passageTitleUI.fontStyle = FontStyles.Bold;
        passageTitleUI.fontSizeMax = 64f;
        passageBodyUI.fontSizeMax = 36f;

        passageMenuPanel.SetActive(true);
        Time.timeScale = 0f;
        isOpen = true;
    }

    private void SetAlignment(bool isStartingNote)
    {
        if (isStartingNote)
        {
            passageTitleUI.alignment = TextAlignmentOptions.Bottom;
            passageBodyUI.alignment = TextAlignmentOptions.MidlineLeft;
        }
        else
        {
            passageTitleUI.alignment = TextAlignmentOptions.Bottom;
            passageBodyUI.alignment = TextAlignmentOptions.Center;
        }
    }

    public void CloseMenu()
    {
        passageMenuPanel.SetActive(false);
        Time.timeScale = 1f;
        isOpen = false;
    }

    public void NextPage()
    {
        if (pages.Count == 0) return;

        if (currentPageIndex < pages.Count - 1)
        {
            currentPageIndex++;
            UpdatePageContent();
            UpdateNavigationButtons();
            UpdatePageCounter();
        }
    }

    public void PreviousPage()
    {
        if (pages.Count == 0) return;

        if (currentPageIndex > 0)
        {
            currentPageIndex--;
            UpdatePageContent();
            UpdateNavigationButtons();
            UpdatePageCounter();
        }
    }

    private void UpdatePageContent()
    {
        if (titles != null && titles.Count > currentPageIndex && !string.IsNullOrEmpty(titles[currentPageIndex]))
            passageTitleUI.text = titles[currentPageIndex];
        else
            passageTitleUI.text = "";

        if (pages != null && pages.Count > currentPageIndex)
            passageBodyUI.text = pages[currentPageIndex];
        else
            passageBodyUI.text = "";
    }

    private void UpdateNavigationButtons()
    {
        if (previousButton != null)
            previousButton.gameObject.SetActive(currentPageIndex > 0);
        if (nextButton != null)
            nextButton.gameObject.SetActive(currentPageIndex < pages.Count - 1);
    }

    private void UpdatePageCounter()
    {
        if (pageCounterUI != null && pages.Count > 0)
            pageCounterUI.text = $"Page {currentPageIndex + 1}/{pages.Count}";
    }
}