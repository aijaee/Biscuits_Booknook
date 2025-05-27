using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    [Header("----- Audio Source -----")]
    [SerializeField] AudioSource masterSource;
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;
    [SerializeField] AudioSource enviSource;
    [SerializeField] AudioSource UISource;

    [Header("----- Audio Clip -----")]
    public AudioClip background;
    public AudioClip backgroundOW;

    private void Start()
    {
        if (SceneManager.GetActiveScene().name == "Overworld")
        {
            musicSource.clip = backgroundOW;
        }
        else
        {
            musicSource.clip = background;
        }

        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        SFXSource.PlayOneShot(clip);
        /* AudioManager audioManager;
         * private void Awake()
         * {
         *      audioManager = GameObject.FindGameObjectwithTag("Audio").GetComponent<AudioManager>();
         * }
         * 
         * audioManager.PlaySFX(audioManager.name); */
    }

    void Update()
    {
        
    }
}
