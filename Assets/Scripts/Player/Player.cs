using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

/// <summary>
/// Base class for the player
/// <para>Contains references to the player's light and movement interfaces (to keep code separate).</para>
/// <para>The exact class that has the interface should be given on the Inspector, if not, it initializes with a basic one (this could also be done with an enum on inspector and a switch)</para>
/// <para>Configurable attributes are taken from an Scriptable object (PlayerStats). Again because there might be different players, but the skeleton/core should be the same for all.</para>
/// </summary>

public class Player : MonoBehaviour
{
    #region Variables
    // Read-only variables (preceded by _ (can't be set to readonly because they have to be initialized on runtime))
    [SerializeField] private PlayerStats _playerStats;
    [SerializeField] private PlayerAnimationsSet _animationSet; // assign in inspector
    private int _teamIndex = -1;

    // needed references
    private InputAction attackHoldAction;
    private PlayerMovement playerMovement;
    private PlayerCombat playerCombat;
    private PlayerAnimator playerAnimator;
    private AbstractHability playerHability;

    // booleans control
    private bool isDashEnabled = true;
    private bool isParryEnabled = true;
    private bool isHeavyMeleeEnabled = true;
    private bool isLightMeleeEnabled = true;
    private bool isHabilityEnabled = true;

    // cooldown durations
    private float _specialHabilityCooldownDuration = -1f;
    private float _lightMeleeCooldownDuration = -1f;
    private float _heavyMeleeCooldownDuration = -1f;
    private float _parryCooldownDuration = -1f;
    #endregion

    void Awake()
    {
        if (CompareTag("Player1"))
            _teamIndex = 0;
        else
            _teamIndex = 1;

        playerHability = GetComponent<AbstractHability>();

        if (playerHability == null)
            throw new System.Exception("Player hability not assigned on inspector!");

        playerHability.Initialize(_teamIndex, this);

        playerAnimator = gameObject.AddComponent<PlayerAnimator>();
        playerAnimator.Initialize(_animationSet);

        playerMovement = gameObject.AddComponent<PlayerMovement>();
        playerMovement.Initialize(_playerStats.Speed, _teamIndex);

        playerCombat = gameObject.AddComponent<PlayerCombat>();
        playerCombat.Initialize(_teamIndex);

        attackHoldAction = gameObject.GetComponent<PlayerInput>().actions.FindAction("Attack");

        _specialHabilityCooldownDuration = _playerStats.HabilityCooldownDuration;
        _lightMeleeCooldownDuration = GameManager.Instance.LightMeleeCooldownDuration();
        _heavyMeleeCooldownDuration = GameManager.Instance.HeavyMeleeCooldownDuration();
        _parryCooldownDuration = GameManager.Instance.ParryCooldownDuration();
    }

    #region Player input
    public void Move(InputAction.CallbackContext ctx)
    {
        playerMovement.Move(ctx.ReadValue<Vector2>().x, ctx.ReadValue<Vector2>().y);
    }

    public void Attack(InputAction.CallbackContext ctx)
    {   if (playerCombat.enabled)
        {
            if (ctx.interaction is TapInteraction && isLightMeleeEnabled)
            {
                if (ctx.performed)
                {
                    playerCombat.ExecuteAttack(false);
                    StartCoroutine(LightMeleeCooldown(_lightMeleeCooldownDuration));
                }
            }

            if (ctx.interaction is HoldInteraction && isHeavyMeleeEnabled)
            {
                if (ctx.started)
                    playerCombat.ChargeAttack();

                if (ctx.canceled)
                    playerCombat.InterruptCharge();

                if (ctx.performed)
                {
                    playerCombat.ExecuteAttack(true);
                    StartCoroutine(HeavyMeleeCooldown(_heavyMeleeCooldownDuration));
                }
            }
        }
    }

    public void Hability(InputAction.CallbackContext ctx)
    {
        if (playerCombat.enabled && isHabilityEnabled && ctx.performed)
        {
            playerHability.Activate();
            StartCoroutine(HabilityCooldown(_specialHabilityCooldownDuration));
        }
    }

    public void Parry(InputAction.CallbackContext ctx)
    {
        if (playerCombat.enabled && ctx.performed && isParryEnabled)
        {
            StartCoroutine(playerCombat.Parry());
            StartCoroutine(ParryCooldown(_parryCooldownDuration));
        }
    }

    public void Dash(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && isDashEnabled)
            playerMovement.Dash();
    }

    public void PauseGame()
    {
        if (UINavigationManager.Instance.CurrentScreen.GetName() == ScreenName.Game.ToString())
            UINavigationManager.Instance.ShowScreen(ScreenName.Pause, false);
    }

    #endregion

    #region Cancel player input

    /// <summary>
    /// Cancel ongoing charge attack
    /// </summary>
    public void CancelChargeAttack()
    {
        attackHoldAction.Disable();
        attackHoldAction.Enable();
    }

    public void DisableWorldInteraction()
    {
        CancelChargeAttack();

        playerCombat.enabled = false; // other players cant interact if this one doesnt have combat enabled, and this one cannot perform actions
        isDashEnabled = false;
    }

    public void EnableWorldInteraction()
    {
        playerCombat.enabled = true;
        isDashEnabled = true;
    }
    #endregion

    #region Cooldown input
    IEnumerator ParryCooldown(float duration)
    {
        isParryEnabled = false;
        yield return new WaitForSeconds(duration);
        isParryEnabled = true;
    }

    IEnumerator LightMeleeCooldown(float duration)
    {
        isLightMeleeEnabled = false;
        yield return new WaitForSeconds(duration);
        isLightMeleeEnabled = true;
    }

    IEnumerator HeavyMeleeCooldown(float duration)
    {
        isHeavyMeleeEnabled = false;
        yield return new WaitForSeconds(duration);
        CancelChargeAttack();
        isHeavyMeleeEnabled = true;
    }

    IEnumerator HabilityCooldown(float duration)
    {
        isHabilityEnabled = false;
        yield return new WaitForSeconds(duration);
        isHabilityEnabled = true;
    }

    #endregion

    public int GetTeamIndex() { return _teamIndex; }
}
