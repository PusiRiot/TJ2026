using UnityEngine;

[CreateAssetMenu(fileName = "PlayerAnimationsSet", menuName = "Scriptable Objects/PlayerAnimationsSet")]
public class PlayerAnimationsSet : ScriptableObject
{
    public AnimationClip Idle;
    public AnimationClip Walk;
    public AnimationClip Melee;
    public AnimationClip Charge;
    public AnimationClip Stun;
    public AnimationClip StunAttack;
    public AnimationClip Parry;
    public AnimationClip ParrySuccess;
}
