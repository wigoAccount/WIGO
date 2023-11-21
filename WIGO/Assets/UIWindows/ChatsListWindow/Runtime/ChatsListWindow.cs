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

        List<UIChatInfo> _eventsData = new List<UIChatInfo>();
        Coroutine _loadingCoroutine;
        CancellationTokenSource _cts;

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
            //if (!PermissionsRequestManager.HasCameraPermission() || !PermissionsRequestManager.HasMicrophonePermission())
            //{
            //    CreatePermissionSettingPopup(true);
            //    return;
            //}

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

            var eventCreated = model.HasMyOwnEvent();
            _createEventButton.SetActive(!eventCreated);
            _myEventButton.SetActive(eventCreated);
        }

        private void OnDestroy()
        {
            MessageRouter.onMessageReceive -= OnReceiveMessage;
            var model = ServiceLocator.Get<GameModel>();
            model.OnChangeMyEventTime -= OnSetRemainingTime;
            model.OnControlMyEvent -= OnControlMyEvent;
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
                Debug.LogError("Error: Recorded video path is empty!");
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

        async void UpdateRequests()
        {
            _loadingCoroutine = StartCoroutine(ActivateLoadingWithDelay());

            _cts = new CancellationTokenSource();
            var model = ServiceLocator.Get<GameModel>();
            var requestsToMyEvent = await model.GetRequestsToMyEvent();
            var myOwnRequests = await NetService.TryGetMyRequests(model.GetUserLinks().data.address, model.ShortToken, _cts.Token);
            
            if (_cts.IsCancellationRequested)
            {
                _cts.Dispose();
                _cts = null;
                return;
            }

            _cts.Dispose();
            _cts = null;

            if (_loadingCoroutine != null)
            {
                StopCoroutine(_loadingCoroutine);
                _loadingCoroutine = null;
            }

            _view.SetLoadingVisible(false);
            bool requestsEmpty = requestsToMyEvent == null || requestsToMyEvent.Count() == 0;
            bool myRequestsEmpty = myOwnRequests == null || myOwnRequests.Count() == 0;
            if (requestsEmpty && myRequestsEmpty)
            {
                _view.SetEmptyTipVisible(true);
                return;
            }

            if (!requestsEmpty)
            {
                foreach (var request in requestsToMyEvent)
                {
                    UIChatInfo info = new UIChatInfo(request, OnSelectChat);
                    _eventsData.Add(info);
                }
            }
            
            if (!myRequestsEmpty)
            {
                foreach (var request in myOwnRequests)
                {
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
            ServiceLocator.Get<UIManager>().Open<EventViewWindow>(WindowId.EVENT_VIEW_SCREEN, window => window.Setup(request, isEvent));
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

        IEnumerator ActivateLoadingWithDelay()
        {
            yield return new WaitForSeconds(0.12f);
            _view.SetLoadingVisible(true);
            _loadingCoroutine = null;
        }
    }
}
