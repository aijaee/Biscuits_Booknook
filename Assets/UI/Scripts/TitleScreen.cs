using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreen : MonoBehaviour {
    public void PlayGame ()
    {
        Debug.Log("Playing");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);

    }

    public void OpenOptions()
    {
        Debug.Log("Opening Options");
        // Add later
    }
    public void QuitGame()
    {
        Debug.Log("Exiting");
        Application.Quit();
    }

    public void ButtonCheck()
    {
        Debug.Log("Pressed");
    }
}
