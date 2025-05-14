using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{
    public AudioMixer audioMixer;
    public Slider volumeSlider;
    public Slider musicSlider;
    public Slider sfxSlider;
    void Start()
    {
        if (audioMixer.GetFloat("Master", out float savedMasterVol))
        {
            volumeSlider.SetValueWithoutNotify(savedMasterVol);
        }

        if (audioMixer.GetFloat("Music", out float savedMusicVol))
        {
            musicSlider.SetValueWithoutNotify(savedMusicVol);
        }
        if (audioMixer.GetFloat("Sonor Effect", out float savedSfxVol))
        {
            musicSlider.SetValueWithoutNotify(savedSfxVol);
        }
    }
    
    public void SetVolumeMaster(float volume)
    {
        audioMixer.SetFloat("Master", volume);
    }
    public void SetVolumeMusic(float volume)
    {
        audioMixer.SetFloat("Music", volume);
    }
    public void SetSfxMusic(float volume)
    {
        audioMixer.SetFloat("Sonor Effect", volume);
    }
}