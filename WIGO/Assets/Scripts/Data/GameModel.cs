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
    public Action OnUpdateProfile;
    public Action<bool, int> OnGetUpdates;

    public string ShortToken { get; private set; }
    public string LongToken { get => _ltoken; }

    [SerializeField] string _ltoken;
    [SerializeField] NotificationSettings _notifications;

    ProfileData _myProfile;
    Event _myEvent;
    LinksData _links;
    Location _myLocation;
    IEnumerable<GeneralData> _availableTags;
    List<Request> _myRequests = new();
    PushNotificationsController _pushNotificationsController;
    (int newEvents, int newRequests) _updates = (0, 0);
    float _timer;
    float _myEventTimer;
    float _updateTimer;
    bool _login;

    public LinksData GetUserLinks() => _links;
    public NotificationSettings GetNotifications() => _notifications;
    public ProfileData GetMyProfile() => _myProfile;
    public bool HasMyOwnEvent() => _myEvent != null;
    public string GetMyEventId() => _myEvent?.uid;
    public string GetMyEventAddress() => _myEvent?.address;
    public string GetLocationFromMyEvent() => _myEvent?.location.ToCorrectString();
    public bool IsMyProfile(string id) => string.Compare(id, _myProfile.uid) == 0;
    public string GetUserId() => _myProfile?.uid;
    public Location GetMyCurrentLocation() => _myLocation;
    public IEnumerable<GeneralData> GetAvailableTags() => _availableTags;
    public IEnumerable<Request> GetAllMyRequests() => _myRequests;
    public IEnumerable<Request> GetCachedRequestsToMyEvent() => _myEvent?.requests;
    public int GetUnreadEventsCount(bool isEvent) => isEvent ? _updates.newEvents : _updates.newRequests;

    public async Task<IEnumerable<Request>> GetRequestsToMyEvent()
    {
        await UpdateMyEvent();
        //_myEvent = await NetService.TryGetMyEvent(_links.data.address, ShortToken);
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
    }

    public void SaveProfile(ProfileData profile)
    {
        _myProfile = profile;
        OnUpdateProfile?.Invoke();
    }

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
        _myLocation = new Location();
        _timer = 0f;
        _myEventTimer = 0f;
        _updateTimer = 0f;
        _myRequests.Clear();
        _updates = (0, 0);
        StopPushNotifications();
        OnControlMyEvent?.Invoke(false);
        PlayerPrefs.DeleteKey("SaveData");
    }

    public void FullClear()
    {
        Clear();
        OnChangeMyEventTime = null;
        OnControlMyEvent = null;
        OnUpdateAvatar = null;
        OnUpdateProfile = null;
        OnGetUpdates = null;
    }

    public async void FinishRegister()
    {
        SaveData();
        var res = await NetService.RequestGlobal(_links.data.address, ShortToken);
        _availableTags = res?.tags;
        //await InitPusNotifications();
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
        await UpdateMyRequests();
        await UpdateEventsAndRequests();

        var res = await NetService.RequestGlobal(_links.data.address, ShortToken);
        _availableTags = res?.tags;

        //await InitPusNotifications();
        SaveData();
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

    public async Task UpdateMyRequests()
    {
        var cts = new CancellationTokenSource();
        cts.CancelAfter(8000);

        var myRequests = await NetService.TryGetMyRequests(_links.data.address, ShortToken, cts.Token);
        if (!cts.IsCancellationRequested)
        {
            _myRequests = myRequests == null ? new List<Request>() : new List<Request>(myRequests);
        }
        cts.Dispose();
    }

    public void DecreaseUpdatesCounter(bool isEvent)
    {
        if (isEvent)
        {
            int events = Mathf.Clamp(_updates.newEvents - 1, 0, int.MaxValue);
            _updates.newEvents = events;
        }
        else
        {
            int requests = Mathf.Clamp(_updates.newRequests - 1, 0, int.MaxValue);
            _updates.newRequests = requests;
        }
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

        CheckLocationTimer();
        CheckUpdateTimer();
        CheckMyEventTimer();
    }

    public async Task<bool> SendLocationDataToServer(CancellationToken token = default)
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
            bool locationSent = await NetService.TrySendLocation(_myLocation, _links.data.address, ShortToken, token);
            return locationSent;
        }
        else
        {
            Debug.LogWarning("Fail get location from plugin");
            return false;
        }
    }

    void SaveData()
    {
        string saveData = JsonReader.Serialize(this);
        PlayerPrefs.SetString("SaveData", saveData);
    }

    async void CheckLocationTimer()
    {
        _timer += Time.unscaledDeltaTime;
        if (_timer >= GameConsts.SEND_LOCATION_PERIOD)
        {
            _timer = 0f;
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(5000);
            bool locationSent = await SendLocationDataToServer(cts.Token);

            if (cts.IsCancellationRequested)
            {
                return;
            }

            cts.Dispose();
            _timer = locationSent ? 0f : GameConsts.SEND_LOCATION_PERIOD - 5f;
        }
    }

    async void CheckUpdateTimer()
    {
        _updateTimer += Time.unscaledDeltaTime;
        if (_updateTimer >= GameConsts.ASK_UPDATES_PERIOD)
        {
            await UpdateEventsAndRequests();
        }
    }

    async Task UpdateEventsAndRequests()
    {
        _updateTimer = 0f;
        CancellationTokenSource cts = new CancellationTokenSource();
        cts.CancelAfter(5000);
        var updateData = await NetService.TryGetUpdateData(_links.data.address, ShortToken, cts.Token);

        if (cts.IsCancellationRequested)
        {
            return;
        }

        cts.Dispose();
        if (updateData.IsEmpty())
        {
            _updates = (updateData.events.Length, updateData.requests.Length);
            return;
        }

        await CheckEventsUpdate(updateData.events, true);
        await CheckRequestsUpdate(updateData.requests, true);

        _updates = (updateData.events.Length, updateData.requests.Length);
    }

    public async Task UpdateAndLoadEventsAndRequests()
    {
        _updateTimer = 0f;
        CancellationTokenSource cts = new CancellationTokenSource();
        cts.CancelAfter(5000);
        var updateData = await NetService.TryGetUpdateData(_links.data.address, ShortToken, cts.Token);

        if (cts.IsCancellationRequested)
        {
            return;
        }

        cts.Dispose();
        
        if (updateData.IsEmpty())
        {
            await UpdateMyRequests();
            await UpdateMyEvent();
            _updates = (updateData.events.Length, updateData.requests.Length);
            return;
        }

        await CheckEventsUpdate(updateData.events, false);
        await CheckRequestsUpdate(updateData.requests, false);

        _updates = (updateData.events.Length, updateData.requests.Length);
    }

    async Task CheckEventsUpdate(string[] events, bool withNotify)
    {
        if (events == null || events.Length == 0)
        {
            return;
        }

        if (_myRequests.Count() > 0)
        {
            Event aproved = null;
            foreach (var aprovedEvent in events)
            {
                var checkEvent = _myRequests.FirstOrDefault(x => string.Compare(x.@event.uid, aprovedEvent) == 0);
                if (checkEvent != null)
                {
                    aproved = checkEvent.@event;
                    break;
                }
            }

            if (aproved == null)
            {
                Debug.LogErrorFormat("Can't find request to event with ID: {0}", events[0]);
                return;
            }

            if (events.Length != _updates.newEvents)
            {
                await UpdateMyRequests();
                await UpdateMyEvent();

                if (withNotify)
                    OnGetUpdates?.Invoke(true, events.Length);
                Debug.LogFormat("Get updates for aproved EVENT with id: {0}", aproved.uid);
            }
        }
        else
        {
            await UpdateMyRequests();
            await UpdateMyEvent();
            if (events.Length != _myRequests.Count)
            {
                Debug.LogErrorFormat("Loaded requests are different from aproved event with id: {0}", events[0]);
                return;
            }

            if (withNotify)
                OnGetUpdates?.Invoke(true, events.Length);
            Debug.LogFormat("Get updates for aproved EVENT with id: {0}", events[0]);
        }
    }

    async Task CheckRequestsUpdate(string[] requests, bool withNotify)
    {
        if (requests == null || requests.Length == 0)
        {
            return;
        }

        if (_myEvent != null && _myEvent.requests != null && _myEvent.requests.Length > 0)
        {
            int updatesCounter = 0;
            bool updatesExist = false;
            foreach (var newRequest in requests)
            {
                if (!Array.Exists(_myEvent.requests, x => string.Compare(x.uid, newRequest) == 0))
                {
                    updatesExist = true;
                    updatesCounter++;
                }
            }

            if (_myEvent.requests.Length != requests.Length || updatesExist)
            {
                await UpdateMyEvent();
                if (withNotify)
                    OnGetUpdates?.Invoke(false, updatesCounter);
                Debug.LogFormat("Get updates for REQUESTS to my Event: {0} new requests", updatesCounter);
            }
        }
        else
        {
            await UpdateMyEvent();
            if (withNotify)
                OnGetUpdates?.Invoke(false, requests.Length);
            Debug.LogFormat("Get updates for REQUESTS to my Event: {0} new requests", requests.Length);
        }
    }

    async Task InitPusNotifications()
    {
#if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID)
        _pushNotificationsController = new();
        await _pushNotificationsController.Init();
#endif
    }

    void StopPushNotifications()
    {
        _pushNotificationsController?.Dispose();
        _pushNotificationsController = null;
    }

    void CheckMyEventTimer()
    {
        if (_myEvent != null)
        {
            _myEventTimer += Time.unscaledDeltaTime;
            if (_myEventTimer >= 1f)
            {
                _myEventTimer -= 1f;
                _myEvent.waiting = Mathf.Clamp(_myEvent.waiting - 1, 0, int.MaxValue);
                if (_myEvent.waiting <= 0)
                    return;

                OnChangeMyEventTime?.Invoke(_myEvent.waiting);
            }
        }
    }
}

[Serializable]
public struct NotificationSettings
{
    public bool responses;
    public bool newMessages;
    public bool newEvent;
    public bool expireEvent;
    public bool areYouOK;
    public bool estimate;
}
