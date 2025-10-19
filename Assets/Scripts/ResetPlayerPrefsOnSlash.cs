using UnityEngine;

public class ResetPlayerPrefsOnSlash : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Slash))
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("All PlayerPrefs have been cleared.");
        }
    }
}
