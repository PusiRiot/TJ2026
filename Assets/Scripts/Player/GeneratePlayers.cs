using UnityEngine;

/// <summary>
/// Script to generate players on runtime when game starts based on the player selection of GameGlobalSettings
/// </summary>
public class GeneratePlayers : MonoBehaviour
{
    [SerializeField] GameObject[] players = new GameObject[2];
    [SerializeField] Transform[] startPosition = new Transform[2];

    private void Awake()
    {
        for (int i = 0; i < 2; i++)
        {
            PlayerCharacter character = GameGlobalSettings.Instance.GetPlayerCharacter(i);

            // get player with character
            foreach (GameObject p in players)
            {
                if (p.GetComponent<Player>().GetPlayerCharacter() == character)
                {
                    p.SetActive(false); // IMPORTANT: instanciate disabled so scripts only awake after player tag its assigned
                    GameObject instanciated = Instantiate(p);
                    instanciated.transform.position = startPosition[i].position;
                    instanciated.tag = i == 0 ? "Player1" : "Player2";
                    instanciated.SetActive(true);
                    break;
                }
            }
        }       
    }

    private void Start()
    {
        gameObject.AddComponent<InputManager>();
    }
}
