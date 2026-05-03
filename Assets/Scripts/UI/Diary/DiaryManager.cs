using System.Collections.Generic;
using UnityEngine;

public class DiaryManager : MonoBehaviour
{
    List<DiaryEntry> allDiaries;

    private void Awake()
    {
        GetComponentsInChildren<DiaryEntry>(true, allDiaries); // the order is always the same
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // enable unlocked entries, and set vfx active if unread
        List<int> unlockedEntries = SystemGameDataStorage.Instance.GetUnlockedDiaryEntries();
        List<int> goneThroughEntries = SystemGameDataStorage.Instance.GetGoneThroughDiaryEntries();

        foreach (int idx in unlockedEntries)
        {
            allDiaries[idx].Initialize(goneThroughEntries.Contains(idx), idx);
        }
    }
}
