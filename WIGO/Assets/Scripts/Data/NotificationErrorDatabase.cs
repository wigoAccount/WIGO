using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Content/Storage/Errors database")]
public class NotificationErrorDatabase : ScriptableObject
{
    [SerializeField] NotificationErrorData[] _data;
    [SerializeField] string _undefinedError;

    public string GetErrorMessageWithId(int id)
    {
        var error = Array.Find(_data, x => x.EqualsId(id));
        return error == null ? _undefinedError : error.GetMessage();
    }
}

[System.Serializable]
public class NotificationErrorData
{
    [SerializeField] int _id;
    [SerializeField] string _message;

    public bool EqualsId(int id) => _id == id;
    public string GetMessage() => _message;
}
