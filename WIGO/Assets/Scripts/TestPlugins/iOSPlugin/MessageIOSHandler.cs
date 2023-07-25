using AOT;
using System.Runtime.InteropServices;
using UnityEngine;

namespace WIGO.Utility
{
    public static class MessageIOSHandler
    {
#if UNITY_IOS && !UNITY_EDITOR
#region open controllers
        [DllImport("__Internal")]
        private static extern void startSwiftCameraController();

        [DllImport("__Internal")]
        private static extern void startSwiftMapController();

        [DllImport("__Internal")]
        private static extern void startSwiftTestController();

        //[DllImport("__Internal")]
        //private static extern void startSwiftTestUserLocationController();
        #endregion

        #region delegates
        public delegate void SwiftTestPluginVideoDidSave(string value);
        [DllImport("__Internal")]
        private static extern void setSwiftTestPluginVideoDidSave(SwiftTestPluginVideoDidSave callBack);

        public delegate void SwiftTestPluginLocationDidSend(string coordinates, string location);
        [DllImport("__Internal")]
        private static extern void setSwiftTestPluginLocationDidSend(SwiftTestPluginLocationDidSend callBack);

        //public delegate void SwiftTestPluginUserLocation(string value);
        //[DllImport("__Internal")]
        //private static extern void setSwiftTestPluginUserLocation(SwiftTestPluginUserLocation callBack);
        #endregion

        #region delegate handlers
        [MonoPInvokeCallback(typeof(SwiftTestPluginVideoDidSave))]
        public static void setSwiftTestPluginVideoDidSave(string value)
        {
            MessageRouter.RouteMessage(NativeMessageType.Video, value);
        }

        [MonoPInvokeCallback(typeof(SwiftTestPluginLocationDidSend))]
        public static void setSwiftTestPluginLocationDidSend(string coordinates, string location)
        {
            Debug.LogFormat("Get location Coord: {0}\r\nLocation: {1}", coordinates, location);
            string message = string.Join("\r\n", coordinates, location);
            MessageRouter.RouteMessage(NativeMessageType.Location, message);
        }

        //[MonoPInvokeCallback(typeof(SwiftTestPluginUserLocation))]
        //public static void setSwiftTestPluginUserLocation(string value)
        //{
        //    MessageRouter.RouteMessage(NativeMessageType.Location, value);
        //}
        #endregion

        [RuntimeInitializeOnLoadMethod]
        public static void Initialize()
        {
            setSwiftTestPluginVideoDidSave(setSwiftTestPluginVideoDidSave);
            setSwiftTestPluginLocationDidSend(setSwiftTestPluginLocationDidSend);
            //setSwiftTestPluginUserLocation(setSwiftTestPluginUserLocation);
        }

        public static void OnPressCameraButton()
        {
            startSwiftCameraController();
        }

        public static void OnPressMapButton()
        {
            startSwiftMapController();
        }

        public static void OnPressTestButton()
        {
            startSwiftTestController();
        }

        //public static void OnGetUserLocation()
        //{
        //    startSwiftTestUserLocationController();
        //}
#endif
    }
}
