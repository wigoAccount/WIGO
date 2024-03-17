using System;
using UnityEngine;
using Firebase.Messaging;
using System.Threading.Tasks;

namespace WIGO.Utility
{
    public class FirebaseNotificationAgent : INotificationAgent
    {
        public event Action<string> OnAuthorization;
        public event Action<PushNotificationStruct> OnPushReceived;

        /// <summary>
        /// Receiving a token from FCM.
        /// </summary>
        /// <returns>Unique token associated with app instance</returns>
        public async Task<string> GetToken() => await FirebaseMessaging.GetTokenAsync();

        /// <summary>
        /// Initialization Notification Agent
        /// </summary>
        public async Task InitAgent()
        {
            FirebaseMessaging.TokenReceived += OnTokenReceived;
            FirebaseMessaging.MessageReceived += OnMessageReceived;

            // request os permissions for push notifications
            try
            {
                await FirebaseMessaging.RequestPermissionAsync();
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to RequestPermissionAsync");
                Debug.LogException(e);
            }
        }

        /// <summary>
        /// Stopping the Notification Agent
        /// </summary>
        public void StopAgent()
        {
            FirebaseMessaging.TokenReceived -= OnTokenReceived;
            FirebaseMessaging.MessageReceived -= OnMessageReceived;
            OnAuthorization = null;
            OnPushReceived = null;
        }

        private void OnTokenReceived(object sender, TokenReceivedEventArgs token)
        {
            Debug.LogFormat("<color=yellow>Push token received: {0}</color>", token.Token);
            OnAuthorization?.Invoke(token.Token);
        }

        private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            var message = new PushNotificationStruct
            (
                e.Message.Notification?.Title ?? string.Empty,
                e.Message.Notification?.Body ?? string.Empty,
                e.Message.From,
                e.Message.Link?.ToString() ?? string.Empty,
                e.Message.NotificationOpened,
                e.Message.Data
            );
            
            OnPushReceived?.Invoke(message);
        }
    }
}