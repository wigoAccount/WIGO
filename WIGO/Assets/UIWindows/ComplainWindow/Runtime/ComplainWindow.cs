using System;
using System.Threading.Tasks;
using UnityEngine;
using WIGO.Core;

namespace WIGO.Userinterface
{
    public class ComplainWindow : UIWindow
    {
        [SerializeField] PanelDragHandler _handler;

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

        public void Setup(AbstractEvent selected)
        {
            _selectedEvent = selected;
        }

        public async void OnComplainSelect(int id)
        {
            _handler.OnClose();
            await Task.Delay(200);
            Debug.LogFormat("Complaint sent: {0} with type {1}", _selectedEvent.uid, id);
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
