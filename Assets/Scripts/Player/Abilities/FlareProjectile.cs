using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class FlareProjectile : Subject<PlayerCombatEvent>
{
    private float _speed = -1f;
    private float _lifetimeIfWall = -1f;
    private float _lifetimeIfPlayer = -1f;
    [SerializeField]
    private float angularSpeed = 50f;
    [SerializeField]
    private Transform cookieLight;
    private Rigidbody rb;
    private int teamIndex;
    AbstractLight enemyLight;
    Animator anim;
    const float fadeAnimDuration = 0.333f;

    private void Awake()
    {
        base.AddObserversOnScene();
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        Crystal stuckCrystal = GetComponentInParent<Crystal>();
        if(stuckCrystal != null)
        {
            stuckCrystal.ReclaimFlag(teamIndex);
        }
        Heal stuckHeal = GetComponentInParent<Heal>();
        if(stuckHeal != null)
        {
            stuckHeal.ReclaimFlag(teamIndex);
        }
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

    private void FixedUpdate()
    {
        cookieLight.Rotate(new Vector3(1, 0, 0) * angularSpeed * Time.fixedDeltaTime);
    }

    private void OnTriggerEnter(Collider collision)
    {
        Debug.Log("Flare projectile collided with " + collision.gameObject.name);
        Player enemyPlayer = collision.transform.GetComponentInParent<Player>();
        if (enemyPlayer != null && enemyPlayer.gameObject.CompareTag("Player" + (teamIndex + 1)))
        {
            // ignore collision with player of the same team
            return;
        }

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        GetComponent<Collider>().enabled = false;
        transform.SetParent(collision.transform);
        
        if (enemyPlayer != null)
        {
            var lights = collision.transform.GetComponentInParent<Player>().transform.GetComponentsInChildren<AbstractLight>();

            foreach (var light in lights)
            {
                if (light.gameObject.CompareTag("PlayerLight"))
                {
                    enemyLight = light;
                    break;
                }
            }
            StartCoroutine(Stuck(true));
        }
        else
        {
            StartCoroutine(Stuck(false));
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.transform.GetComponentInParent<Player>() != null)
        {
            // ignore collision with players, since they are handled on trigger enter
            return;
        }
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        GetComponent<Collider>().enabled = false;
        transform.SetParent(collision.transform);
        StartCoroutine(Stuck(false));
    }
    
    private IEnumerator Stuck(bool stuckToPlayer)
    {
        //Audio
        //Audio
        AkUnitySoundEngine.PostEvent("Play_Peggy_Hability", gameObject);
        AkUnitySoundEngine.SetRTPCValue("Music_LowPassFilter", 80f);

        if (stuckToPlayer) { 
            enemyLight.TurnOff();
            yield return new WaitForSeconds(_lifetimeIfPlayer);
        }
        else
        {
            yield return new WaitForSeconds(_lifetimeIfWall);
        }
        Eliminate();
    }

    public void Eliminate()
    {
        AkUnitySoundEngine.SetRTPCValue("Music_LowPassFilter", 0f);
        if (enemyLight != null && !enemyLight.GetComponentInParent<PlayerCombat>().isDead)
        {
            enemyLight.TurnOn();
        }

        Notify(PlayerCombatEvent.StartAbilityCooldown, new int[] { teamIndex });
        anim.SetTrigger("Fade");
        Destroy(gameObject, fadeAnimDuration);
    }
}

