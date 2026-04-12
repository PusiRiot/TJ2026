using UnityEngine;

public class PlayerFootstepSFX : MonoBehaviour
{
    public void PlayFootstep()
    {
        AkUnitySoundEngine.PostEvent("Play_Footsteps", gameObject);
    }
}
