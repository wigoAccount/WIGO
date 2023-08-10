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

        [DllImport("__Internal")]
        private static extern void swiftTestPluginNearestLocations();

        [DllImport("__Internal")]
        private static extern void startSwiftCalcTimeController(string myLocation, string theirLocation);
        #endregion

        #region delegates
        public delegate void SwiftTestPluginVideoDidSave(string value);
        [DllImport("__Internal")]
        private static extern void setSwiftTestPluginVideoDidSave(SwiftTestPluginVideoDidSave callBack);

        public delegate void SwiftTestPluginLocationDidSend(string coordinates, string location);
        [DllImport("__Internal")]
        private static extern void setSwiftTestPluginLocationDidSend(SwiftTestPluginLocationDidSend callBack);

        public delegate void SwiftTestPluginNearestLocations(string value);
        [DllImport("__Internal")]
        private static extern void setSwiftTestPluginNearestLocations(SwiftTestPluginNearestLocations callBack);

        public delegate void SwiftTestPluginCalculateTime();
        [DllImport("__Internal")]
        private static extern void setSwiftTestPluginCalculateTime(SwiftTestPluginCalculateTime callBack);
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

        [MonoPInvokeCallback(typeof(SwiftTestPluginNearestLocations))]
        public static void setSwiftTestPluginNearestLocations(string value)
        {
            MessageRouter.RouteMessage(NativeMessageType.MyLocation, value);
        }
        #endregion

        [RuntimeInitializeOnLoadMethod]
        public static void Initialize()
        {
            setSwiftTestPluginVideoDidSave(setSwiftTestPluginVideoDidSave);
            setSwiftTestPluginLocationDidSend(setSwiftTestPluginLocationDidSend);
            setSwiftTestPluginNearestLocations(setSwiftTestPluginNearestLocations);
            setSwiftTestPluginCalculateTime(() => Debug.Log("Map was opened and closed!"));
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

        public static void OnGetUserLocation()
        {
            swiftTestPluginNearestLocations();
        }

        public static void OnViewMap(string myLocation, string theirLocation)
        {
            startSwiftCalcTimeController(myLocation, theirLocation);
        }
#endif
    }
}
