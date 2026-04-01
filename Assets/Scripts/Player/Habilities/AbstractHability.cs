using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Abstract class representing a player hability.
/// <para>Each hability should be a child class that implements the virtual method to be called from Player input</para>
/// </summary>
public abstract class AbstractHability : MonoBehaviour
{
    protected Player player;
    protected int _teamIndex;

    public void Initialize(int teamIndex, Player player)
    {
        this.player = player;
        this._teamIndex = teamIndex;
    }

    virtual public void Activate()
    {
        throw new System.NotImplementedException("Implement on child object");
    }
}

