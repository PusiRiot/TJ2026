using UnityEngine;

public class Music : MonoBehaviour
{
    [SerializeField] private AudioClip menuMusic;

    private void Start()
    {
        AudioManager.Instance.PlayMusic(menuMusic, true, 1.5f);
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
