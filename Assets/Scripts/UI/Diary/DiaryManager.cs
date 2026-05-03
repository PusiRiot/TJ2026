using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class DiaryManager : MonoBehaviour
{
    List<DiaryEntry> allDiaries;

    // references to the entry text visualizer
    public TextMeshProUGUI EntryTitleSlot;

    public TextMeshProUGUI EntryTextSlot;

    private void Awake()
    {
        allDiaries = GetComponentsInChildren<DiaryEntry>(true).ToList(); // the order is always the same
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // enable unlocked entries, and set vfx active if unread
        List<int> unlockedEntries = SystemGameDataStorage.Instance.GetUnlockedDiaryEntries();
        List<int> goneThroughEntries = SystemGameDataStorage.Instance.GetGoneThroughDiaryEntries();

        foreach (int idx in unlockedEntries)
        {
            allDiaries[idx].Initialize(this, goneThroughEntries.Contains(idx), idx);
        }
    }
}
