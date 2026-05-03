using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.Collections.Generic;

[Serializable]
public class GameData
{
    public List<int> UnlockedDiaryEntries; // there are N diary entries, this array stores the number of the ones unlocked, from 0 to N-1
    public List<int> GoneThroughDiaryEntries; // this stores the unlocked diary entries that have already been read by the player
}

public class SystemGameDataStorage : MonoBehaviour
{
    private string savePath;
    private GameData gameData;

    #region Singleton
    public static SystemGameDataStorage Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);

        LoadGameData();
    }
    #endregion

    #region Save/Load system
    void SaveGameData()
    {
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(savePath, FileMode.Create);
        formatter.Serialize(stream, gameData);
        stream.Close();
    }

    void LoadGameData()
    {
        savePath = Application.persistentDataPath + "/savefile.dat";

        if (File.Exists(savePath))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(savePath, FileMode.Open);

            gameData = formatter.Deserialize(stream) as GameData;
            stream.Close();
        }
        else
        {
            gameData = new GameData();
            gameData.UnlockedDiaryEntries = new List<int>();
            gameData.GoneThroughDiaryEntries = new List<int>();
        }
    }
    #endregion

    public bool UnlockDiaryEntries()
    {
        int newEntryId = GetRandomLockedDiaryEntry();

        if (newEntryId < 0) return false;

        gameData.UnlockedDiaryEntries.Add(newEntryId);

        SaveGameData();

        return true;
    }

    public List<int> GetUnlockedDiaryEntries()
    {
        return gameData.UnlockedDiaryEntries;
    }

    public List<int> GetGoneThroughDiaryEntries()
    {
        return gameData.GoneThroughDiaryEntries;
    }

    public void GoThroughDiaryEntry(int idx)
    {
        gameData.GoneThroughDiaryEntries.Add(idx);
    }

    int GetRandomLockedDiaryEntry()
    {
        List<int> availableIds = new List<int>();

        // find ids not unlocked
        for (int i = 0; i <= GameStatsAccess.Instance.GetMaxDiaryEntriesNumber(); i++)
        {
            if (!gameData.UnlockedDiaryEntries.Contains(i))
            {
                availableIds.Add(i);
            }
        }

        // check if there are left to unlock
        if (availableIds.Count == 0)
        {
            return -1;
        }

        // pick random id
        int randomIndex = UnityEngine.Random.Range(0, availableIds.Count);
        return availableIds[randomIndex];
    }
}