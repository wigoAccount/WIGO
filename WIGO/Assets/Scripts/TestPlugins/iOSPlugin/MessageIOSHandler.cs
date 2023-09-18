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
        private static extern void startSwiftRouteController(string theirLocation);

        [DllImport("__Internal")]
        private static extern void startTurnGeolocationController();
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
        private static extern void swiftTestPluginNearestLocations(SwiftTestPluginNearestLocations callBack);
        #endregion

        #region delegate handlers
        [MonoPInvokeCallback(typeof(SwiftTestPluginVideoDidSave))]
        public static void onGetVideoPath(string value)
        {
            MessageRouter.RouteMessage(NativeMessageType.Video, value);
        }

        [MonoPInvokeCallback(typeof(SwiftTestPluginLocationDidSend))]
        public static void onGetFullLocation(string coordinates, string location)
        {
            Debug.LogFormat("Get location Coord: {0}\r\nLocation: {1}", coordinates, location);
            string message = string.Join("\r\n", coordinates, location);
            MessageRouter.RouteMessage(NativeMessageType.Location, message);
        }

        [MonoPInvokeCallback(typeof(SwiftTestPluginNearestLocations))]
        public static void onGetMyLocation(string value)
        {
            MessageRouter.RouteMessage(NativeMessageType.MyLocation, value);
        }
        #endregion

        [RuntimeInitializeOnLoadMethod]
        public static void Initialize()
        {
            setSwiftTestPluginVideoDidSave(onGetVideoPath);
            setSwiftTestPluginLocationDidSend(onGetFullLocation);
            swiftTestPluginNearestLocations(onGetMyLocation);
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

        public static void OnViewMap(string theirLocation)
        {
            startSwiftRouteController(theirLocation);
        }

        public static void OnAllowLocationPermission()
        {
            startTurnGeolocationController();
        }
#endif
    }
}
