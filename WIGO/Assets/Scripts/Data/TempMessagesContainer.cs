using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Content/Storage/Messages Storage")]
public class TempMessagesContainer : ScriptableObject
{
    [TextArea]
    [SerializeField] List<string> _messages;

    public string GetRandomMessage()
    {
        int rnd = Random.Range(0, _messages.Count);
        return _messages[rnd];
    }

    public string GetMessageAt(int index)
    {
        return index >= 0 && index < _messages.Count ? _messages[index] : "NULL";
    }
}
