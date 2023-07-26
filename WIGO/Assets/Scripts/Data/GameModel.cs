using System.Threading.Tasks;
using UnityEngine;
using WIGO;
using WIGO.Core;

[System.Serializable]
public class GameModel
{
    public string ShortToken { get; private set; }
    public string LongToken { get => _ltoken; }

    [SerializeField] string _ltoken;
    [SerializeField] NotificationSettings _notifications;

    ProfileData _myProfile;
    EventCard _myEvent;
    LinksData _links;

    public LinksData GetUserLinks() => _links;
    public NotificationSettings GetNotifications() => _notifications;
    public ProfileData GetMyProfile() => _myProfile;
    public bool HasMyOwnEvent() => _myEvent != null;
    public bool IsMyProfile(string id) => string.Compare(id, _myProfile.uid) == 0;

    public void SaveTokens(string ltoken, string stoken, LinksData links)
    {
        _ltoken = ltoken;
        ShortToken = stoken;
        _links = links;
        SaveData();
    }

    public void SaveProfile(ProfileData profile) => _myProfile = profile;

    public void SaveNotifications(NotificationSettings settings)
    {
        _notifications = settings;
        SaveData();
    }

    public void SetMyEvent(EventCard card)
    {
        _myEvent = card;
    }

    public void Clear()
    {
        _ltoken = string.Empty;
        ShortToken = string.Empty;
        _myProfile = null;
        _myEvent = null;
        _links = new LinksData();
        PlayerPrefs.DeleteKey("SaveData");
    }

    public async Task<bool> TryLogin()
    {
        if (string.IsNullOrEmpty(_ltoken))
        {
            return false;
        }

        var data = await NetService.TryLogin(_ltoken);
        if (string.IsNullOrEmpty(data.ltoken) || string.IsNullOrEmpty(data.stoken) || data.profile == null)
        {
            return false;
        }

        _ltoken = data.ltoken;
        _myProfile = data.profile;
        SaveData();
        return true;
    }

    void SaveData()
    {
        string saveData = JsonReader.Serialize(this);
        PlayerPrefs.SetString("SaveData", saveData);
    }
}

[System.Serializable]
public struct NotificationSettings
{
    public bool responses;
    public bool newMessages;
    public bool newEvent;
    public bool expireEvent;
    public bool areYouOK;
    public bool estimate;
}
