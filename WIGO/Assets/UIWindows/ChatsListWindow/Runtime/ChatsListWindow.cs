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
            //    CreatePermissionSettingPopup();
            //    return;
            //}

            //CreateEvent();
#if UNITY_EDITOR
            OnRecordComplete(_editorVideoPath);
#elif UNITY_IOS
            MessageIOSHandler.OnPressCameraButton();
#endif
        }

        void CreateEvent()
        {
#if UNITY_EDITOR
            OnRecordComplete(_editorVideoPath);
#elif UNITY_IOS
            MessageIOSHandler.OnPressCameraButton();
#endif
        }

        void CreatePermissionSettingPopup()
        {
            List<PopupOption> options = new List<PopupOption>
            {
                new PopupOption(_permissionData.GetMessageAt(1), OnOpenAppSettings),
                new PopupOption(_permissionData.GetMessageAt(2), OnDeclinePermissions, UIGameColors.RED_HEX)
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

        void OnDeclinePermissions()
        {
            ServiceLocator.Get<UIManager>().GetPopupManager().CloseCurrentPopup();
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
