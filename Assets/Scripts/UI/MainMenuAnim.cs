using UnityEngine;


/// <summary>
/// Animation that plays on the main menu on awake
/// </summary>
public class MainMenuAnim : MonoBehaviour
{
    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
        Debug.Log("trigger anim");
        anim.SetTrigger("FirstTime");
    }
}
