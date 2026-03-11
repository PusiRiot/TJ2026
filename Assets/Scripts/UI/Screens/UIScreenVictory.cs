using TMPro;
using UnityEngine;

public class UIScreenVictory : UIScreen, IObserver<GameEvent>
{
    [SerializeField] TextMeshProUGUI winningTeamText;

    public void OnNotify(GameEvent evt, object data = null)
    {
        if (evt == GameEvent.GameEnd)
        {
            int[] teamScore = (int[])data;
            if (teamScore[0] > teamScore[1])
                winningTeamText.text = "Gana el jugador 1";
            else
                winningTeamText.text = "Gana el jugador 2";

            Show();
        }
    }
}
