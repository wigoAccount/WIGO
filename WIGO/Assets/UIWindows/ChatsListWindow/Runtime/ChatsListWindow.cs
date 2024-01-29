using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine;
using WIGO.Core;
using WIGO.Utility;

namespace WIGO.Userinterface
{
    public class ChatsListWindow : UIWindow
    {
        [SerializeField] ChatsListView _view;
        [SerializeField] RecyclableChatsListScroll _chatsScroll;
        [SerializeField] ChatCategoriesPanel _categoriesPanel;
        [SerializeField] WindowAnimator _animator;
        [Space]
        [SerializeField] GameObject _createEventButton;
        [SerializeField] GameObject _myEventButton;
        [SerializeField] TMP_Text _remainingTimeLabel;
        [SerializeField] string _editorVideoPath;
        [SerializeField] TempMessagesContainer _permissionData;
        [SerializeField] UpdateArea _updateArea;

        List<UIChatInfo> _eventsData = new List<UIChatInfo>();
        Coroutine _loadingCoroutine;
        CancellationTokenSource _cts;
        bool _focusLost;

        public override void OnOpen(WindowId previous)
        {
            _animator.OnOpen();
            UpdateRequests();
        }

        public override void OnReopen(WindowId previous, UIWindowModel cachedModel)
        {
            _animator.OnReopen();
            UpdateRequests();
        }

        public override void OnClose(WindowId next, Action callback = null)
        {
            _cts?.Cancel();
            ClearWindow(next != WindowId.EVENT_VIEW_SCREEN);

            callback?.Invoke();
        }

        public override void OnBack(WindowId previous, Action callback = null)
        {
            _cts?.Cancel();
            ClearWindow();
            callback?.Invoke();
        }

        public void OnMainMenuClick()
        {
            ServiceLocator.Get<UIManager>().SwitchTo(WindowId.FEED_SCREEN);
        }

        public void OnSettingsClick()
        {
            ServiceLocator.Get<UIManager>().Open<SettingsWindow>(WindowId.SETTINGS_SCREEN);
        }

        public void OnCreateEventClick()
        {
#if UNITY_EDITOR
            OnRecordComplete(_editorVideoPath);
#elif UNITY_IOS
            MessageIOSHandler.OnPressCameraButton();
#endif
        }

        public void OnOpenMyEvent()
        {
            ServiceLocator.Get<UIManager>().Open<EventsRequestsWindow>(WindowId.EVENTS_REQUESTS_SCREEN);
        }

        protected override void Awake()
        {
            MessageRouter.onMessageReceive += OnReceiveMessage;
            _view.Init();
            _categoriesPanel.Init(OnChatCategorySelect);
            var model = ServiceLocator.Get<GameModel>();
            model.OnChangeMyEventTime += OnSetRemainingTime;
            model.OnControlMyEvent += OnControlMyEvent;
            model.OnGetUpdates += OnGetUpdates;
            
            int newEvents = model.GetUnreadEventsCount(true);
            int newRequests = model.GetUnreadEventsCount(false);
            _categoriesPanel.SetUnreadMessages(false, newEvents);
            _categoriesPanel.SetUnreadMessages(true, newRequests);

            var eventCreated = model.HasMyOwnEvent();
            _createEventButton.SetActive(!eventCreated);
            _myEventButton.SetActive(eventCreated);
            _updateArea.Init(UpdateRequests);
        }

        private void OnDestroy()
        {
            MessageRouter.onMessageReceive -= OnReceiveMessage;
            var model = ServiceLocator.Get<GameModel>();
            model.OnChangeMyEventTime -= OnSetRemainingTime;
            model.OnControlMyEvent -= OnControlMyEvent;
            model.OnGetUpdates -= OnGetUpdates;
        }

        private void OnApplicationPause(bool pause)
        {
            if (!pause && _focusLost)
            {
                _focusLost = false;
                ServiceLocator.Get<UIManager>().GetPopupManager().CloseCurrentPopup();
            }
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
            if (string.IsNullOrEmpty(videoPath) || string.Compare(videoPath, "null") == 0)
            {
                Debug.LogWarning("Recorded video path is empty!");
                return;
            }

            ServiceLocator.Get<UIManager>().Open<VideoPreviewWindow>(WindowId.VIDEO_PREVIEW_SCREEN,
                (window) => window.Setup(videoPath, null));
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

        void OnGetUpdates(bool isEvents, int count)
        {
            _categoriesPanel.SetUnreadMessages(!isEvents, count);
            if (gameObject.activeSelf)
            {
                UpdateWithCache();
            }
        }

        void UpdateWithCache()
        {
            var model = ServiceLocator.Get<GameModel>();
            var requestsToMyEvent = model.GetCachedRequestsToMyEvent();
            var myOwnRequests = model.GetAllMyRequests();
            CreateScroll(requestsToMyEvent, myOwnRequests);
        }

        async void UpdateRequests()
        {
            _loadingCoroutine = StartCoroutine(ActivateLoadingWithDelay());

            var model = ServiceLocator.Get<GameModel>();
            await model.UpdateAndLoadEventsAndRequests();

            _updateArea.DeactivateLoading();
            var requestsToMyEvent = model.GetCachedRequestsToMyEvent();
            var myOwnRequests = model.GetAllMyRequests();
            int eventUpdates = model.GetUnreadEventsCount(true);
            int requestUpdates = model.GetUnreadEventsCount(false);
            _categoriesPanel.SetUnreadMessages(false, eventUpdates);
            _categoriesPanel.SetUnreadMessages(true, requestUpdates);

            //_cts = new CancellationTokenSource();
            //var model = ServiceLocator.Get<GameModel>();
            //var requestsToMyEvent = await model.GetRequestsToMyEvent();
            //await model.UpdateMyRequests();
            //var myOwnRequests = model.GetAllMyRequests();//NetService.TryGetMyRequests(model.GetUserLinks().data.address, model.ShortToken, _cts.Token);

            //if (_cts.IsCancellationRequested)
            //{
            //    _cts.Dispose();
            //    _cts = null;
            //    return;
            //}

            //_cts.Dispose();
            //_cts = null;

            if (_loadingCoroutine != null)
            {
                StopCoroutine(_loadingCoroutine);
                _loadingCoroutine = null;
            }

            _view.SetLoadingVisible(false);
            CreateScroll(requestsToMyEvent, myOwnRequests);
        }

        void CreateScroll(IEnumerable<Request> requestsToMyEvent, IEnumerable<Request> myOwnRequests)
        {
            _eventsData.Clear();
            bool requestsEmpty = requestsToMyEvent == null || requestsToMyEvent.Count() == 0;
            bool myRequestsEmpty = myOwnRequests == null || myOwnRequests.Count() == 0;
            if (requestsEmpty && myRequestsEmpty)
            {
                _chatsScroll.ClearScroll();
                _view.SetEmptyTipVisible(true);
                return;
            }

            if (!requestsEmpty)
            {
                foreach (var request in requestsToMyEvent)
                {
                    if (request.GetStatus() == Request.RequestStatus.decline)
                        continue;

                    UIChatInfo info = new UIChatInfo(request, OnSelectChat);
                    _eventsData.Add(info);
                }
            }

            if (!myRequestsEmpty)
            {
                foreach (var request in myOwnRequests)
                {
                    if (request.GetStatus() == Request.RequestStatus.decline)
                        continue;

                    UIChatInfo info = new UIChatInfo(request, OnSelectChat, true);
                    _eventsData.Add(info);
                }
            }

            OnChatCategorySelect(_categoriesPanel.GetOpenedCategory());
        }

        void OnChatCategorySelect(ChatCategory category)
        {
            switch (category)
            {
                case ChatCategory.All:
                    if (_eventsData.Count > 0)
                    {
                        _chatsScroll.CreateScroll(_eventsData);
                    }
                    _view.SetEmptyTipVisible(_eventsData.Count == 0);
                    break;
                case ChatCategory.MyEvents:
                case ChatCategory.MyRequests:
                    bool response = category == ChatCategory.MyEvents;
                    var data = _eventsData.FindAll(x => x.GetCard().IsResponse() == response);
                    if (data == null || data.Count == 0)
                    {
                        _chatsScroll.ClearScroll();
                    }
                    else
                    {
                        _chatsScroll.CreateScroll(data);
                    }
                    _view.SetEmptyTipVisible(data == null || data.Count == 0);
                    break;
                default:
                    break;
            }
        }

        void OnSelectChat(Request request, bool isEvent)
        {
            ServiceLocator.Get<UIManager>().Open<EventViewWindow>(WindowId.EVENT_VIEW_SCREEN, window => window.Setup(request, isEvent, OnAbstractEventWatched));
        }

        void ClearWindow(bool resetCategory = true)
        {
            _chatsScroll.ClearScroll();
            _eventsData.Clear();
            _view.SetLoadingVisible(false);
            _view.SetEmptyTipVisible(false);
            if (resetCategory)
            {
                _categoriesPanel.ResetCategories();
            }

            if (_loadingCoroutine != null)
            {
                StopCoroutine(_loadingCoroutine);
                _loadingCoroutine = null;
            }
        }

        void OnAbstractEventWatched(bool isEvent)
        {
            var model = ServiceLocator.Get<GameModel>();
            _categoriesPanel.SetUnreadMessages(!isEvent, model.GetUnreadEventsCount(isEvent));
        }

        IEnumerator ActivateLoadingWithDelay()
        {
            yield return new WaitForSeconds(0.12f);
            _view.SetLoadingVisible(true);
            _loadingCoroutine = null;
        }
    }
}
