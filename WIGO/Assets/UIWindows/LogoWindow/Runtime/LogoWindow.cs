using System;
using System.Collections;
using UnityEngine;

namespace WIGO.Userinterface
{
    public class LogoWindow : UIWindow
    {
        [SerializeField] float _waitTime;

        Coroutine _waitCoroutine;

        public override void OnClose(WindowId next, Action callback = null)
        {
            if (_waitCoroutine != null)
            {
                StopCoroutine(_waitCoroutine);
                _waitCoroutine = null;
            }
            base.OnClose(next, callback);
        }

        public void Setup(bool startCounter)
        {
            if (startCounter)
            {
                _waitCoroutine = StartCoroutine(DelayClose());
            }
        }

        IEnumerator DelayClose()
        {
            yield return new WaitForSeconds(_waitTime);
            ServiceLocator.Get<UIManager>().Open<FeedWindow>(WindowId.FEED_SCREEN);
        }
    }
}
