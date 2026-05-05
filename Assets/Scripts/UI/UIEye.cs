using UnityEngine;
using UnityEngine.UI;

public class UIPlayerEye : MonoBehaviour, IObserver<PlayerCombatEvent>
{
    [SerializeField] int teamIdx;
    [SerializeField] Image abilityImage;
    [SerializeField] Sprite lifeDrainSprite;
    [SerializeField] Sprite flareSprite;
    
    Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    #region Observer
    public void OnNotify(PlayerCombatEvent evt, object data = null)
    {
        switch (evt)
        {
            case PlayerCombatEvent.AbilityEnabled:
                {
                    int[] intParams = (int[])data;
                    if (intParams[0] == teamIdx)
                        animator.SetTrigger("OpenIdle");
                    break;
                }
            case PlayerCombatEvent.AbilityDisabled:
                {
                    int[] intParams = (int[])data;
                    if (intParams[0] == teamIdx)
                    {
                        animator.SetTrigger("OpenWide");
                        if (intParams[1] == 0)
                            abilityImage.sprite = lifeDrainSprite;
                        else
                            abilityImage.sprite = flareSprite;

                    }
                    break;
                }
            case PlayerCombatEvent.StartAbilityCooldown:
                {
                    int[] intParams = (int[])data;
                    if (intParams[0] == teamIdx)
                        animator.SetTrigger("Close");
                    break;
                }
        }
    }
    #endregion
}
