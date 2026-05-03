using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIScreen : MonoBehaviour
{
    [SerializeField] protected ScreenName screenName;
    [SerializeField] protected GameObject firstToNavigate;
    UIBackButton backBtn;

    virtual protected void Awake()
    {
        backBtn = GetComponentInChildren<UIBackButton>(true);

    }
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
        {
            MusicManager.Instance.PlayGamePlayMusic();

            if (!GameManager.Instance.GameInitializing)
                GameManager.Instance.UnpauseGame();
        }
        if (screenName == ScreenName.Pause)
        {
            MusicManager.Instance.PlayPauseMusic();
            GameManager.Instance.PauseGame();
        }

        EventSystem.current.SetSelectedGameObject(firstToNavigate);
        gameObject.SetActive(true);
    }

    public bool HasBackButton()
    {
        return backBtn != null;
    }

    public void ChangeBackButtonText(string text)
    {
        if (backBtn != null)
            backBtn.ChangeText(text);
    }
}
