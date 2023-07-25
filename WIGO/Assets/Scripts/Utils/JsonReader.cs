using UnityEngine;

public static class JsonReader
{
    public static T Deserialize<T>(string text)
    {
        T items = JsonUtility.FromJson<T>(text);
        return items;
    }

    public static string Serialize<T>(T data)
    {
        string answer = JsonUtility.ToJson(data);
        return answer;
    }
}
