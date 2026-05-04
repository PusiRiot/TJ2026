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
        flareInstance = Instantiate(flarePrefab, flareSpawn.position, Quaternion.Euler(0.0f, transform.rotation.eulerAngles.y, 0.0f));
        flareInstance.GetComponent<FlareProjectile>().Initialize(_teamIndex, _playerStats);
        flareInstance.GetComponentInChildren<AbstractLight>().SetTeam(_teamIndex);
    }

    public override void Stop()
    {
        flareInstance.GetComponent<FlareProjectile>().Eliminate();
    }

    #endregion

    #region Private Methods
    #endregion
}

