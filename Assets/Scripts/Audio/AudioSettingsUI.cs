using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsUI : MonoBehaviour
{
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private void Start()
    {
        musicSlider.value = 1f;
        sfxSlider.value = 1f;

        musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
    }

    private void OnMusicVolumeChanged(float value)
    {
        AkUnitySoundEngine.SetRTPCValue("Music_Volume", value * 100f);
    }

    private void OnSFXVolumeChanged(float value)
    {
        AkUnitySoundEngine.SetRTPCValue("SFX_Volume", value * 100f);
    }
}
