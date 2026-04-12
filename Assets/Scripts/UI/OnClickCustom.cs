using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class AutoClickOnSelect : MonoBehaviour, ISelectHandler
{
    private Button _button;

    void Awake()
    {
        _button = GetComponent<Button>();
    }

    public void OnSelect(BaseEventData eventData)
    {
        // Verifica que el botón sea interactuable antes de pulsar
        if (_button != null && _button.interactable)
        {
            _button.onClick.Invoke();
        }
    }
}