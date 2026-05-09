using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AudioSettingsUI : MonoBehaviour
{
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    //private void Start()
    //{
    //    int valueType = 0;
    //    AkUnitySoundEngine.GetRTPCValue("Music_Volume", null, 0, out float musicValue, ref valueType);
    //    AkUnitySoundEngine.GetRTPCValue("SFX_Volume", null, 0, out float sfxValue, ref valueType);

    //    Debug.Log($"Music_Volume RTPC: {musicValue}");
    //    Debug.Log($"SFX_Volume RTPC: {sfxValue}");

    //    musicSlider.SetValueWithoutNotify(musicValue / 100f);
    //    sfxSlider.SetValueWithoutNotify(sfxValue / 100f);

    //    musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
    //    sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
    //}

    private void OnEnable()
    {
        float musicValue = PlayerPrefs.GetFloat("Music_Volume", 20f);
        float sfxValue = PlayerPrefs.GetFloat("SFX_Volume", 20f);

        musicSlider.SetValueWithoutNotify(musicValue / 100f);
        sfxSlider.SetValueWithoutNotify(sfxValue / 100f);

        AkUnitySoundEngine.SetRTPCValue("Music_Volume", musicValue);
        AkUnitySoundEngine.SetRTPCValue("SFX_Volume", sfxValue);

        musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
    }

    private void OnMusicVolumeChanged(float value)
    {
        float rtpcValue = value * 100f;
        PlayerPrefs.SetFloat("Music_Volume", rtpcValue);
        AkUnitySoundEngine.SetRTPCValue("Music_Volume", rtpcValue);
    }

    private void OnSFXVolumeChanged(float value)
    {
        float rtpcValue = value * 100f;
        PlayerPrefs.SetFloat("SFX_Volume", rtpcValue);
        AkUnitySoundEngine.SetRTPCValue("SFX_Volume", rtpcValue);
    }

    private void OnDisable()
    {
        musicSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
        sfxSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
    }

}
