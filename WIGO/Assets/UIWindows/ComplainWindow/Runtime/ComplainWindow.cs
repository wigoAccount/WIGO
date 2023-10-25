using System;
using UnityEngine;
using WIGO.Core;

namespace WIGO.Userinterface
{
    public class ComplainWindow : UIWindow
    {
        [SerializeField] PanelDragHandler _handler;
        [Space]
        [SerializeField] string[] _complaintsText;

        Action _onComplaintSent;
        ComplainWindowView _view;
        AbstractEvent _selectedEvent;

        public override void OnOpen(WindowId previous)
        {
            _view.OnOpen();
            _handler.OnOpen();
        }

        public override void OnClose(WindowId next, Action callback = null)
        {
            _view.OnClose(callback);
        }

        public void Setup(AbstractEvent selected, Action onComplaintSent = null)
        {
            _selectedEvent = selected;
            _onComplaintSent = onComplaintSent;
        }

        public async void OnComplainSelect(int id)
        {
            _handler.OnClose();
            _onComplaintSent?.Invoke();

            var model = ServiceLocator.Get<GameModel>();
            CreateComplaintRequest request = new CreateComplaintRequest()
            {
                eventid = _selectedEvent.uid,
                txt = _complaintsText[id]
            };

            bool res = await NetService.TrySendComplaint(request, model.GetUserLinks().data.address, model.ShortToken);
            if (res)
            {
                ServiceLocator.Get<UIManager>().GetPopupManager().AddDoneNotification();
            }
        }

        public void OnCancelClick()
        {
            _handler.OnClose();
        }

        protected override void Awake()
        {
            _view = GetComponent<ComplainWindowView>();
            _handler.Init(CloseWindow);
        }

        void CloseWindow()
        {
            ServiceLocator.Get<UIManager>().CloseCurrent();
        }
    }
}
