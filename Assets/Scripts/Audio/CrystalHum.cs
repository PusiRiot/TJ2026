using UnityEngine;

public class CrystalHum : MonoBehaviour
{
    [SerializeField] private float humDistance = 3f;
    private Transform player1;
    private Transform player2;
    private bool isPlaying = false;

    private void Start()
    {
        player1 = GameObject.FindGameObjectWithTag("Player1")?.transform;
        player2 = GameObject.FindGameObjectWithTag("Player2")?.transform;
    }

    private void Update()
    {
        bool playerNearby = IsPlayerClose(player1) || IsPlayerClose(player2);

        if (playerNearby && !isPlaying)
        {
            AkUnitySoundEngine.PostEvent("Play_Crystal_Hum", gameObject);
            isPlaying = true;
        }
        else if (!playerNearby && isPlaying)
        {
            AkUnitySoundEngine.PostEvent("Stop_Crystal_Hum", gameObject); // make a stop event in Wwise
            isPlaying = false;
        }
    }

    private bool IsPlayerClose(Transform player)
    {
        if (player == null) return false;
        return Vector3.Distance(transform.position, player.position) <= humDistance;
    }
}
