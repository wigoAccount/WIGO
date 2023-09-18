using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        [SerializeField] int _chatsCount;

        List<UIChatInfo> _eventsData = new List<UIChatInfo>();
        Coroutine _loadingCoroutine;

        // remove _isClosed
        bool _isClosed;

        public override void OnOpen(WindowId previous)
        {
            _isClosed = false;
            _animator.OnOpen();
            CreateChats();
        }

        public override void OnReopen(WindowId previous, UIWindowModel cachedModel)
        {
            _animator.OnReopen();
        }

        public override void OnClose(WindowId next, Action callback = null)
        {
            if (next != WindowId.EVENT_VIEW_SCREEN)
            {
                ClearWindow();
            }

            callback?.Invoke();
            _isClosed = true;
        }

        public override void OnBack(WindowId previous, Action callback = null)
        {
            ClearWindow();
            callback?.Invoke();
            _isClosed = true;
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

        async void CreateChats()
        {
            _loadingCoroutine = StartCoroutine(ActivateLoadingWithDelay());

            // [TODO]: update my event
            // get requests from my event and check them as Response
            // get my requests to other events
            // show both types in list
            await Task.Delay(600);

            // [TODO]: temp solution. Use cts
            if (_isClosed)
                return;

            if (_loadingCoroutine != null)
            {
                StopCoroutine(_loadingCoroutine);
                _loadingCoroutine = null;
            }

            _view.SetLoadingVisible(false);
            if (_chatsCount == 0)
            {
                _view.SetEmptyTipVisible(true);
                return;
            }

            for (int i = 0; i < _chatsCount; i++)
            {
                AbstractEvent card;
                if (UnityEngine.Random.Range(0f, 1f) > 0.5f)
                {
                    card = new Request();
                }
                else
                {
                    card = new Core.Event();
                }
                UIChatInfo info = new UIChatInfo(card, OnSelectChat);
                _eventsData.Add(info);
                //_chatsData.Add(info);
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

        void OnSelectChat(AbstractEvent card)
        {
            ServiceLocator.Get<UIManager>().Open<EventViewWindow>(WindowId.EVENT_VIEW_SCREEN, window => window.Setup(card));
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
