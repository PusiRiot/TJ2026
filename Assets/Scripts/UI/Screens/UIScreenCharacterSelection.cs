public class UIScreenCharacterSelection : UIScreen
{
    CharacterSelection[] players;

    private void Awake()
    {
        players = GetComponentsInChildren<CharacterSelection>();

        foreach (var p in players)
        {
            p.PlayerReadyChanged.AddListener(OnPlayerReadyChanged);
        }
    }

    private void OnPlayerReadyChanged(bool _)
    {
        bool allReady = true;

        foreach (var p in players)
        {
            if (!p.PlayerReady)
            {
                allReady = false;
                break;
            }
        }

        if (allReady)
            UINavigationManager.Instance.LoadScene(SceneName.GameScene);
    }
}
