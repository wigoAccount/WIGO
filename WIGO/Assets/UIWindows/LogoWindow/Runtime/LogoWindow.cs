using System;
using System.Collections;
using UnityEngine;

namespace WIGO.Userinterface
{
    public class LogoWindow : UIWindow
    {
        [SerializeField] float _waitTime;
        [SerializeField] int _checkLocationMaxCount = 5;

        Coroutine _waitCoroutine;
        int _counter;

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
                _counter = 0;
                _waitCoroutine = StartCoroutine(DelayCheck(_waitTime));
            }
        }

        async void TrySendLocationToServer()
        {
            var model = ServiceLocator.Get<GameModel>();
            bool locationSent = await model.SendLocationDataToServer();
            
            if (!locationSent)
            {
                if (_counter >= _checkLocationMaxCount)
                {
                    ServiceLocator.Get<UIManager>().GetPopupManager().AddErrorNotification(20);
                    return;
                }

                _counter++;
                _waitCoroutine = StartCoroutine(DelayCheck(0.5f));
                return;
            }

            ServiceLocator.Get<UIManager>().Open<FeedWindow>(WindowId.FEED_SCREEN);
        }

        IEnumerator DelayCheck(float time)
        {
            yield return new WaitForSeconds(time);
            _waitCoroutine = null;
            TrySendLocationToServer();
        }
    }
}
