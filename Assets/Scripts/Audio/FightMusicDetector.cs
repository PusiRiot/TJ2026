using UnityEngine;

public class FightMusicDetector : MonoBehaviour
{
    [SerializeField] private float fightDistance = 5f; // ajusta según el tamaño de tu arena

    private Transform player1;
    private Transform player2;
    private bool isFightMusicPlaying = false;

    private void Start()
    {
        player1 = GameObject.FindGameObjectWithTag("Player1")?.transform;
        player2 = GameObject.FindGameObjectWithTag("Player2")?.transform;

        MusicManager.Instance.PlayGamePlayMusic();
    }

    private void Update()
    {
        if (player1 == null || player2 == null) return;

        float distance = Vector2.Distance(player1.position, player2.position);

        if (distance <= fightDistance && !isFightMusicPlaying)
        {
            MusicManager.Instance.PlayFightMusic();
            isFightMusicPlaying = true;
        }
        else if (distance > fightDistance && isFightMusicPlaying)
        {
            MusicManager.Instance.PlayGamePlayMusic();
            isFightMusicPlaying = false;
        }
    }
}
