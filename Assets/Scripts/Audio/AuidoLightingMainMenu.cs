using UnityEngine;

public class AuidoLightingMainMenu : MonoBehaviour
{
    void PlayLightingSound ()
    {
        AkUnitySoundEngine.PostEvent("Play_Thunder", gameObject);
    }

    void PlayZap1Sound()
    {
        AkUnitySoundEngine.PostEvent("Zap_1", gameObject);
    }

    void PlayZap2Sound()
    {
        AkUnitySoundEngine.PostEvent("Zap_2", gameObject);
    }

    void PlayZap3Sound()
    {
        AkUnitySoundEngine.PostEvent("Zap_3", gameObject);
    }
}
