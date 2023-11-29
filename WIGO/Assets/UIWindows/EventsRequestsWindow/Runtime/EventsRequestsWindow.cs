using System;
using UnityEngine;
using WIGO.Utility;

namespace WIGO.Userinterface
{
    public class EventsRequestsWindow : UIWindow
    {
        [SerializeField] EventsRequestsHeader _header;
        [SerializeField] EventsPart _eventsPart;
        [SerializeField] string _editorVideoPath;
        //[SerializeField] RequestsPart _requestsPart;

        public override void OnOpen(WindowId previous)
        {
            _eventsPart.SetPartActive(true);
            //_requestsPart.SetPartActive(false);
        }

        public override void OnClose(WindowId next, Action callback = null)
        {
            if (next == WindowId.CHATS_LIST_SCREEN || next == WindowId.SETTINGS_SCREEN)
            {
                _header.ResetHeader();
                _eventsPart.ResetPart();
                //_requestsPart.ResetPart();
                //_eventsPart.SetPartActive(true, false);
                //_requestsPart.SetPartActive(false, false);
            }
            base.OnClose(next, callback);
        }

        public override void OnBack(WindowId previous, Action callback = null)
        {
            _header.ResetHeader();
            _eventsPart.ResetPart();
            //_requestsPart.ResetPart();
            //_eventsPart.SetPartActive(true, false);
            //_requestsPart.SetPartActive(false, false);
            
            callback?.Invoke();
        }

        public void OnMainScreenClick()
        {
            ServiceLocator.Get<UIManager>().SwitchTo(WindowId.FEED_SCREEN);
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
#if UNITY_EDITOR
            OnRecordComplete(_editorVideoPath);
#elif UNITY_IOS
            MessageIOSHandler.OnPressCameraButton();
#endif
        }

        protected override void Awake()
        {
            MessageRouter.onMessageReceive += OnReceiveMessage;
            _header.Initialize(OnChangeCategory);
            _eventsPart.Initialize();
            //_requestsPart.Initialize();
        }

        private void OnDestroy()
        {
            MessageRouter.onMessageReceive -= OnReceiveMessage;
        }

        void OnChangeCategory(int category)
        {
            _header.ChangeCategory(category);
            _eventsPart.SetPartActive(category == 0);
            //_requestsPart.SetPartActive(category == 1);
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
                return;
            }

            ServiceLocator.Get<UIManager>().Open<VideoPreviewWindow>(WindowId.VIDEO_PREVIEW_SCREEN,
                (window) => window.Setup(videoPath, null));
        }
    }
}
