using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "Scriptable Objects/PlayerStats")]
public class PlayerStats : ScriptableObject
{
    [Header("General")]
    public float Speed = 10f;
    public float AbilityCooldownDuration = 10f;

    [Header("Life Drain - Dr. Hives")]
    public int LifeDrainNumPulses = 3;
    public float LifeDrainPulseCadence = 2f;
    public int LifeDrainPulseDamage = 1;
    public int LifeDrainPulseHeal = 1;
}
