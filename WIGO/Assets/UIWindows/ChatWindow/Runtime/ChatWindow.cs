using Crystal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using WIGO.Core;
using WIGO.Utility;

namespace WIGO.Userinterface
{
    public class ChatWindow : UIWindow
    {
        [SerializeField] ChatWindowView _view;
        [SerializeField] WindowAnimator _animator;
        [SerializeField] RecyclableChatScroll _scroll;
        [SerializeField] ChatInputBottomPanel _inputPanel;
        [SerializeField] SafeArea _safeArea;

        ChatData _currentChatData;
        Coroutine _keyboardCoroutine;
        KeyboardManager _keyboardManager;

        public override void OnOpen(WindowId previous)
        {
            _animator.OnOpen();
        }

        public override void OnClose(WindowId next, Action callback = null)
        {
            _inputPanel.ResetPanel();
            _scroll.ClearScroll();
            base.OnClose(next, callback);
        }

        public async void Setup(ChatData data)
        {
            await Task.Delay(100);

            var profile = new UserProfile();
            profile.SetUsername("User062137");
            Setup(data, profile);
        }

        public void Setup(ChatData data, UserProfile profile)
        {
            _currentChatData = data;
            _view.SetupInfo(profile);
            CreateMessages();
        }

        public void OnBackButtonClick()
        {
            ServiceLocator.Get<UIManager>().CloseCurrent();
        }

        public void OnSendMessageClick()
        {
            string message = _inputPanel.GetMessage();
            if (!string.IsNullOrEmpty(message))
            {
                _inputPanel.ClearMessage();
                _scroll.SetAddingHeight(0f);
                OnSendMessage(message);
            }
        }

        protected override void Awake()
        {
            _view.Init();
            _scroll.Init();
            _inputPanel.Initialize(OnActivateKeyboard, (delta) => _scroll?.SetAddingHeight(delta));
            _keyboardManager = ServiceLocator.Get<KeyboardManager>();
        }

        void CreateMessages()
        {
            // [TODO]: clear if content's empty

            List<UIMessageInfo> data = new List<UIMessageInfo>();
            foreach (var msg in _currentChatData.GetMessages())
            {
                var info = new UIMessageInfo(msg);
                data.Add(info);
            }

            _scroll?.CreateScroll(data);
        }

        void AddMessage(ChatMessage message)
        {
            _currentChatData.AddMessage(message);
            CreateMessages();
        }

        async void OnSendMessage(string message)
        {
            ChatMessage data = new ChatMessage(message, DateTime.Now);
            await Task.Delay(100);

            AddMessage(data);
        }

        void OnActivateKeyboard(bool activate)
        {
            if (_keyboardCoroutine != null)
            {
                StopCoroutine(_keyboardCoroutine);
                _keyboardCoroutine = null;
            }

            if (activate)
            {
                _keyboardCoroutine = StartCoroutine(CalculateKeyboardHeight());
                return;
            }

            _inputPanel.SetViewDefault();
            _scroll?.SetToDefaultBottom();
        }

        IEnumerator CalculateKeyboardHeight()
        {
            for (float timer = 0f; timer < 1f; timer += Time.deltaTime / 1f)
            {
                float height = _keyboardManager.GetKeyboardHeight() - _safeArea.GetSafeAreaBottomPadding(); // get height
                _inputPanel.SetHeight(height);
                _scroll?.SetBottomHeight(height);
                yield return null;
            }

            _keyboardCoroutine = null;
        }
    }
}
