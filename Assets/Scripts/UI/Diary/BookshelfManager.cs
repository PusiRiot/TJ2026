using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class BookshelfManager : MonoBehaviour
{
    List<BookOnShelf> allBooks;
    [SerializeField] EntryTextList allEntriesText;
    // references to the entry text visualizer
    [SerializeField] TextMeshProUGUI entryTitleSlot;
    [SerializeField] TextMeshProUGUI entryTextSlot;
    [SerializeField] GameObject firstTime;

    private void Awake()
    {
        allBooks = GetComponentsInChildren<BookOnShelf>(true).ToList(); // the order is always the same
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // enable unlocked entries, and set vfx active if unread
        List<int> unlockedEntries = SystemGameDataStorage.Instance.GetUnlockedDiaryEntries();
        List<int> goneThroughEntries = SystemGameDataStorage.Instance.GetGoneThroughDiaryEntries();

        if (unlockedEntries.Count > 0)
        {
            firstTime.SetActive(false);
            for (int i = 0; i < unlockedEntries.Count; i++)
            {
                int entryidx = unlockedEntries[i];
                allBooks[i].Initialize(this, goneThroughEntries.Contains(entryidx), entryidx);
            }
        }
        else
        {
            firstTime.SetActive(true);
        }
    }

    public void ChangeEntryText(int entryIdx)
    {
        entryTitleSlot.text = allEntriesText.entries[entryIdx].Title;
        entryTextSlot.text = allEntriesText.entries[entryIdx].Text;
    }
}
