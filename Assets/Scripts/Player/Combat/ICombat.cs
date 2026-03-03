using System.Collections;

/// <summary>
/// Combat methods for an specific attacking style (e.g. stun, turn off light). Attack and defense methods
/// <para>NOTE: Subject to change, i just made it an interface to get a global idea to start</para>
/// <para>NOTE: If a player can have multiple attacks maybe it could be a command pattern (?)</para>
/// <para>NOTE: If different players can have different attacks this need to be separated into attack and defense because they need to defend from all</para>
/// </summary>
interface ICombat
{
    /// <summary>
    /// This executes an attack with the given attack effect. 
    /// <para>The attack effect could be damage, knockback, or any other effect that the attack has.</para> 
    /// <para>The method should see if it collides with another ICombat interface and Hit if it's not protected</para>
    /// </summary>
    void ChargeAttack();

    /// <summary>
    /// This executes an attack with the given attack effect. 
    /// <para>The attack effect could be damage, knockback, or any other effect that the attack has.</para> 
    /// <para>The method should see if it collides with another ICombat interface and Hit if it's not protected</para>
    /// </summary>
    void ExecuteAttack(bool isHeavyAttack);

    /// <summary>
    /// While parry executes the player is protected from attacks
    /// <para>NOTE: This could have a cooldown or something</para>
    /// </summary>
    IEnumerator ParryAttack();

    /// <summary>
    /// When the player is hit by an attack, this method should be called with the attack effect. 
    /// <para>It should execute the defense according to the given attack effect (e.g. reduce health, apply knockback, etc.)</para>
    /// </summary>
    void ReceiveAttack(float? attackEffect = null, bool unableToParry = false);
}
