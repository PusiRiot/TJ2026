using System.Collections;
using UnityEngine;

/// <summary>
/// Abstract class representing a player hability.
/// <para>Each hability should be a child class that implements the virtual method to be called from Player input</para>
/// </summary>
public abstract class AbstractAbility : MonoBehaviour, IObserver<PlayerCombatEvent>
{
    protected Player player;
    protected PlayerStats _playerStats;
    protected int _teamIndex;

    public void Initialize(int teamIndex, Player player, PlayerStats playerStats)
    {
        this.player = player;
        this._teamIndex = teamIndex;
        this._playerStats = playerStats;
    }

    virtual public void Activate()
    {
        throw new System.NotImplementedException("Implement on child object");
        //Activates the ability, should be called from the Player class when the player presses the corresponding button.
        //The child class should either implement the specific ability themselves or call another script where the abiliy is implemented
        //(see LifeDrain)
    }

    virtual public void Stop()
    {
        throw new System.NotImplementedException("Implement on child object");
        // Stops the ability, this should be called from other classes that want to interrupt the ability
        // (e.g. player gets stunned while using it)
        // All the deactivation logic common to all abilities (disabling the ability so it cant be used while the cooldown is active,
        // UI feedback, etc) should NOT be implemented here, but in the Cooldown Coroutine.
        // All the specific deactivation logic of each ability (deactivating visuals, etc) should be implemented here by the child class
        // IF AND ONLY IF the implementation of the ability is also done in the child class
        // Otherwise, the deactivation logic should be implemented in the script where the ability is implemented and this method should
        // just be used as a way to call that logic from other classes (see LifeDrain)
    }

    protected void StartCooldown()
    {
        player.StartAbilityCooldown();
    }

    #region IObserver
    public void OnNotify(PlayerCombatEvent evt, object data = null)
    {
        if (evt == PlayerCombatEvent.StartAbilityCooldown)
        {
            int[] intParams = (int[])data;

            if (intParams[0] == _teamIndex)
            {
                StartCooldown();
            }
        }
    }
    #endregion
}

