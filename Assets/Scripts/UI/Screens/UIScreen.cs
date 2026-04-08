using UnityEngine;
using UnityEngine.EventSystems;

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
        if (screenName == ScreenName.Game)
            GameManager.Instance.UnpauseGame();
        if (screenName == ScreenName.Pause)
            GameManager.Instance.PauseGame();

        EventSystem.current.SetSelectedGameObject(firstToNavigate);
        gameObject.SetActive(true);
    }
}
