using UnityEngine;

public class PlayerSFX : MonoBehaviour
{
    [SerializeField] private string hurtEvent;
    [SerializeField] private string turnOnEvent;
    [SerializeField] private string turnOffEvent;

    public void PlayHurt() => AkUnitySoundEngine.PostEvent(hurtEvent, gameObject);
    public void PlayTurnOn() => AkUnitySoundEngine.PostEvent(turnOnEvent, gameObject);
    public void PlayTurnOff() => AkUnitySoundEngine.PostEvent(turnOffEvent, gameObject);
}
