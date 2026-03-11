public class UIScreenPause : UIScreen
{
    override public void Hide()
    {
        GameManager.Instance.UnpauseGame();
        gameObject.SetActive(false);
    }

    override public void Show()
    {
        GameManager.Instance.PauseGame();
        gameObject.SetActive(true);
    }
}
