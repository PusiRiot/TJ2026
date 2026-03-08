using UnityEngine;

public class UIScreen : MonoBehaviour
{
    [SerializeField] protected ScreenName screenName;

    public string GetName()
    {
        return screenName.ToString();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }
}
