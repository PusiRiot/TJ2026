using UnityEngine;
/// <summary>
/// Physical book on shelf, that has a reference to the idx of the text
/// </summary>
public class BookOnShelf : MonoBehaviour
{
    BookshelfManager bookshelfManager;

    int entryIdx;
    ParticleSystem newReadVFX;
    bool goneTrough = false;

    void Awake()
    {
        newReadVFX = GetComponentInChildren<ParticleSystem>();

        // start disabled
        gameObject.SetActive(false);
    }
    private void OnEnable()
    {
        if (!goneTrough)
        {
            newReadVFX.Clear();
            newReadVFX.Simulate(0, true, true);

            newReadVFX.Play();

            Debug.Log($"VFX Play called on {gameObject.name}. IsPlaying: {newReadVFX.isPlaying}");
        }
    }

    private void OnDisable()
    {
        if (!goneTrough)
            newReadVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    public void Initialize(BookshelfManager bookshelfManager, bool goneTrough, int entryIdx)
    {
        this.bookshelfManager = bookshelfManager;
        gameObject.SetActive(true);
        this.goneTrough = goneTrough;
        this.entryIdx = entryIdx;
    }

    public void ReadDiary()
    {
        bookshelfManager.ChangeEntryText(entryIdx);

        UINavigationManager.Instance.ShowScreen(ScreenName.DiaryEntry, false);

        if (!goneTrough)
        {
            goneTrough = true;
            newReadVFX.Stop();
            SystemGameDataStorage.Instance.GoThroughDiaryEntry(entryIdx);
        }
    }
}
