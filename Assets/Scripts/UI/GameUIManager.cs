using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class is responsible for managing the in game UI (score, timer...), it listen's to GameManager events.
/// </summary>
public class GameUIManager : Subject<GameUIAnimEvents>, IObserver<PlayerMovementEvent>, IObserver<GameEvent>, IObserver<PlayerCombatEvent>
{
    #region Variables
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI[] teamScoreTexts = new TextMeshProUGUI[2];
    [SerializeField] private Image[] playerDashEnabled = new Image[2];
    [SerializeField] private Image[] playerLives = new Image[2];
    [SerializeField] private TextMeshProUGUI[] abilityCooldownTexts = new TextMeshProUGUI[2];
    [SerializeField] private TextMeshProUGUI[] deathCooldownTexts = new TextMeshProUGUI[2];
    [SerializeField] private TextMeshProUGUI[] dashCooldownTexts = new TextMeshProUGUI[2];
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI timeUpText;

    [Header("Settings")]
    [SerializeField] private Color timerEndingColor;
    float timePassed = 0;
    int lastCurrentTime = 0;
    int _maxLives;

    bool timeBelowZero = false;
    #endregion

    #region Monobehaviour

    private void Start()
    {
        base.AddObserversOnScene();

        teamScoreTexts[0].text = "0";
        teamScoreTexts[1].text = "0";

        timerText.color = Color.white;
        timerText.text = System.TimeSpan.FromSeconds(GameStatsAccess.Instance.GetGameDuration()).ToString(@"mm\:ss");

        timeUpText.enabled = false;

        _maxLives = GameStatsAccess.Instance.GetMaxLives();
        playerLives[0].fillAmount = 1;
        playerLives[1].fillAmount = 1;

        //Audio
        MusicManager.Instance.PlayNoMusic();
        AkUnitySoundEngine.PostEvent("Play_rain", gameObject);

        StartCoroutine(StartAnimations());
    }

    void Update()
    {
        if (!timeBelowZero)
            UpdateTimer();
    }
    #endregion

    void UpdateTimer()
    {
        timePassed += Time.deltaTime;

        int currentTime = Mathf.CeilToInt(GameStatsAccess.Instance.GetGameDuration() - timePassed);

        if (currentTime == lastCurrentTime) return; // same second

        lastCurrentTime = currentTime;

        if (currentTime <= 0)
        {
            timeBelowZero = true;
            StartCoroutine(TimeUp());
        }

        if (currentTime <= 10)
        {
            AnimateTimerEnding();
        }

        if(currentTime == 10 )
        {
            StartCoroutine(AudioEndCountdown());
        }

        timerText.text = System.TimeSpan.FromSeconds(currentTime).ToString(@"mm\:ss");
    }

    IEnumerator StartAnimations()
    {
        yield return new WaitForSecondsRealtime(1);

        Notify(GameUIAnimEvents.LightningOnStart);

        yield return new WaitForSecondsRealtime(2);

        Notify(GameUIAnimEvents.PlayersLightOn);

        yield return new WaitForSecondsRealtime(2);

        //Audio
        AkUnitySoundEngine.PostEvent("Play_Countdown", gameObject);

        timeUpText.enabled = true;
                for (int i = 3; i > 0; i--)
        {
            timeUpText.text = i + "...";
            LeanTween.scale(timeUpText.gameObject, Vector3.one * 1.3f, 0.8f)
            .setEasePunch()
            .setIgnoreTimeScale(true);
            yield return new WaitForSecondsRealtime(1);
        }

        timeUpText.text = "START!";
        LeanTween.scale(timeUpText.gameObject, Vector3.one * 1.3f, 0.8f)
            .setEasePunch()
            .setIgnoreTimeScale(true);
        yield return new WaitForSecondsRealtime(1);

        timeUpText.enabled = false;

        Notify(GameUIAnimEvents.GameStart);

        MusicManager.Instance.PlayGamePlayMusic();

        GameManager.Instance.InitializationComplete();
    }

    IEnumerator TimeUp()
    {
        GameManager.Instance.PauseGame(); // pause game and set message that time's up
        timeUpText.text = "TIME'S UP!";
        timeUpText.enabled = true;

        //Audio
        AkUnitySoundEngine.PostEvent("Play_EndBell", gameObject);

        // Animation on appear
        LeanTween.scale(timeUpText.gameObject, Vector3.one * 1.5f, 0.8f)
        .setEaseInOutQuad()
        .setIgnoreTimeScale(true);

        yield return new WaitForSecondsRealtime(2);

        FadeToBlack();

        yield return new WaitForSecondsRealtime(0.5f);

        GameManager.Instance.TimerEnded(); // Notify GameManager of timer end
    }

    void FadeToBlack()
    {
        timeUpText.alpha = 1f;
        LeanTween.value(timeUpText.gameObject, 1f, 0f, 0.4f)
            .setOnUpdate((float val) => { timeUpText.alpha = val; })
            .setIgnoreTimeScale(true);
    }

    IEnumerator SuddenDeath()
    {
        timeUpText.text = "SUDDEN DEATH!";
        timeUpText.alpha = 1f;
        timeUpText.color = timerEndingColor;

        //Audio
        AkUnitySoundEngine.PostEvent("Play_EndBell", gameObject);

        yield return new WaitForSecondsRealtime(2);

        // animation when disappearing
        timerText.enabled = false;
        Vector3 targetPos = timeUpText.transform.parent.InverseTransformPoint(timerText.transform.position);
        LeanTween.move(timeUpText.rectTransform, targetPos, 0.8f)
                .setEaseInOutBack()
                .setIgnoreTimeScale(true);

        LeanTween.scale(timeUpText.gameObject, Vector3.one * 0.5f, 0.8f)
            .setEaseInOutQuad()
            .setIgnoreTimeScale(true);

        //Audio
        AkUnitySoundEngine.SetRTPCValue("Music_Speed", 75f, null, 2000);

        GameManager.Instance.UnpauseGame();
    }

    void AnimateTimerEnding()
    {
        LeanTween.cancelAll();

        LeanTween.scale(timerText.gameObject, Vector3.one * 1.1f, 1f)
           .setEasePunch();

        LeanTween.value(timerText.gameObject, Color.white, timerEndingColor, 0.5f).setOnUpdate((Color val) => { timerText.color = val; }).setEaseInOutCubic().setLoopPingPong(1);
    }

    IEnumerator AudioEndCountdown()
    {
        //Audio
        AkUnitySoundEngine.PostEvent("Play_clockTick", gameObject);

        for (int i = 9; i > 0; i--)
        {
            yield return new WaitForSecondsRealtime(1);

            AkUnitySoundEngine.PostEvent("Play_clockTick", gameObject);
        }
    }

    #region IObserver
    public void OnNotify(PlayerMovementEvent evt, object data = null)
    {
        switch (evt)
        {
            case PlayerMovementEvent.DashConsumed:
                {
                    int teamIndex = (int)data;
                    playerDashEnabled[teamIndex].gameObject.SetActive(false);
                    break;
                }
            case PlayerMovementEvent.DashEnabled:
                {
                    int teamIndex = (int)data;
                    playerDashEnabled[teamIndex].gameObject.SetActive(true);
                    break;
                }
            case PlayerMovementEvent.DashCooldownUpdate:
                {
                    int[] processedData = data as int[];
                    int teamIndex = processedData[0];
                    int remainingCooldown = processedData[1];
                    dashCooldownTexts[teamIndex].text = remainingCooldown > 0 ? remainingCooldown.ToString() : "";

                    break;
                }
        }
    }

    public void OnNotify(GameEvent evt, object data = null)
    {
        switch (evt)
        {
            case GameEvent.ScoreUpdate:
                {
                    int[] score = data as int[];
                    teamScoreTexts[0].text = score[0].ToString();
                    teamScoreTexts[1].text = score[1].ToString();
                    break;
                }
            case GameEvent.SuddenDeath:
                {
                    StartCoroutine(SuddenDeath());
                    break;
                }
        }
    }

    public void OnNotify(PlayerCombatEvent evt, object data = null)
    {
        switch (evt)
        {
            case PlayerCombatEvent.ReceivedDamage:
                {
                    int[] dataDamage = data as int[];
                    int teamIndex = dataDamage[0];
                    int damage = dataDamage[1];
                    playerLives[teamIndex].fillAmount = Mathf.Max(playerLives[teamIndex].fillAmount - (float)damage / (float)_maxLives, 0);
                    break;
                }
            case PlayerCombatEvent.ReceivedHeal:
                {
                    int[] dataHeal = data as int[];
                    int teamIndex = dataHeal[0];
                    int healAmount = dataHeal[1];
                    playerLives[teamIndex].fillAmount = Mathf.Min(playerLives[teamIndex].fillAmount + (float)healAmount / (float)_maxLives, _maxLives);
                    break;
                }
            case PlayerCombatEvent.BackToLife:
                {
                    playerLives[(int)data].fillAmount = 1;
                    break;
                }
            case PlayerCombatEvent.AbilityCooldownUpdate:
                {
                    int[] processedData = data as int[];
                    int teamIndex = processedData[0];
                    int remainingCooldown = processedData[1];
                    abilityCooldownTexts[teamIndex].text = remainingCooldown > 0 ? remainingCooldown.ToString() : "";

                    break;
                }
            case PlayerCombatEvent.DeathCooldownUpdate:
                {
                    int[] processedData = data as int[];
                    int teamIndex = processedData[0];
                    int remainingCooldown = processedData[1];
                    deathCooldownTexts[teamIndex].text = remainingCooldown > 0 ? remainingCooldown.ToString() : "";

                    break;
                }
        }
    }
    #endregion
}
