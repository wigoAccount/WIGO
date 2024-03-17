using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using WIGO.Core;
using WIGO.Userinterface;

namespace WIGO.Utility
{
    public partial class PushNotificationsController
    {
        INotificationAgent _notificationAgent = null;
        Action _pushAction = null;
        CancellationTokenSource _cts;

        bool _isPushNotificationAfterFocus = false;

        public PushNotificationsController()
        {
            _notificationAgent = null;
            _pushAction = null;
        }

        /// <summary>
        /// Configuring and starting the Notification Agent
        /// </summary>
        /// <returns><see cref="Task"/></returns>
        public async Task Init()
        {
            Application.focusChanged += Application_focusChanged;

            _notificationAgent = new FirebaseNotificationAgent();
            _notificationAgent.OnPushReceived += Message;

            await _notificationAgent.InitAgent();
            var token = await _notificationAgent.GetToken();
            await Authorize(token);
        }

        /// <summary>
        /// Stop the Notification Controller, Notification Agent and release resources
        /// </summary>
        public void Dispose()
        {
            Application.focusChanged -= Application_focusChanged;

            _cts?.Cancel();
            _cts = null;

            _notificationAgent.StopAgent();
            _notificationAgent.OnPushReceived -= Message;

            _notificationAgent = null;
            _pushAction = null;
        }

        /// <summary>
        /// Launch Action from Push
        /// </summary>
        public void FirePushAction()
        {
            if (_pushAction != null)
            {
                _pushAction.Invoke();
                _pushAction = null;
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Test Push Action. For Editor Only
        /// </summary>
        public void TestPush(string pushId, string category)
        {
            _isPushNotificationAfterFocus = true;
            Message(new PushNotificationStruct(
                "",
                "",
                "",
                "",
                true,
                new Dictionary<string, string> { { "window", "BASE_CUSTOMIZE_WINDOW" }, { "push_id", pushId }, { "param1", category }/*, { "param2", "BodyShape" }, {"param3", "Boots" }*/ }));
        }
#endif

        private async Task Authorize(string value)
        {
#if DEVELOPMENT_BUILD
            Debug.LogWarningFormat("Firebase Auth Token - {0}", value);
#endif

            var model = ServiceLocator.Get<GameModel>();
            _cts = new CancellationTokenSource();
            await NetService.RegisterDevicePush(value, model.GetUserLinks().data.address, model.ShortToken, _cts.Token);

            _cts.Dispose();
            _cts = null;
        }

        private void Message(PushNotificationStruct message)
        {
            Debug.LogFormat("Firebase Message Received:\r\n{0}", message.ToString());
            if (!message.opened_from_push || message.data == null)
                return;

            _pushAction = GetActionFromPush(message.data);

            if (_isPushNotificationAfterFocus)
            {
                _isPushNotificationAfterFocus = false;
                FirePushAction();
            }
        }

        private Action GetActionFromPush(IDictionary<string, string> data)
        {
            Action result = null;
            if (data.TryGetValue("window", out string w))
            {
                if (Enum.TryParse(w, true, out WindowId windowId))
                {
                    //result = Actions.OpenWindowWithOption(windowId, data);
                    Debug.LogFormat("<color=yellow>TEST parse actions -> window: {0}</color>", windowId.ToString());
                }
            }

            return result;
        }

        private void Application_focusChanged(bool focusOnApplication)
        {
            _isPushNotificationAfterFocus = focusOnApplication;
        }
    }
}
