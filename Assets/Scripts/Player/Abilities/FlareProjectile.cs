using System.Collections;
using UnityEngine;

public class FlareProjectile : Subject<PlayerCombatEvent>
{
    private float _speed = -1f;
    private float _lifetimeIfWall = -1f;
    private float _lifetimeIfPlayer = -1f;
    private Rigidbody rb;
    private int teamIndex;
    AbstractLight enemyLight;

    private void Awake()
    {
        base.AddObserversOnScene();
    }

    public void Initialize(int inTeamIndex, PlayerStats playerStats)
    {
        teamIndex = inTeamIndex;
        _speed = playerStats.FlareSpeed;
        _lifetimeIfWall = playerStats.FlareLifetimeIfWall;
        _lifetimeIfPlayer = playerStats.FlareLifetimeIfPlayer;

        rb = GetComponentInChildren<Rigidbody>();
        rb.linearVelocity = transform.forward * _speed;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Player" + (teamIndex + 1)))
        {
            // ignore collision with player of the same team
            return;
        }

        rb.linearVelocity = Vector3.zero;
        rb.isKinematic = true;
        GetComponent<Collider>().enabled = false;
        transform.SetParent(collision.transform);

        int oppositeTeamIndex = ((teamIndex + 1) % 2) + 1;
        if (collision.gameObject.CompareTag("Player" + oppositeTeamIndex))
        {
            enemyLight = collision.gameObject.GetComponentInChildren<AbstractLight>();
            StartCoroutine(Stuck(true));
        }
        else
        {
            StartCoroutine(Stuck(false));
        }
    }
    // stuck to player

    // stuck to wall
    private IEnumerator Stuck(bool stuckToPlayer)
    {
        if (stuckToPlayer) { 
            enemyLight.TurnOff();
            yield return new WaitForSeconds(_lifetimeIfPlayer);
        }
        else
        {
            yield return new WaitForSeconds(_lifetimeIfWall);
        }
        Destroy(gameObject);

    }

    private void OnDestroy()
    {
        if(enemyLight != null)
        {
            enemyLight.TurnOn();
        }

        Notify(PlayerCombatEvent.StartAbilityCooldown, new int[]{ teamIndex});
    }
}

