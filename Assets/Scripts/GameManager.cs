using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

/// <summary>
/// Game logic and state
/// </summary>
public class GameManager : Subject<GameEvent>
{
    #region Variables
    int[] teamScore = new int[2] { 0, 0 };

    bool suddenDeathEnabled = false;
    public bool SuddenDeathEnabled { get { return suddenDeathEnabled; } }

    int maxScore;
    #endregion

    #region Singleton implementation
    public static GameManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        UnpauseGame();
    }

    #endregion

    private void Start()
    {
        AddObserversOnScene();

        maxScore = FindObjectsByType<Crystal>(FindObjectsSortMode.None).Length;
    }

    #region Pause Game
    public void PauseGame()
    {
        Time.timeScale = 0;
    }

    public void UnpauseGame()
    {
        Time.timeScale = 1;
    }
    #endregion

    #region Score management

    public void ChangeScore(int teamIndex, int scoreChange)
    {
        teamScore[teamIndex] += scoreChange;
        Debug.Log($"Team {teamIndex} score changed! Current score: {teamScore[0]} - {teamScore[1]}");
        Notify(GameEvent.ScoreUpdate, teamScore);
        CheckWinCondition();
    }

    #endregion

    #region End game conditions management

    private void CheckWinCondition()
    {
        if (!suddenDeathEnabled)
        {
            if (teamScore[0] >= maxScore || teamScore[1] >= maxScore)
                Notify(GameEvent.GameEnd, teamScore);
        } else
        {
            if (teamScore[0] != teamScore[1])
                Notify(GameEvent.GameEnd, teamScore);
        }
    }

    public void TimerEnded()
    {
        if (teamScore[0] != teamScore[1])
        {
            Notify(GameEvent.GameEnd, teamScore);
        }
        else
        {
            Notify(GameEvent.SuddenDeath);
            suddenDeathEnabled = true;
        }
    }
    #endregion
}
