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
        [SerializeField] bool _eventCreated;
        [SerializeField] string _editorVideoPath;

        List<Event> _loadedCards = new List<Event>();
        UIEventCardElement _currentCard;
        CancellationTokenSource _cts;
        Event _acceptedEvent;
        int _currentCardIndex;
        bool _waitForLocation;

        public override void OnOpen(WindowId previous)
        {
            var model = ServiceLocator.Get<GameModel>();
            var profile = model.GetMyProfile();
            _eventCreated = model.HasMyOwnEvent();
            _createEventButton.SetActive(!_eventCreated);
            _myEventButton.SetActive(_eventCreated);
            _userProfileElement.Setup(profile);
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
            _waitForLocation = false;
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

        public void OnCreateEventClick()
        {
            _acceptedEvent = null;
#if UNITY_EDITOR
            string path = System.IO.Path.Combine(Application.streamingAssetsPath, _editorVideoPath);
            OnRecordComplete(path);
#elif UNITY_IOS
            MessageIOSHandler.OnPressCameraButton();
#endif
        }

        public void OnOpenMyEvent()
        {
            ServiceLocator.Get<UIManager>().Open<EventsRequestsWindow>(WindowId.EVENTS_REQUESTS_SCREEN);
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
            ServiceLocator.Get<GameModel>().OnChangeMyEventTime += OnSetRemainingTime;
        }

        private void OnDestroy()
        {
            MessageRouter.onMessageReceive -= OnReceiveMessage;
            ServiceLocator.Get<GameModel>().OnChangeMyEventTime -= OnSetRemainingTime;
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

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ServiceLocator.Get<UIManager>().Open<ComplainWindow>(WindowId.COMPLAIN_SCREEN, window => window.Setup(new AbstractEvent()), true);
            }
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
            _acceptedEvent = null;
            _currentCard?.Clear();
            _currentCard = null;
            _endOfPostsController.Deactivate();
            _loadingLabel.SetActive(true);
            _waitForLocation = true;

#if UNITY_EDITOR
            string location = "55.767,37.684";
            OnGetLocation(location);
#elif UNITY_IOS
            MessageIOSHandler.OnGetUserLocation();
#endif
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

        void OnCardSwipe(Event accepted, bool accept)
        {
            if (accept)
            {
                _acceptedEvent = accepted;
                UIGameColors.SetTransparent(_overlay);
                _overlay.gameObject.SetActive(true);
                _overlay.DOFade(1f, 0.4f).OnComplete(() => StartCoroutine(DelayLaunchRecord()));

                return;
            }

            CreateNextCard();
        }

        IEnumerator DelayLaunchRecord()
        {
            _currentCard = null;
            yield return new WaitForEndOfFrame();

#if UNITY_EDITOR
            string path = System.IO.Path.Combine(Application.streamingAssetsPath, _editorVideoPath);
            OnRecordComplete(path);
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

        void OnSetRemainingTime(int seconds)
        {
            int minutes = Mathf.FloorToInt(seconds / 60f);
            _remainingTimeLabel.SetText(minutes.ToString());
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
                    break;
                case NativeMessageType.MyLocation:
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
                (window) => window.Setup(videoPath, _acceptedEvent));
        }

        async void OnGetLocation(string location)
        {
            if (_waitForLocation)
            {
                _waitForLocation = false;
                Debug.LogFormat("Get location before get cards: {0}", location);

                var model = ServiceLocator.Get<GameModel>();
                var locationData = GameConsts.ParseLocation(location);
                await NetService.TrySendLocation(locationData, model.GetUserLinks().data.address, model.ShortToken);
                await UpdateFeedCards();
            }
        }

        async Task UpdateFeedCards()
        {
            int categoryUid = _filtersController.GetFilterCategory();
            var model = ServiceLocator.Get<GameModel>();
            _cts = new CancellationTokenSource();
            _cts.CancelAfter(8000);

            int[] tags = categoryUid == 0 ? new int[0] : new int[] { categoryUid };
            FeedRequest request = new FeedRequest()
            {
                tags = tags
            };
            IEnumerable<Event> cards = await NetService.TryGetFeedEvents(request, model.GetUserLinks().data.address, model.ShortToken, _cts.Token);

            if (_cts.IsCancellationRequested)
            {
                return;
            }

            _cts.Dispose();
            _cts = null;
            _loadingLabel.SetActive(false);
            _loadedCards = categoryUid == 0
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
