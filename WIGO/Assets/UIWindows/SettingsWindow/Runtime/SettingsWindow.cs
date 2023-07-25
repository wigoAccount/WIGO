using System;
using System.Collections.Generic;
using UnityEngine;

namespace WIGO.Userinterface
{
    public class SettingsWindow : UIWindow
    {
        [SerializeField] WindowAnimator _animator;
        [SerializeField] SettingsLevelMain _mainScreen;

        public override void OnOpen(WindowId previous)
        {
            _animator.OnOpen();
        }

        public override void OnReopen(WindowId previous, UIWindowModel cachedModel)
        {
            _animator.OnReopen();
        }

        public override void OnClose(WindowId next, Action callback = null)
        {
            _mainScreen.ResetWindow();
            callback?.Invoke();
        }

        public void Setup(int[] path)
        {
            if (path == null || path.Length == 0)
            {
                return;
            }

            _mainScreen.OnClose();
            var child = _mainScreen.OpenChildQuiet(path[0]);
            for (int i = 1; i < path.Length; i++)
            {
                if (child == null)
                {
                    break;
                }

                child = child.OpenChildQuiet(path[i]);
            }
            
            if (child != null)
            {
                child.gameObject.SetActive(true);
            }
        }

        public void OnMainFeedClick()
        {
            ServiceLocator.Get<UIManager>().SwitchTo(WindowId.FEED_SCREEN);
        }

        public void OnChatsClick()
        {
            ServiceLocator.Get<UIManager>().Open<ChatsListWindow>(WindowId.CHATS_LIST_SCREEN);
        }
    }
}
