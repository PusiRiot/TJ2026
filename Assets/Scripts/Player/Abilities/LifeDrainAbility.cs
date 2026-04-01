using System.Collections.Generic;
using UnityEngine;

public class LifeDrainAbility : AbstractAbility
{
    #region Variables
    SpotLight spotLight;

    #endregion

    #region MonoBehaviour 
    private void Awake()
    {
        spotLight = GetComponentInChildren<SpotLight>();
    }
    #endregion

    #region Virtual Methods
    override public void Activate()
    {
        spotLight.ActivateLifeDrain(_playerStats);
    }

    public override void Stop() 
    {
        spotLight.StopLifeDrain();
    }

    #endregion

    #region Private Methods
    #endregion
}

