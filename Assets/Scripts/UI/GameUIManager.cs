using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This class is responsible for managing the in game UI (score, timer...), it listen's to GameManager events.
/// </summary>
public class GameUIManager : MonoBehaviour, IObserver<PlayerMovementEvent>, IObserver<GameEvent>, IObserver<PlayerCombatEvent>
{
    #region Variables
    [SerializeField] private TextMeshProUGUI[] teamScoreTexts = new TextMeshProUGUI[2];
    [SerializeField] private Image[] playerDashEnabled = new Image[2];
    [SerializeField] private Image[] playerAbilityEnabled = new Image[2];
    [SerializeField] private Image[] playerLives = new Image[2];
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI timeUpText;
    float timePassed = 0;
    int _maxLives;

    bool timeBelowZero = false;
    #endregion

    #region Monobehaviour

    private void Start()
    {
        teamScoreTexts[0].text = "0";
        teamScoreTexts[1].text = "0";

        timerText.text = System.TimeSpan.FromSeconds(GameStatsAccess.Instance.GetGameDuration()).ToString(@"mm\:ss");

        timeUpText.enabled = false;

        _maxLives = GameStatsAccess.Instance.GetMaxLives();
        playerLives[0].fillAmount = 1;
        playerLives[1].fillAmount = 1;

        //StartCoroutine(StartCountdown());
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
        if (currentTime <= 0)
        {
            timeBelowZero = true;
            StartCoroutine(TimeUp());
        }

        timerText.text = System.TimeSpan.FromSeconds(currentTime).ToString(@"mm\:ss");
    }

    IEnumerator StartCountdown()
    {
        GameManager.Instance.PauseGame();
        timeUpText.enabled = true;

        //for (int i = 3; i > 0; i--)
        //{
        //    timeUpText.text = i + "...";
        //    yield return new WaitForSecondsRealtime(1);
        //}
        timeUpText.text = "3...";
        yield return new WaitForSecondsRealtime(1);
        timeUpText.text = "2...";
        yield return new WaitForSecondsRealtime(1);
        timeUpText.text = "1...";
        yield return new WaitForSecondsRealtime(1);

        timeUpText.text = "¡Ya!";
        yield return new WaitForSecondsRealtime(1);
        timeUpText.enabled = false;

        GameManager.Instance.UnpauseGame();
    }

    IEnumerator TimeUp()
    {
        GameManager.Instance.PauseGame(); // pause game and set message that time's up
        timeUpText.text = "¡Tiempo!";
        timeUpText.enabled = true;
        yield return new WaitForSecondsRealtime(2);
        GameManager.Instance.TimerEnded(); // Notify GameManager of timer end
    }

    IEnumerator SuddenDeath()
    {
        timeUpText.text = "Muerte súbita";
        yield return new WaitForSecondsRealtime(2);
        GameManager.Instance.UnpauseGame();
        timeUpText.enabled = false;
    }

    #region IObserver
    public void OnNotify(PlayerMovementEvent evt, object data = null)
    {
        switch (evt)
        {
            case PlayerMovementEvent.DashConsumed:
                {
                    int teamIndex = (int)data;
                    Color teamColor = playerDashEnabled[teamIndex].color;
                    teamColor.a = 0.05f;
                    playerDashEnabled[teamIndex].color = teamColor;
                    break;
                }
            case PlayerMovementEvent.DashEnabled:
                {
                    int teamIndex = (int)data;
                    Color teamColor = playerDashEnabled[teamIndex].color;
                    teamColor.a = 1f;
                    playerDashEnabled[teamIndex].color = teamColor;
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
            case PlayerCombatEvent.AbilityEnabled:
                {
                    int[] dataTeam = data as int[];
                    int teamIndex = dataTeam[0];
                    Color teamColor = playerAbilityEnabled[teamIndex].color;
                    teamColor.a = 1f;
                    playerAbilityEnabled[teamIndex].color = teamColor;
                    break;
                }
            case PlayerCombatEvent.AbilityDisabled:
                {
                    int[] dataTeam = data as int[];
                    int teamIndex = dataTeam[0];
                    Color teamColor = playerAbilityEnabled[teamIndex].color;
                    teamColor.a = 0.05f;
                    playerAbilityEnabled[teamIndex].color = teamColor;
                    break;
                }
        }
    }
    #endregion
}
