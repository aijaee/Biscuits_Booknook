using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public void GoToNextScene()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            LevelTransition.Instance.TransitionToScene(nextSceneIndex);
        }
    }

        public void GoToScene(int sceneIndex)
    {
        if (sceneIndex >= 0 && sceneIndex < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings)
        {
            LevelTransition.Instance.TransitionToScene(sceneIndex);
        }
    }
}