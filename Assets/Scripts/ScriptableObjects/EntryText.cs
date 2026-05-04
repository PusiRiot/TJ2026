using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public class EntryText
{
    public string Title;
    [TextArea] public string Text;
}

[CreateAssetMenu(fileName = "EntryText", menuName = "Scriptable Objects/EntryText")]
public class EntryTextList : ScriptableObject
{
    public List<EntryText> entries;
}