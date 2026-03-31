using UnityEngine;

[CreateAssetMenu(fileName = "PlayerAnimationsSet", menuName = "Scriptable Objects/PlayerAnimationsSet")]
public class PlayerAnimationsSet : ScriptableObject
{
    public AnimationClip Idle;
    public AnimationClip Walk;
    public AnimationClip HeavyMelee;
    public AnimationClip LightMelee;
    public AnimationClip Charge;
    public AnimationClip Stun;
    public AnimationClip StunAttack;
    public AnimationClip Parry;
    public AnimationClip ParrySuccess;
}
