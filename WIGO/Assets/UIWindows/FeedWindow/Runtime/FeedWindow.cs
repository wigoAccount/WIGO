using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using WIGO.Core;
using WIGO.Utility;
using Event = WIGO.Core.Event;
using TMPro;
using System.Threading.Tasks;

namespace WIGO.Userinterface
{
    public class FeedWindow : UIWindow
    {
        [SerializeField] UserProfileWithTextElement _userProfileElement;
        [SerializeField] FiltersEventsController _filtersController;
        [SerializeField] UIEventCardElement _cardPrefab;
        [SerializeField] RectTransform _cardsdContent;
        [SerializeField] EndOfPostsController _endOfPostsController;
        [SerializeField] GameObject _loadingLabel;
        [SerializeField] Image _overlay;
        [Space]
        [SerializeField] GameObject _createEventButton;
        [SerializeField] GameObject _myEventButton;
        [SerializeField] TMP_Text _remainingTimeLabel;
        [SerializeField] string _editorVideoPath;
        [SerializeField] TempMessagesContainer _permissionData;

        List<Event> _loadedCards = new List<Event>();
        UIEventCardElement _currentCard;
        CancellationTokenSource _cts;
        Event _acceptedEvent;
        int _currentCardIndex;
        bool _focusLost;

        public override void OnOpen(WindowId previous)
        {
            RefreshFeed();
        }

        public override void OnReopen(WindowId previous, UIWindowModel cachedModel)
        {
            if (previous == WindowId.COMPLAIN_SCREEN)
            {
                return;
            }

            OnOpen(previous);
        }

        public override void OnClose(WindowId next, Action callback = null)
        {
            _currentCard?.Clear();
            _currentCard = null;
            _filtersController.ResetFilters();
            _currentCardIndex = 0;
            _endOfPostsController.Deactivate();
            _loadingLabel.SetActive(false);
            _overlay.gameObject.SetActive(false);
            callback?.Invoke();
        }

        public void OnProfileClick()
        {
            ServiceLocator.Get<UIManager>().Open<ProfileWindow>(WindowId.PROFILE_SCREEN, (window) => window.Setup());
        }

        public void OnChatsClick()
        {
            ServiceLocator.Get<UIManager>().Open<ChatsListWindow>(WindowId.CHATS_LIST_SCREEN);
        }

        public void OnSettingsClick()
        {
            ServiceLocator.Get<UIManager>().Open<SettingsWindow>(WindowId.SETTINGS_SCREEN);
        }

        public void OnAskPermissionsClick()
        {
            PermissionsRequestManager.RequestBothPermissionsAtFirstTime((res, data) =>
            {
                Debug.LogFormat("<color=cyan>Access: {0}; Camera: {1}; Mic: {2}</color>", res ? "OK" : "Denied", data.cameraOn ? "OK" : "Denied", data.microphoneOn ? "OK" : "Denied");
            });
        }

        public void OnCreateEventClick()
        {
            //string saveData = PlayerPrefs.GetString("Permissions");
            //if (string.IsNullOrEmpty(saveData))
            //{
            //    PermissionsRequestManager.RequestBothPermissionsAtFirstTime((res, data) =>
            //    {
            //        string jsonData = JsonReader.Serialize(data);
            //        PlayerPrefs.SetString("Permissions", jsonData);
            //        if (res)
            //        {
            //            CreateEvent();
            //        }
            //    });
            //    return;
            //}

            //bool camAllow = PermissionsRequestManager.HasCameraPermission();
            //bool micAllow = PermissionsRequestManager.HasMicrophonePermission();
            //if (!camAllow || !micAllow)
            //{
            //    CreatePermissionSettingPopup(true);
            //    return;
            //}

            //CreateEvent();
            _acceptedEvent = null;
#if UNITY_EDITOR
            OnRecordComplete(_editorVideoPath);
#elif UNITY_IOS
            MessageIOSHandler.OnPressCameraButton();
#endif
        }

//        void CreateEvent()
//        {
//            _acceptedEvent = null;
//#if UNITY_EDITOR
//            OnRecordComplete(_editorVideoPath);
//#elif UNITY_IOS
//            MessageIOSHandler.OnPressCameraButton();
//#endif
//        }

        public void OnOpenMyEvent()
        {
            ServiceLocator.Get<UIManager>().Open<EventsRequestsWindow>(WindowId.EVENTS_REQUESTS_SCREEN);
        }

        public void OnRefreshFeedClick()
        {
            _endOfPostsController.Deactivate();
            RefreshFeed();
        }

        public void OnTurnOnNotificationsClick()
        {
            ServiceLocator.Get<UIManager>().Open<SettingsWindow>(WindowId.SETTINGS_SCREEN, (window) => window.Setup(new int[] { 0, 0 }));
        }

        public void OnFiltersOffClick()
        {
            _filtersController.ResetFilters();
            RefreshFeed();
        }

        protected override void Awake()
        {
            MessageRouter.onMessageReceive += OnReceiveMessage;
            _filtersController.Initialize(OnApplyFilterCategory);
            var model = ServiceLocator.Get<GameModel>();
            model.OnChangeMyEventTime += OnSetRemainingTime;
            model.OnControlMyEvent += OnControlMyEvent;

            var eventCreated = model.HasMyOwnEvent();
            _createEventButton.SetActive(!eventCreated);
            _myEventButton.SetActive(eventCreated);

            var profile = ServiceLocator.Get<GameModel>().GetMyProfile();
            _userProfileElement.Setup(profile);
        }

        private void OnDestroy()
        {
            MessageRouter.onMessageReceive -= OnReceiveMessage;
            var model = ServiceLocator.Get<GameModel>();
            model.OnChangeMyEventTime -= OnSetRemainingTime;
            model.OnControlMyEvent -= OnControlMyEvent;
        }

        private void OnApplicationPause(bool pause)
        {
            if (!pause && _focusLost)
            {
                _focusLost = false;
                ServiceLocator.Get<UIManager>().GetPopupManager().CloseCurrentPopup();
                RefreshFeed();
            }
        }

        private void OnApplicationFocus(bool focus)
        {
            string status = focus ? "ON" : "OFF";
            Debug.LogFormat("<color=orange>FOCUS {0}</color>", status);
        }

        void OnApplyFilterCategory()
        {
            _cts?.Cancel();
            RefreshFeed();
        }

        async void RefreshFeed()
        {
            _acceptedEvent = null;
            _currentCard?.Clear();
            _currentCard = null;
            _endOfPostsController.Deactivate();
            _loadingLabel.SetActive(true);

            var model = ServiceLocator.Get<GameModel>();
            bool locationSent = await model.SendLocationDataToServer();
            if (locationSent)
            {
                await UpdateFeedCards();
            }
            else
            {
                _loadingLabel.SetActive(false);
                _endOfPostsController.Activate(EndOfPostsType.EmptyFeed);
                ServiceLocator.Get<UIManager>().GetPopupManager().AddErrorNotification(14);
            }
        }

        void CreateNextCard()
        {
            if (_currentCardIndex >= _loadedCards.Count)
            {
                _currentCard = null;
                SetEndOfPosts();
                return;
            }

            var card = _loadedCards[_currentCardIndex];
            _currentCardIndex++;

            _currentCard = Instantiate(_cardPrefab, _cardsdContent);
            _currentCard.Setup(card, OnCardSwipe);
        }

        void OnCardSwipe(Event card, bool accept)
        {
            if (accept)
            {
                //string saveData = PlayerPrefs.GetString("Permissions");
                //if (string.IsNullOrEmpty(saveData))
                //{
                //    PermissionsRequestManager.RequestBothPermissionsAtFirstTime((res, data) =>
                //    {
                //        string jsonData = JsonReader.Serialize(data);
                //        PlayerPrefs.SetString("Permissions", jsonData);
                //        if (res)
                //        {
                //            AcceptEvent(card);
                //        }
                //    });
                //    return;
                //}

                //bool camAllow = PermissionsRequestManager.HasCameraPermission();
                //bool micAllow = PermissionsRequestManager.HasMicrophonePermission();
                //if (!camAllow || !micAllow)
                //{
                //    CreatePermissionSettingPopup(true);
                //    return;
                //}

                //AcceptEvent(card);
                _acceptedEvent = card;
                UIGameColors.SetTransparent(_overlay);
                _overlay.gameObject.SetActive(true);
                _overlay.DOFade(1f, 0.4f).OnComplete(() => StartCoroutine(DelayLaunchRecord()));
                return;
            }

            DeclineCard(card.uid);
            CreateNextCard();
        }

        //void AcceptEvent(Event card)
        //{
        //    _acceptedEvent = card;
        //    UIGameColors.SetTransparent(_overlay);
        //    _overlay.gameObject.SetActive(true);
        //    _overlay.DOFade(1f, 0.4f).OnComplete(() => StartCoroutine(DelayLaunchRecord()));
        //}

        async void DeclineCard(string cardId)
        {
            if (string.IsNullOrEmpty(cardId))
            {
                Debug.LogError("Fail to decline card. It's empty");
                return;
            }

            var model = ServiceLocator.Get<GameModel>();
            string uid = await NetService.TrySendDeclineEvent(cardId, model.GetUserLinks().data.address, model.ShortToken);
            if (string.IsNullOrEmpty(uid))
            {
                Debug.LogErrorFormat("Fail to decline card: {0}. Server error", cardId);
            }
        }

        void CreatePermissionSettingPopup(bool createEvent)
        {
            List<PopupOption> options = new List<PopupOption>
            {
                new PopupOption(_permissionData.GetMessageAt(1), OnOpenAppSettings),
                new PopupOption(_permissionData.GetMessageAt(2), () => OnDeclinePermissions(createEvent), UIGameColors.RED_HEX)
            };
            ServiceLocator.Get<UIManager>().GetPopupManager().AddPopup(_permissionData.GetMessageAt(0), options);
        }

        void OnOpenAppSettings()
        {
            try
            {
                _focusLost = true;
#if UNITY_EDITOR
                return;
#elif UNITY_ANDROID && !UNITY_EDITOR
                using var unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using AndroidJavaObject currentActivityObject = unityClass.GetStatic<AndroidJavaObject>("currentActivity");
                string packageName = currentActivityObject.Call<string>("getPackageName");

                using var uriClass = new AndroidJavaClass("android.net.Uri");
                using AndroidJavaObject uriObject = uriClass.CallStatic<AndroidJavaObject>("fromParts", "package", packageName, null);
                using var intentObject = new AndroidJavaObject("android.content.Intent", "android.settings.APPLICATION_DETAILS_SETTINGS", uriObject);
                intentObject.Call<AndroidJavaObject>("addCategory", "android.intent.category.DEFAULT");
                intentObject.Call<AndroidJavaObject>("setFlags", 0x10000000);
                currentActivityObject.Call("startActivity", intentObject);
#elif UNITY_IOS && !UNITY_EDITOR
                Application.OpenURL("App-Prefs:");
#endif
            }
            catch (Exception ex)
            {
                _focusLost = false;
                Debug.LogException(ex);
            }
        }

        void OnDeclinePermissions(bool createEvent)
        {
            ServiceLocator.Get<UIManager>().GetPopupManager().CloseCurrentPopup();
            if (!createEvent)
            {
                CreateNextCard();
            }
        }

        IEnumerator DelayLaunchRecord()
        {
            _currentCard = null;
            yield return new WaitForEndOfFrame();

#if UNITY_EDITOR
            OnRecordComplete(_editorVideoPath);
#elif UNITY_IOS
            MessageIOSHandler.OnPressCameraButton();
#endif
        }

        void SetEndOfPosts()
        {
            var model = ServiceLocator.Get<GameModel>();

            bool hasMyEvent = model.HasMyOwnEvent();
            if (!hasMyEvent)
            {
                _endOfPostsController.Activate(EndOfPostsType.HaveNoMyEvent);
                return;
            }

            //var notifications = ServiceLocator.Get<GameModel>().GetNotifications();
            //if (!notifications.newEvent)
            //{
            //    _endOfPostsController.Activate(EndOfPostsType.NotificationsOff);
            //    return;
            //}

            if (_filtersController.FiltersApplied())
            {
                _endOfPostsController.Activate(EndOfPostsType.FiltersSearch);
                return;
            }

            _endOfPostsController.Activate(EndOfPostsType.EmptyFeed);
        }

        void OnSetRemainingTime(int seconds)
        {
            int minutes = Mathf.FloorToInt(seconds / 60f);
            _remainingTimeLabel.SetText(minutes.ToString());
        }

        void OnControlMyEvent(bool exist)
        {
            _createEventButton.SetActive(!exist);
            _myEventButton.SetActive(exist);
        }

        void OnReceiveMessage(NativeMessageType type, string message)
        {
            if (!gameObject.activeSelf)
            {
                return;
            }

            switch (type)
            {
                case NativeMessageType.Video:
                    OnRecordComplete(message);
                    break;
                case NativeMessageType.Location:
                case NativeMessageType.MyLocation:
                case NativeMessageType.Other:
                    Debug.LogFormat("<color=red>Unexpected message: {0}</color>", message);
                    break;
                default:
                    break;
            }
        }

        void OnRecordComplete(string videoPath)
        {
			if (string.IsNullOrEmpty(videoPath))
            {
                _overlay.gameObject.SetActive(false);
                RefreshFeed();
                return;
            }
			
            ServiceLocator.Get<UIManager>().Open<VideoPreviewWindow>(WindowId.VIDEO_PREVIEW_SCREEN,
                (window) => window.Setup(videoPath, _acceptedEvent));
        }

        async Task UpdateFeedCards()
        {
            int categoryUid = _filtersController.GetFilterCategory();
            var model = ServiceLocator.Get<GameModel>();
            _cts = new CancellationTokenSource();
            _cts.CancelAfter(8000);

            int[] tags = categoryUid == 0 ? null : new int[] { categoryUid };
            FeedRequest request = new FeedRequest()
            {
                tags = tags,
                gender = 0
            };
            IEnumerable<Event> cards = await NetService.TryGetFeedEvents(request, model.GetUserLinks().data.address, model.ShortToken, _cts.Token);

            if (_cts.IsCancellationRequested)
            {
                return;
            }

            _cts.Dispose();
            _cts = null;
            _loadingLabel.SetActive(false);
            _loadedCards = categoryUid == 0 || cards == null
                ? new List<Event>(cards)
                : new List<Event>(cards.Where(x => x.ContainsTag(categoryUid)));
            _currentCardIndex = 0;

            if (_loadedCards.Count == 0)
            {
                SetEndOfPosts();
                return;
            }

            CreateNextCard();
        }
    }
}
