using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine;

public class VolumeSettings : MonoBehaviour
{
    public static VolumeSettings Instance { get; private set; }

    [SerializeField] private AudioMixer myMixer;
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private float masterVolume = 1f;
    private float musicVolume = 1f;
    private float sfxVolume = 1f;

    private void Awake()
    {
        LoadVolumeValues();
    }

    private void Start()
    {
        ApplyVolumesToSliders();
    }

    private void LoadVolumeValues()
    {
        if (PlayerPrefs.HasKey("masterVolume"))
        {
            masterVolume = PlayerPrefs.GetFloat("masterVolume");
            musicVolume = PlayerPrefs.GetFloat("musicVolume");
            sfxVolume = PlayerPrefs.GetFloat("sfxVolume");
        }

        ApplyVolumesToMixer();
    }

    public void ApplyVolumesToSliders()
    {
        if (masterSlider != null) masterSlider.value = masterVolume;
        if (musicSlider != null) musicSlider.value = musicVolume;
        if (sfxSlider != null) sfxSlider.value = sfxVolume;
    }

    private void ApplyVolumesToMixer()
    {
        if (myMixer != null)
        {
            myMixer.SetFloat("MasterVolume", Mathf.Log10(masterVolume) * 20);
            myMixer.SetFloat("MusicVolume", Mathf.Log10(musicVolume) * 20);
            myMixer.SetFloat("SFXVolume", Mathf.Log10(sfxVolume) * 20);
        }
    }

    public void SetMasterVolume()
    {
        if (masterSlider != null)
        {
            masterVolume = masterSlider.value;
            myMixer.SetFloat("MasterVolume", Mathf.Log10(masterVolume) * 20);
            PlayerPrefs.SetFloat("masterVolume", masterVolume);
        }
    }

    public void SetMusicVolume()
    {
        if (musicSlider != null)
        {
            musicVolume = musicSlider.value;
            myMixer.SetFloat("MusicVolume", Mathf.Log10(musicVolume) * 20);
            PlayerPrefs.SetFloat("musicVolume", musicVolume);
        }
    }

    public void SetSFXVolume()
    {
        if (sfxSlider != null)
        {
            sfxVolume = sfxSlider.value;
            myMixer.SetFloat("SFXVolume", Mathf.Log10(sfxVolume) * 20);
            PlayerPrefs.SetFloat("sfxVolume", sfxVolume);
        }
    }

    public void ResetToDefault()
    {
        masterVolume = 1f;
        musicVolume = 1f;
        sfxVolume = 1f;

        if (masterSlider != null) masterSlider.value = masterVolume;
        if (musicSlider != null) musicSlider.value = musicVolume;
        if (sfxSlider != null) sfxSlider.value = sfxVolume;

        ApplyVolumesToMixer();
        
        PlayerPrefs.SetFloat("masterVolume", masterVolume);
        PlayerPrefs.SetFloat("musicVolume", musicVolume);
        PlayerPrefs.SetFloat("sfxVolume", sfxVolume);
    }
}