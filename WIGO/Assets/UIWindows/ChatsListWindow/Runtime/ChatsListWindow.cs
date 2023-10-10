using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using WIGO.Core;

namespace WIGO.Userinterface
{
    public class ChatsListWindow : UIWindow
    {
        [SerializeField] ChatsListView _view;
        [SerializeField] RecyclableChatsListScroll _chatsScroll;
        [SerializeField] ChatCategoriesPanel _categoriesPanel;
        [SerializeField] WindowAnimator _animator;

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
        }

        public override void OnClose(WindowId next, Action callback = null)
        {
            _cts?.Cancel();
            if (next != WindowId.EVENT_VIEW_SCREEN)
            {
                ClearWindow();
            }

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

        protected override void Awake()
        {
            _view.Init();
            _categoriesPanel.Init(OnChatCategorySelect);
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
            if (requestsToMyEvent.Count() == 0 && myOwnRequests.Count() == 0)
            {
                _view.SetEmptyTipVisible(true);
                return;
            }

            foreach (var request in requestsToMyEvent)
            {
                UIChatInfo info = new UIChatInfo(request, OnSelectChat);
                _eventsData.Add(info);
            }

            foreach (var request in myOwnRequests)
            {
                UIChatInfo info = new UIChatInfo(request, OnSelectChat, true);
                _eventsData.Add(info);
            }

            _chatsScroll.CreateScroll(_eventsData);
        }

        void OnChatCategorySelect(ChatCategory category)
        {
            switch (category)
            {
                case ChatCategory.All:
                    if (_eventsData.Count > 0)
                    {
                        _view.SetEmptyTipVisible(false);
                        _chatsScroll.CreateScroll(_eventsData);
                    }
                    break;
                case ChatCategory.MyEvents:
                case ChatCategory.MyRequests:
                    bool response = category == ChatCategory.MyEvents;
                    var data = _eventsData.FindAll(x => x.GetCard().IsResponse() == response);
                    if (data == null || data.Count == 0)
                    {
                        _chatsScroll.ClearScroll();
                        _view.SetEmptyTipVisible(true);
                    }
                    else
                    {
                        _chatsScroll.CreateScroll(data);
                    }
                    break;
                default:
                    break;
            }
        }

        // [TODO]: set isMyRequest in Request class
        void OnSelectChat(Request request)
        {
            ServiceLocator.Get<UIManager>().Open<EventViewWindow>(WindowId.EVENT_VIEW_SCREEN, window => window.Setup(request, false));
        }

        void ClearWindow()
        {
            _chatsScroll.ClearScroll();
            _eventsData.Clear();
            _categoriesPanel.ResetCategories();
            _view.SetLoadingVisible(false);
            _view.SetEmptyTipVisible(false);
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
