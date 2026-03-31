using UnityEngine;

public class HealSpawn : MonoBehaviour, IObserver<PlayerCombatEvent>
{
    private GameObject healObject;
    bool firstDamage = false;

    void Awake()
    {
        Heal foundHealScript = FindAnyObjectByType<Heal>(FindObjectsInactive.Include);

        if (foundHealScript != null)
        {
            healObject = foundHealScript.gameObject;
        }
        else
        {
            Debug.LogError("Could not find an object with the Heal script in the scene!");
        }
    }

    public void OnNotify(PlayerCombatEvent evt, object data = null)
    {
        switch (evt)
        {
            case PlayerCombatEvent.ReceivedDamage:
            {
                if (!firstDamage)
                {
                    firstDamage = true;
                    healObject.SetActive(true);
                }
                break;
            }
        }
    }
}
