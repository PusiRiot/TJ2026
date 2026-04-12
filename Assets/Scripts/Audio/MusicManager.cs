using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [SerializeField] private string musicEventName = "Play_Music"; // Your Wwise play event
    [SerializeField] private string stateGroupName = "Game_State"; // Your Wwise State Group name

    // State name constants — must match exactly what's in Wwise
    public const string STATE_TITLE = "TitleMenu";
    public const string STATE_FIGHT = "Fight";
    public const string STATE_PAUSE = "Pause";
    public const string STATE_END_GAME = "GameOver";
    public const string STATE_GAMEPLAY = "GamePlay";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        AkUnitySoundEngine.PostEvent(musicEventName, gameObject);
        SetMusicState(STATE_TITLE);
        AkUnitySoundEngine.SetRTPCValue("Music_Volume", 0f);
        AkUnitySoundEngine.SetRTPCValue("SFX_Volume", 80f);
    }

    public void SetMusicState(string stateName)
    {
        AkUnitySoundEngine.SetState(stateGroupName, stateName);
    }

    public void PlayTitleMusic() => SetMusicState(STATE_TITLE);
    public void PlayFightMusic() => SetMusicState(STATE_FIGHT);
    public void PlayPauseMusic() => SetMusicState(STATE_PAUSE);
    public void PlayEndGameMusic() => SetMusicState(STATE_END_GAME);
    public void PlayGamePlayMusic() => SetMusicState(STATE_GAMEPLAY);
}
