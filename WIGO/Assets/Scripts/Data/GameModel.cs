using System.Threading.Tasks;
using UnityEngine;
using WIGO;
using WIGO.Core;
using WIGO.Utility;
using Event = WIGO.Core.Event;

[System.Serializable]
public class GameModel
{
    public string ShortToken { get; private set; }
    public string LongToken { get => _ltoken; }

    [SerializeField] string _ltoken;
    [SerializeField] NotificationSettings _notifications;

    ProfileData _myProfile;
    Event _myEvent;
    LinksData _links;
    Location _myLocation;
    float _timer;

    public LinksData GetUserLinks() => _links;
    public NotificationSettings GetNotifications() => _notifications;
    public ProfileData GetMyProfile() => _myProfile;
    public bool HasMyOwnEvent() => _myEvent != null;
    public bool IsMyProfile(string id) => string.Compare(id, _myProfile.uid) == 0;
    public Location GetMyCurrentLocation() => _myLocation;

    public void Initialize()
    {
        MessageRouter.onMessageReceive += OnReceiveMessage;
        _timer = 58f;
    }

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

    public void SetMyEvent(Event card)
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
        ShortToken = data.stoken;
        _links = data.links;
        _myProfile = data.profile;
        SaveData();
        return true;
    }

    public void Tick()
    {
        _timer += Time.unscaledDeltaTime;
        if (_timer >= 60f)
        {
            _timer = 0f;
#if UNITY_EDITOR
            string sum = "14.88,192.66";
            OnReceiveMessage(NativeMessageType.MyLocation, sum);
#elif UNITY_IOS
            MessageIOSHandler.OnGetUserLocation();
#endif
        }
    }

    void OnReceiveMessage(NativeMessageType type, string message)
    {
        switch (type)
        {
            case NativeMessageType.Video:
            case NativeMessageType.Location:
                break;
            case NativeMessageType.MyLocation:
                Debug.LogFormat("<color=yellow>GET MY LOC: {0}</color>", message);
                _myLocation = ParseLocation(message);
                break;
            case NativeMessageType.Other:
                Debug.LogFormat("Message: {0}", message);
                break;
            default:
                break;
        }
    }

    Location ParseLocation(string coordinates)
    {
        Location loc = new Location();
        string[] splited = coordinates.Replace("\"", "").Split(",");
        if (splited.Length > 1)
        {
            var longitude = splited[0];
            var latitude = splited[1];
            loc.latitude = latitude;
            loc.longitude = longitude;
            Debug.LogFormat("<color=yellow>MY LOCATION: Latitude: {0}\r\nLongitude: {1}</color>", latitude, longitude);
        }
        else
            Debug.LogWarningFormat("Can't split coordinates: {0}", coordinates);

        return loc;
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
