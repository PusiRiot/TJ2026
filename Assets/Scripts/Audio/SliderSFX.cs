using UnityEngine;
using UnityEngine.EventSystems;

public class SliderSFX : MonoBehaviour, IPointerUpHandler
{
    public void OnPointerUp(PointerEventData eventData)
    {
        AkUnitySoundEngine.PostEvent("Select_UI", gameObject);
    }
}
