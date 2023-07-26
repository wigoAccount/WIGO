using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using WIGO.Core;
using WIGO.Utility;

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
        [SerializeField] EventCard[] _testCards;
        [SerializeField] bool _eventCreated;
        [SerializeField] string _editorVideoPath;

        List<EventCard> _loadedCards = new List<EventCard>();
        UIEventCardElement _currentCard;
        CancellationTokenSource _cts;
        int _currentCardIndex;
        bool _isResponse;
        bool _waitForLocation;

        public override void OnOpen(WindowId previous)
        {
            var profile = ServiceLocator.Get<GameModel>().GetMyProfile();
            _userProfileElement.Setup(profile);
            RefreshFeed();
        }

        public override void OnClose(WindowId next, Action callback = null)
        {
            _waitForLocation = false;
            _currentCard?.Clear();
            _currentCard = null;
            _filtersController.ResetFilters();
            _currentCardIndex = 0;
            _endOfPostsController.Deactivate();
            _loadingLabel.SetActive(false);
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

        public void OnCreateEventClick()
        {
            if (_eventCreated)
            {
                ServiceLocator.Get<UIManager>().Open<EventsRequestsWindow>(WindowId.EVENTS_REQUESTS_SCREEN);
            }
            else
            {
                //ServiceLocator.Get<UIManager>().Open<ResponseInfoWindow>(WindowId.RESPONSE_INFO_SCREEN, (window) => window.Setup());
                _isResponse = false;
#if UNITY_EDITOR
                string path = System.IO.Path.Combine(Application.streamingAssetsPath, _editorVideoPath);
                OnRecordComplete(path);
#elif UNITY_IOS
                MessageIOSHandler.OnPressCameraButton();
#endif
            }
        }

        public void OnRefreshFeedClick()
        {
            _waitForLocation = false;
            _endOfPostsController.Deactivate();
            RefreshFeed();
        }

        public void OnTurnOnNotificationsClick()
        {
            ServiceLocator.Get<UIManager>().Open<SettingsWindow>(WindowId.SETTINGS_SCREEN, (window) => window.Setup(new int[] { 0, 0 }));
        }

        public void OnFiltersOffClick()
        {
            _waitForLocation = false;
            _filtersController.ResetFilters();
            RefreshFeed();
        }

        protected override void Awake()
        {
            MessageRouter.onMessageReceive += OnReceiveMessage;
            _filtersController.Initialize(OnApplyFilterCategory);
        }

        private void OnDestroy()
        {
            MessageRouter.onMessageReceive -= OnReceiveMessage;
        }

        private void OnApplicationPause(bool pause)
        {
            string status = pause ? "PAUSED" : "UNPAUSED";
            Debug.LogFormat("<color=yellow>APP {0}</color>", status);
        }

        private void OnApplicationFocus(bool focus)
        {
            string status = focus ? "ON" : "OFF";
            Debug.LogFormat("<color=orange>FOCUS {0}</color>", status);
        }

        void OnApplyFilterCategory()
        {
            _waitForLocation = false;
            _cts?.Cancel();
            _currentCard?.Clear();
            _endOfPostsController.Deactivate();
            RefreshFeed();
        }

        void RefreshFeed()
        {
            _currentCard?.Clear();
            _currentCard = null;
            _endOfPostsController.Deactivate();
            _loadingLabel.SetActive(true);
            _cts = new CancellationTokenSource();
            _waitForLocation = true;

//#if UNITY_EDITOR
            string location = "Longitude: -294.67 Latitude: 81.62";
            OnGetLocation(location);
//#elif UNITY_IOS
//            MessageIOSHandler.OnGetUserLocation();
//#endif

            //// fake loading posts
            //var category = _filtersController.GetFilterCategory();
            //await Task.Delay(600, _cts.Token);

            //if (_cts.IsCancellationRequested)
            //{
            //    return;
            //}

            //_cts = null;
            //_loadingLabel.SetActive(false);
            //_loadedCards = category == EventCategory.All
            //    ? new List<EventCard>(_testCards)
            //    : new List<EventCard>(Array.FindAll(_testCards, x => x.HasCategory(category)));
            //_currentCardIndex = 0;

            //if (_loadedCards.Count == 0)
            //{
            //    SetEndOfPosts();
            //    return;
            //}

            //CreateNextCard();
        }

        void CreateNextCard()
        {
            if (_currentCardIndex >= _loadedCards.Count)
            {
                // [TODO]: add label
                _currentCard = null;
                SetEndOfPosts();
                return;
            }

            var card = _loadedCards[_currentCardIndex];
            _currentCardIndex++;

            _currentCard = Instantiate(_cardPrefab, _cardsdContent);
            _currentCard.Setup(card, OnCardSkip);
        }

        void OnCardSkip(bool accept)
        {
            if (accept)
            {
                //ServiceLocator.Get<UIManager>().Open<ResponseInfoWindow>(WindowId.RESPONSE_INFO_SCREEN, (window) => window.Setup(true));
                _isResponse = true;
#if UNITY_EDITOR
                string path = System.IO.Path.Combine(Application.streamingAssetsPath, _editorVideoPath);
                OnRecordComplete(path);
#elif UNITY_IOS
                MessageIOSHandler.OnPressCameraButton();
#endif
                return;
            }

            CreateNextCard();
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

            var notifications = ServiceLocator.Get<GameModel>().GetNotifications();
            if (!notifications.newEvent)
            {
                _endOfPostsController.Activate(EndOfPostsType.NotificationsOff);
                return;
            }

            if (_filtersController.FiltersApplied())
            {
                _endOfPostsController.Activate(EndOfPostsType.FiltersSearch);
                return;
            }

            _endOfPostsController.Activate(EndOfPostsType.EmptyFeed);
        }

        void OnReceiveMessage(NativeMessageType type, string message)
        {
            switch (type)
            {
                case NativeMessageType.Video:
                    OnRecordComplete(message);
                    break;
                case NativeMessageType.Location:
                    OnGetLocation(message);
                    break;
                case NativeMessageType.Other:
                    Debug.LogFormat("Message: {0}", message);
                    break;
                default:
                    break;
            }
        }

        void OnRecordComplete(string videoPath)
        {
			if (string.IsNullOrEmpty(videoPath))
            {
                RefreshFeed();
                return;
            }
			
            ServiceLocator.Get<UIManager>().Open<VideoPreviewWindow>(WindowId.VIDEO_PREVIEW_SCREEN,
                (window) => window.Setup(videoPath, _isResponse));
        }

        async void OnGetLocation(string location)
        {
            if (_waitForLocation)
            {
                _waitForLocation = false;
                Debug.LogFormat("Location: {0}", location);

                // fake loading posts
                var category = _filtersController.GetFilterCategory();
                await Task.Delay(600, _cts.Token);

                if (_cts.IsCancellationRequested)
                {
                    return;
                }

                _cts = null;
                _loadingLabel.SetActive(false);
                _loadedCards = category == EventCategory.All
                    ? new List<EventCard>(_testCards)
                    : new List<EventCard>(Array.FindAll(_testCards, x => x.HasCategory(category)));
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
}
