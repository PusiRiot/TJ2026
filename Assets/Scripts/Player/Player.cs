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

public class Player : Subject<PlayerCombatEvent>
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
    private AbstractAbility playerAbility;

    // booleans control
    private bool isDashEnabled = true;
    private bool isParryEnabled = true;
    private bool isHeavyMeleeEnabled = true;
    private bool isLightMeleeEnabled = true;
    private bool isAbilityEnabled = true;
    private bool actionsEnabled = true;
    // cooldown durations
    private float _specialAbilityCooldownDuration = -1f;
    private float _lightMeleeCooldownDuration = -1f;
    private float _heavyMeleeCooldownDuration = -1f;
    private float _parryCooldownDuration = -1f;
    #endregion

    void Awake()
    {
        base.AddObserversOnScene();

        if (CompareTag("Player1"))
            _teamIndex = 0;
        else
            _teamIndex = 1;

        playerAbility = GetComponent<AbstractAbility>();

        if (playerAbility == null)
            throw new System.Exception("Player hability not assigned on inspector!");

        playerAbility.Initialize(_teamIndex, this, _playerStats);

        playerAnimator = gameObject.GetComponent<PlayerAnimator>();
        playerAnimator.Initialize(_animationSet);

        playerMovement = gameObject.GetComponent<PlayerMovement>();
        playerMovement.Initialize(_playerStats.Speed, _teamIndex);

        playerCombat = gameObject.GetComponent<PlayerCombat>();
        playerCombat.Initialize(_teamIndex);

        attackHoldAction = gameObject.GetComponent<PlayerInput>().actions.FindAction("Attack");

        _specialAbilityCooldownDuration = _playerStats.AbilityCooldownDuration;
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
    {   if (actionsEnabled)
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

    public void Ability(InputAction.CallbackContext ctx)
    {
        if (actionsEnabled && isAbilityEnabled && ctx.performed)
        {
            playerAbility.Activate();
            isAbilityEnabled = false;
            Notify(PlayerCombatEvent.AbilityDisabled, new int[] { _teamIndex });
        }
    }

    public void StartAbilityCooldown() { 
        StartCoroutine(AbilityCooldown(_specialAbilityCooldownDuration));
    }

    public void Parry(InputAction.CallbackContext ctx)
    {
        if (actionsEnabled && ctx.performed && isParryEnabled)
        {
            StartCoroutine(playerCombat.Parry());
            StartCoroutine(ParryCooldown(_parryCooldownDuration));
        }
    }

    public void Dash(InputAction.CallbackContext ctx)
    {
        if (actionsEnabled && ctx.performed && isDashEnabled)
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

    /// <summary>
    /// To be used when player is dead. Player can move but it cannot interact with world (it cannot attack, use abilities or dash) and world cannot interact with it (combat script disabled)
    /// </summary>
    public void DisableWorldInteraction()
    {
        CancelChargeAttack();

        actionsEnabled = false;
        playerCombat.enabled = false; // other players cant interact if this one doesnt have combat enabled, and this one cannot perform actions
    }

    /// <summary>
    /// To be used when player comes back to live.
    /// </summary>
    public void EnableWorldInteraction()
    {
        actionsEnabled = true;
        playerCombat.enabled = true;
    }

    /// <summary>
    /// Player can't perform actions (attack, dash, use abilities), but world can still interact with it (combat script enabled)
    /// </summary>
    public void DisablePlayerActions()
    {
        actionsEnabled = false;
    }

    /// <summary>
    /// Player can't perform actions (attack, dash, use abilities), but world can still interact with it (combat script enabled)
    /// </summary>
    public void EnablePlayerActions()
    {
        actionsEnabled = true;
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

    IEnumerator AbilityCooldown(float duration)
    {
        Notify(PlayerCombatEvent.AbilityDisabled, new int[] { _teamIndex });
        isAbilityEnabled = false;
        yield return new WaitForSeconds(duration);
        isAbilityEnabled = true;
        Notify(PlayerCombatEvent.AbilityEnabled, new int[] { _teamIndex });
    }

    #endregion
    public int GetTeamIndex() { return _teamIndex; }
}
