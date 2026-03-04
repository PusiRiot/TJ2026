using TMPro;
using UnityEngine;

/// <summary>
/// This class is responsible for managing the score UI, it listens to the GameManager's ChangeScore event and updates the score display accordingly.
/// </summary>
public class ScoreUIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI[] teamScoreTexts = new TextMeshProUGUI[2];
    [SerializeField] private TextMeshProUGUI timerText;
    float timePassed = 0;
    [SerializeField] private GameObject endGamePanel; // TODO: This is a temporary solution

    bool timeBelowZero = false;

    private void Awake()
    {
        // TODO: Once everything is more stablished we should add a way to ensure this is always assigned instead of manually checking the inspector, maybe with a tag or something like that
        if (teamScoreTexts == null)
        {
            Debug.LogWarning("Team score texts not assigned in the inspector, unless you want to see a sea of red on the console you should add them :)");
        }

        teamScoreTexts[0].text = "0";
        teamScoreTexts[1].text = "0";

        timerText.text = GameManager.Instance.GetGameDuration().ToString("F2");

        endGamePanel.SetActive(false);
    }

    private void OnEnable()
    {
        GameManager.Instance.UpdateUIScore += UpdateScoreUI;
        GameManager.Instance.EndGame += EndGameUI;
    }
    private void OnDisable()
    {
        GameManager.Instance.UpdateUIScore -= UpdateScoreUI;
        GameManager.Instance.EndGame -= EndGameUI;
    }

    void Update()
    {
        if (!timeBelowZero)
            UpdateTimer();
    }

    void UpdateScoreUI(int teamIndex, int teamScore)
    {
        teamScoreTexts[teamIndex].text = teamScore.ToString();
    }

    void UpdateTimer()
    {
        timePassed += Time.deltaTime;
        int currentTime = Mathf.CeilToInt(GameManager.Instance.GetGameDuration() - timePassed);
        if (currentTime <= 0) timeBelowZero = true;

        timerText.text = currentTime.ToString();
    }

    void EndGameUI(int winningTeamIndex)
    {
        Debug.Log($"Team {winningTeamIndex} wins!");
        endGamePanel.SetActive(true);
    }
}
