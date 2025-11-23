using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserManualCheck : MonoBehaviour
{
    public GameObject UserManualPanel;
    [SerializeField] private float delaySeconds = 2f;
    [SerializeField] private float fadeDuration = 1f;

    private void Start()
    {
        CheckAndOpenOnFreshStart();
    }

    private void CheckAndOpenOnFreshStart()
    {
        int hasPlayed = PlayerPrefs.GetInt("HasPlayedBefore", 0);
        
        if (hasPlayed == 0)
        {
            SetPrefsHasPlayed();
            StartCoroutine(OpenMenuAfterDelay());
        }
    }

    private void SetPrefsHasPlayed()
    {
        PlayerPrefs.SetInt("HasPlayedBefore", 1);
        PlayerPrefs.Save();
    }

    private IEnumerator OpenMenuAfterDelay()
    {
        yield return new WaitForSeconds(delaySeconds);
        
        if (UserManualPanel != null)
        {
            CanvasGroup canvasGroup = UserManualPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = UserManualPanel.AddComponent<CanvasGroup>();
            }
            
            canvasGroup.alpha = 0f;
            UserManualPanel.SetActive(true);
            
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = 1f;
            
            Time.timeScale = 0f;
            
            yield return null;
            if (UserManualUI.Instance != null)
            {
                UserManualUI.Instance.OpenMenu();
            }
        }
    }
}
