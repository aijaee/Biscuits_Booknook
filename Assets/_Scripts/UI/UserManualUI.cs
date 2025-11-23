using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UserManualUI : MonoBehaviour
{
    public static UserManualUI Instance { get; private set; }

    [Header("UI References")]
    public GameObject passageMenuPanel;
    public RawImage pageImage;
    public Button nextButton;
    public Button previousButton;
    public TMP_Text pageCounterUI;

    public static bool isOpen;

    [Header("Manual Pages")]
    public List<Sprite> pages;

    private int currentPageIndex = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);
        }

        if (passageMenuPanel != null)
        {
            passageMenuPanel.SetActive(false);
        }
        isOpen = false;

        if (nextButton != null)
            nextButton.onClick.AddListener(NextPage);
        if (previousButton != null)
            previousButton.onClick.AddListener(PreviousPage);
    }

    public void OpenMenu()
    {   
        currentPageIndex = 0;

        UpdatePageContent();
        UpdateNavigationButtons();
        UpdatePageCounter();

        passageMenuPanel.SetActive(true);
        Time.timeScale = 0f;
        isOpen = true;
    }

    public void OpenMenu(Sprite page)
    {
        pages = new List<Sprite> { page };
        currentPageIndex = 0;

        UpdatePageContent();
        UpdateNavigationButtons();
        UpdatePageCounter();

        passageMenuPanel.SetActive(true);
        Time.timeScale = 0f;
        isOpen = true;
    }

    public void OpenMenu(List<Sprite> pageSprites)
    {
        pages = pageSprites;
        currentPageIndex = 0;

        UpdatePageContent();
        UpdateNavigationButtons();
        UpdatePageCounter();

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
        if (pageImage != null && pages != null && pages.Count > currentPageIndex && pages[currentPageIndex] != null)
        {
            pageImage.texture = pages[currentPageIndex].texture;
        }
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