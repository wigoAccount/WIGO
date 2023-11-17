using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using WIGO;
using WIGO.Core;
using WIGO.Utility;
using System.Linq;
using Event = WIGO.Core.Event;
using System.Threading;
using System;

[Serializable]
public class GameModel
{
    public Action<int> OnChangeMyEventTime;
    public Action<bool> OnControlMyEvent;
    public Action<Texture2D> OnUpdateAvatar;
    public string ShortToken { get; private set; }
    public string LongToken { get => _ltoken; }

    [SerializeField] string _ltoken;
    [SerializeField] NotificationSettings _notifications;

    ProfileData _myProfile;
    Event _myEvent;
    LinksData _links;
    Location _myLocation;
    IEnumerable<GeneralData> _availableTags;
    float _timer;
    float _myEventTimer;
    bool _login;

    public LinksData GetUserLinks() => _links;
    public NotificationSettings GetNotifications() => _notifications;
    public ProfileData GetMyProfile() => _myProfile;
    public bool HasMyOwnEvent() => _myEvent != null;
    public string GetMyEventId() => _myEvent == null ? null : _myEvent.uid;
    public bool IsMyProfile(string id) => string.Compare(id, _myProfile.uid) == 0;
    public string GetUserId() => _myProfile == null ? null : _myProfile.uid;
    public Location GetMyCurrentLocation() => _myLocation;
    public IEnumerable<GeneralData> GetAvailableTags() => _availableTags;

    public async Task<IEnumerable<Request>> GetRequestsToMyEvent()
    {
        _myEvent = await NetService.TryGetMyEvent(_links.data.address, ShortToken);
        _myEventTimer = 0f;
        OnControlMyEvent?.Invoke(_myEvent != null);
        if (_myEvent == null)
        {
            return null;
        }

        return _myEvent.requests;
    }

    public string GetCategoryNameWithIndex(int uid)
    {
        return _availableTags.FirstOrDefault(x => x.uid == uid).name;
    }

    public void SaveTokens(string ltoken, string stoken, LinksData links)
    {
        _ltoken = ltoken;
        ShortToken = stoken;
        _links = links;
        //SaveData();
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
        _myEventTimer = 0f;
        OnControlMyEvent?.Invoke(_myEvent != null);
    }

    public void Clear()
    {
        _login = false;
        _ltoken = string.Empty;
        ShortToken = string.Empty;
        _myProfile = null;
        _myEvent = null;
        _links = new LinksData();
        OnControlMyEvent?.Invoke(false);
        PlayerPrefs.DeleteKey("SaveData");
    }

    public async void FinishRegister()
    {
        SaveData();
        var res = await NetService.RequestGlobal(_links.data.address, ShortToken);
        _availableTags = res?.tags;
        _timer = 55f;
        _login = true;
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

        await UpdateMyEvent();

        var res = await NetService.RequestGlobal(_links.data.address, ShortToken);
        _availableTags = res?.tags;

        SaveData();
        _timer = 55f;
        _login = true;
        return true;
    }

    public async Task UpdateMyEvent()
    {
        var cts = new CancellationTokenSource();
        cts.CancelAfter(8000);
        var myEvent = await NetService.TryGetMyEvent(_links.data.address, ShortToken, cts.Token);
        if (!cts.IsCancellationRequested)
        {
            _myEvent = myEvent;
            _myEventTimer = 0f;
            OnControlMyEvent?.Invoke(myEvent != null);
        }
        cts.Dispose();
    }

    public void UpdateMyAvatar(Texture2D texture)
    {
        OnUpdateAvatar?.Invoke(texture);
    }

    public void Tick()
    {
        if (!_login)
        {
            return;
        }

        _timer += Time.unscaledDeltaTime;
        if (_timer >= 60f)
        {
            _timer = 0f;
            SendLocationDataToServer();
        }

        if (_myEvent != null)
        {
            _myEventTimer += Time.unscaledDeltaTime;
            if (_myEventTimer >= 1f)
            {
                _myEventTimer -= 1f;
                _myEvent.waiting = Mathf.Clamp(_myEvent.waiting - 1, 0, int.MaxValue);
                if (_myEvent.waiting <= 0)
                {
                    //_myEvent = null;
                    //OnControlMyEvent?.Invoke(false);
                    return;
                }

                OnChangeMyEventTime?.Invoke(_myEvent.waiting);
            }
        }
    }

    async void SendLocationDataToServer()
    {
        bool gotLocation = false;

#if UNITY_EDITOR
        gotLocation = true;
        _myLocation = new Location()
        {
            latitude = "55.762021191659485",
            longitude = "37.63407669596055"
        };
#elif UNITY_IOS
        gotLocation = MessageIOSHandler.TryGetMyLocation(out Location location);
        _myLocation = location;
#endif

        if (gotLocation)
        {
            await NetService.TrySendLocation(_myLocation, _links.data.address, ShortToken);
        }
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
