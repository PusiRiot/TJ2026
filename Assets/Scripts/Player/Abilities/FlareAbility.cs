using System.Collections.Generic;
using UnityEngine;

public class FlareAbility : AbstractAbility
{
    #region Variables
    [SerializeField] private GameObject flarePrefab;
    [SerializeField] private Transform flareSpawn;
    private GameObject flareInstance;
    #endregion

    #region Virtual Methods
    override public void Activate()
    {
        flareInstance = Instantiate(flarePrefab, flareSpawn.position, flareSpawn.rotation);
        flareInstance.GetComponent<FlareProjectile>().Initialize(_teamIndex, _playerStats);
        flareInstance.GetComponent<AbstractLight>().SetTeam(_teamIndex);
    }

    public override void Stop()
    {
        Destroy(flareInstance);
    }

    #endregion

    #region Private Methods
    #endregion
}

