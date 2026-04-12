using UnityEngine;
using UnityEngine.UI;

public class AudioSettings : MonoBehaviour
{
    [SerializeField] private Slider musicSlider;

    private void Start()
    {
        musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
    }

    private void OnMusicVolumeChanged(float value)
    {
        AkUnitySoundEngine.SetRTPCValue("Music_Volume", value * 100f);
    }
}
