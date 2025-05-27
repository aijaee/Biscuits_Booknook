using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GraphicsSettings : MonoBehaviour
{
    [Header("UI Components")]
    public TextMeshProUGUI resolutionText;
    public TextMeshProUGUI displayModeText;
    public TextMeshProUGUI renderingResolutionText;

    [Header("UI Buttons")]
    public Button resolutionLeftButton;
    public Button resolutionRightButton;
    public Button displayModeLeftButton;
    public Button displayModeRightButton;
    public Button renderingResolutionLeftButton;
    public Button renderingResolutionRightButton;

    [Header("Settings Arrays")]
    private int resolutionIndex = 0;
    private int displayModeIndex = 0;
    private int renderingResolutionIndex = 0;

    private string[] resolutions = { "1920x1080", "1366 x 768", "1280 x 720" };
    private string[] displayModes = { "Fullscreen", "Windowed" };
    private string[] renderingResolutions = { "Low", "Medium", "High" };

    void Start()
    {
        // Load saved settings
        LoadSettings();

        // Update UI texts with current settings
        UpdateResolutionUI();
        UpdateDisplayModeUI();
        UpdateRenderingResolutionUI();

        // Add listeners to the buttons to handle button clicks
        resolutionLeftButton.onClick.AddListener(OnResolutionLeft);
        resolutionRightButton.onClick.AddListener(OnResolutionRight);
        displayModeLeftButton.onClick.AddListener(OnDisplayModeLeft);
        displayModeRightButton.onClick.AddListener(OnDisplayModeRight);
        renderingResolutionLeftButton.onClick.AddListener(OnRenderingResolutionLeft);
        renderingResolutionRightButton.onClick.AddListener(OnRenderingResolutionRight);
    }

    // Left button click for changing resolution
    public void OnResolutionLeft()
    {
        resolutionIndex = Mathf.Max(0, resolutionIndex - 1);
        UpdateResolutionUI();
        SaveSettings(); // Save settings when changed
    }

    // Right button click for changing resolution
    public void OnResolutionRight()
    {
        resolutionIndex = Mathf.Min(resolutions.Length - 1, resolutionIndex + 1);
        UpdateResolutionUI();
        SaveSettings(); // Save settings when changed
    }

    // Left button click for changing display mode
    public void OnDisplayModeLeft()
    {
        displayModeIndex = Mathf.Max(0, displayModeIndex - 1);
        UpdateDisplayModeUI();
        SaveSettings(); // Save settings when changed
    }

    // Right button click for changing display mode
    public void OnDisplayModeRight()
    {
        displayModeIndex = Mathf.Min(displayModes.Length - 1, displayModeIndex + 1);
        UpdateDisplayModeUI();
        SaveSettings(); // Save settings when changed
    }

    // Left button click for changing rendering resolution
    public void OnRenderingResolutionLeft()
    {
        renderingResolutionIndex = Mathf.Max(0, renderingResolutionIndex - 1);
        UpdateRenderingResolutionUI();
        SaveSettings(); // Save settings when changed
    }

    // Right button click for changing rendering resolution
    public void OnRenderingResolutionRight()
    {
        renderingResolutionIndex = Mathf.Min(renderingResolutions.Length - 1, renderingResolutionIndex + 1);
        UpdateRenderingResolutionUI();
        SaveSettings(); // Save settings when changed
    }

    // Update resolution UI text
    void UpdateResolutionUI()
    {
        resolutionText.text = resolutions[resolutionIndex];
        string[] res = resolutions[resolutionIndex].Split('x');
        Screen.SetResolution(int.Parse(res[0]), int.Parse(res[1]), displayModes[displayModeIndex] == "Fullscreen");
    }

    // Update display mode UI text
    void UpdateDisplayModeUI()
    {
        displayModeText.text = displayModes[displayModeIndex];
        Screen.fullScreen = displayModeIndex == 0;
    }

    // Update rendering resolution UI text
    void UpdateRenderingResolutionUI()
    {
        renderingResolutionText.text = renderingResolutions[renderingResolutionIndex];
        QualitySettings.SetQualityLevel(renderingResolutionIndex);
    }

    // Save settings using PlayerPrefs
    void SaveSettings()
    {
        PlayerPrefs.SetInt("ResolutionIndex", resolutionIndex);
        PlayerPrefs.SetInt("DisplayModeIndex", displayModeIndex);
        PlayerPrefs.SetInt("RenderingResolutionIndex", renderingResolutionIndex);
        PlayerPrefs.Save();
    }

    // Load saved settings
    void LoadSettings()
    {
        if (PlayerPrefs.HasKey("ResolutionIndex"))
        {
            resolutionIndex = PlayerPrefs.GetInt("ResolutionIndex");
        }
        if (PlayerPrefs.HasKey("DisplayModeIndex"))
        {
            displayModeIndex = PlayerPrefs.GetInt("DisplayModeIndex");
        }
        if (PlayerPrefs.HasKey("RenderingResolutionIndex"))
        {
            renderingResolutionIndex = PlayerPrefs.GetInt("RenderingResolutionIndex");
        }
    }
}
