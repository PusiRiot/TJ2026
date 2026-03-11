using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIScreen : MonoBehaviour
{
    [SerializeField] protected ScreenName screenName;
    [SerializeField] protected GameObject firstToNavigate;

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
        EventSystem.current.SetSelectedGameObject(firstToNavigate);
    }
}
