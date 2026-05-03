using TMPro;
using UnityEngine;

/// <summary>
/// Give to BackButton on a screen, dont give it a button component
/// </summary>
public class UIBackButton : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI bindingText;

    public void ChangeText(string text)
    {
        bindingText.text = text;
    }
}
