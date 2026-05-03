using UnityEngine;

public class DiaryEntry : MonoBehaviour
{
    [SerializeField][TextArea] string entryTitle;
    [SerializeField][TextArea] string entryText;
    DiaryManager diaryManager;

    int diaryEntryIdx;
    ParticleSystem newReadVFX;
    bool goneTrough = false;

    void Awake()
    {
        newReadVFX = GetComponentInChildren<ParticleSystem>();

        // start disabled
        gameObject.SetActive(false);
    }

    public void Initialize(DiaryManager diaryManager, bool goneTrough, int idx)
    {
        this.diaryManager = diaryManager;
        gameObject.SetActive(true);
        this.goneTrough = goneTrough;
        diaryEntryIdx = idx;

        if (!goneTrough)
            newReadVFX.Play();
    }


    public void ReadDiary()
    {
        diaryManager.EntryTitleSlot.text = entryTitle;
        diaryManager.EntryTextSlot.text = entryText;
        UINavigationManager.Instance.ShowScreen(ScreenName.DiaryEntry, false);

        if (!goneTrough)
        {
            goneTrough = true;
            newReadVFX.Stop();
            SystemGameDataStorage.Instance.GoThroughDiaryEntry(diaryEntryIdx);
        }
    }


}
