using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIScreenVictory : UIScreen, IObserver<GameEvent>
{
    [SerializeField] TextMeshProUGUI winningTeamText;
    [SerializeField] Image unlockedDiaryEntry;
    /* [SerializeField] TextMeshProUGUI buttonRematchText;
     [SerializeField] TextMeshProUGUI buttonMenuText;
     [SerializeField] Material skullMat;
     [SerializeField] Material fireMat;*/
    [SerializeField] Image peggy;
    [SerializeField] Image hives;


    /*[SerializeField] Color p1_color1;
    [SerializeField] Color p1_color2;
    [SerializeField] Color p2_color1;
    [SerializeField] Color p2_color2;*/

    public void OnNotify(GameEvent evt, object data = null)
    {
        if (evt == GameEvent.GameEnd)
        {
            int[] teamScore = (int[])data;
            if (teamScore[0] > teamScore[1])
            {
                winningTeamText.text = "Player 1 wins";
                /*winningTeamText.color = p1_color2;
                buttonRematchText.color = p1_color2;
                buttonMenuText.color = p1_color2;
                skullMat.SetColor("_Color", p1_color1);
                fireMat.SetColor("_Color", p1_color1);*/
            }

            else
            {
                winningTeamText.text = "Player 2 wins";
                /*winningTeamText.color = p2_color2;
                buttonRematchText.color = p2_color2;
                buttonMenuText.color = p2_color2;
                skullMat.SetColor("_Color", p2_color1);
                fireMat.SetColor("_Color", p2_color1);*/
            }

            bool unlocked = SystemGameDataStorage.Instance.UnlockDiaryEntries();

            unlockedDiaryEntry.enabled = unlocked;

            Show();

            MusicManager.Instance.PlayEndGameMusic();
        }
    }
}
