using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTransition : MonoBehaviour
{
    public static LevelTransition Instance { get; private set; }

    [Header("References")]
    public Animator crossfadeAnimator; // Animator with Crossfade_Start & Crossfade_End
    public float transitionDuration = 1f; // Adjust to match animation length

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void TransitionToScene(int sceneIndex)
    {
        StartCoroutine(DoTransition(sceneIndex));
    }

    private IEnumerator DoTransition(int sceneIndex)
    {
        // Play fade-out (to black)
        crossfadeAnimator.SetTrigger("CrossfadeStart");

        // Wait until animation finishes
        yield return new WaitForSeconds(transitionDuration);

        // Load next scene
        SceneManager.LoadScene(sceneIndex);

        // Play fade-in (from black)
        crossfadeAnimator.SetTrigger("CrossfadeEnd");
    }
}
