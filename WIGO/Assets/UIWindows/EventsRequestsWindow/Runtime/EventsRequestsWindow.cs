using System;
using UnityEngine;

namespace WIGO.Userinterface
{
    public class EventsRequestsWindow : UIWindow
    {
        [SerializeField] EventsRequestsHeader _header;
        [SerializeField] EventsPart _eventsPart;
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
            //ServiceLocator.Get<UIManager>().Open<RecordEventWindow>(WindowId.RECORD_EVENT_SCREEN, (window) => window.Setup(false));
        }

        protected override void Awake()
        {
            _header.Initialize(OnChangeCategory);
            _eventsPart.Initialize();
            //_requestsPart.Initialize();
        }

        void OnChangeCategory(int category)
        {
            _header.ChangeCategory(category);
            _eventsPart.SetPartActive(category == 0);
            //_requestsPart.SetPartActive(category == 1);
        }
    }
}
