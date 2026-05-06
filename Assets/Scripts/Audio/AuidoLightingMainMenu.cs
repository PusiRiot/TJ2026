using UnityEngine;

public class AuidoLightingMainMenu : MonoBehaviour
{
    void PlayLightingSound ()
    {
        AkUnitySoundEngine.PostEvent("Play_Thunder", gameObject);
    }
}
