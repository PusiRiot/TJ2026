using UnityEngine;

public class UIScreen : MonoBehaviour
{
    [SerializeField] protected ScreenName screenName;

    public string GetName()
    {
        return screenName.ToString();
    }

    virtual public void Hide()
    {
        gameObject.SetActive(false);
    }

    virtual public void Show()
    {
        gameObject.SetActive(true);
    }
}
