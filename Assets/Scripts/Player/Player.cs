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
    [SerializeField] private PlayerStats playerStats;
    private IMovement playerMovement;
    private ICombat playerCombat;
    int teamIndex = -1;

    void Awake()
    {
        playerMovement = gameObject.GetComponent<IMovement>();
        playerMovement ??= gameObject.AddComponent<BasicMovement>();

        playerCombat = gameObject.GetComponent<ICombat>();
        playerCombat ??= gameObject.AddComponent<StunCombat>();

        if (CompareTag("Player1"))
            teamIndex = 0;
        else
            teamIndex = 1;

        playerMovement.Init(playerStats.Speed, teamIndex);
    }

    #region Player input
    public void Move(InputAction.CallbackContext ctx)
    {
        playerMovement.Move(ctx.ReadValue<Vector2>().x, ctx.ReadValue<Vector2>().y);
    }

    public void Attack(InputAction.CallbackContext ctx)
    {
        // NOTE: is it a bug that charging sparks appear when light attacking?

        if (ctx.interaction is TapInteraction)
        {
            if (ctx.performed)
                playerCombat.ExecuteAttack(false);
        }

        if (ctx.interaction is HoldInteraction)
        {
            if (ctx.started)
                playerCombat.ChargeAttack();

            if (ctx.canceled)
                playerCombat.InterruptCharge();

            if (ctx.performed)
                playerCombat.ExecuteAttack(true);
        }
    }

    public void Parry(InputAction.CallbackContext ctx)
    {
        if (ctx.canceled)
            StartCoroutine(playerCombat.ParryAttack());
    }

    public void Dash(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
            StartCoroutine(playerMovement.Dash(false));
    }

    #endregion
}
