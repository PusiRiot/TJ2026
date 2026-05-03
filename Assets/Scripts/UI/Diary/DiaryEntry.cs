using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class DiaryEntry : MonoBehaviour
{
    [SerializeField] TextField entryTitle;
    [SerializeField] TextField entryText;

    TextMeshPro titleSlot;
    TextMeshPro textSlot;
    int diaryEntryIdx;
    ParticleSystem newReadVFX;
    bool goneTrough = false;

    void Awake()
    {
        newReadVFX = GetComponent<ParticleSystem>();

        titleSlot = GameObject.FindGameObjectWithTag("EntryTitle").GetComponent<TextMeshPro>();
        textSlot = GameObject.FindGameObjectWithTag("EntryText").GetComponent<TextMeshPro>();


        // start disabled
        gameObject.SetActive(false);
    }

    public void Initialize(bool goneTrough, int idx)
    {
        gameObject.SetActive(true);
        this.goneTrough = goneTrough;
        diaryEntryIdx = idx;

        if (goneTrough)
            newReadVFX.Play();
    }


    public void ReadDiary()
    {
        titleSlot.text = entryTitle.text;
        textSlot.text = entryText.text;
        UINavigationManager.Instance.ShowScreen(ScreenName.DiaryEntry, false);

        if (!goneTrough)
        {
            goneTrough = true;
            newReadVFX.Stop();
            SystemGameDataStorage.Instance.GoThroughDiaryEntry(diaryEntryIdx);
        }
    }


}
