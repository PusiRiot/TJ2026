using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIScreenVictory : UIScreen, IObserver<GameEvent>
{
    [SerializeField] TextMeshProUGUI winningTeamText;
    [SerializeField] GameObject unlockedDiaryEntry;
    [SerializeField] Image peggy;
    [SerializeField] Image hives;

    public void OnNotify(GameEvent evt, object data = null)
    {
        if (evt == GameEvent.GameEnd)
        {
            int[] teamScore = (int[])data;

            if (teamScore[0] > teamScore[1])
            {
                PlayerCharacter winningCharacter = GameGlobalSettings.Instance.GetPlayerCharacter(0);

                peggy.gameObject.SetActive(winningCharacter == PlayerCharacter.Peggy);
                hives.gameObject.SetActive(winningCharacter == PlayerCharacter.DrHives);

                winningTeamText.text = "Player 1 wins";
            }

            else
            {
                PlayerCharacter winningCharacter = GameGlobalSettings.Instance.GetPlayerCharacter(1);

                peggy.gameObject.SetActive(winningCharacter == PlayerCharacter.Peggy);
                hives.gameObject.SetActive(winningCharacter == PlayerCharacter.DrHives);
                winningTeamText.text = "Player 2 wins";
            }

            bool unlocked = SystemGameDataStorage.Instance.UnlockDiaryEntries();

            unlockedDiaryEntry.SetActive(unlocked);

            Show();

            // unpause game but set game elements to inactive so they dont sound or anything
            GameObject gameElements = GameObject.FindGameObjectWithTag("GameElements");
            gameElements.SetActive(false);

            Player[] players = FindObjectsByType<Player>(FindObjectsSortMode.None);
            foreach (Player player in players)
            {
                player.gameObject.SetActive(false);
            }

            GameManager.Instance.UnpauseGame();

            MusicManager.Instance.PlayEndGameMusic();
        }
    }
}
