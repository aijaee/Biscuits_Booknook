using UnityEngine;

public class ResetPlayerPrefsOnSlash : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Slash))
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.SetInt("HasPlayedBefore", 0);
            PlayerPrefs.Save();
        }
    }
}
