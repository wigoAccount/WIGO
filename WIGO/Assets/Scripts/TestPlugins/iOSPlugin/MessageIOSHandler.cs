using AOT;
using System.Runtime.InteropServices;
using UnityEngine;
using WIGO.Core;

namespace WIGO.Utility
{
    public static class MessageIOSHandler
    {
#if UNITY_IOS && !UNITY_EDITOR
        #region open controllers
        [DllImport("__Internal")]
        private static extern void startSwiftCameraControllerLocation();

        [DllImport("__Internal")]
        private static extern void startSwiftMapController();

        [DllImport("__Internal")]
        private static extern string GetLastKnownLocation();

        [DllImport("__Internal")]
        private static extern void startSwiftRouteController(string theirLocation);

        [DllImport("__Internal")]
        private static extern void startTurnGeolocation();
        #endregion

        #region delegates
        public delegate void SwiftTestPluginVideoDidSave(string value);
        [DllImport("__Internal")]
        private static extern void setSwiftTestPluginVideoDidSaveLocation(SwiftTestPluginVideoDidSave callBack);

        public delegate void SwiftTestPluginLocationDidSend(string coordinates, string location);
        [DllImport("__Internal")]
        private static extern void setSwiftTestPluginLocationDidSend(SwiftTestPluginLocationDidSend callBack);
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
        #endregion

        [RuntimeInitializeOnLoadMethod]
        public static void Initialize()
        {
            setSwiftTestPluginVideoDidSaveLocation(onGetVideoPath);
            setSwiftTestPluginLocationDidSend(onGetFullLocation);
        }

        public static void OnPressCameraButton()
        {
            startSwiftCameraControllerLocation();
        }

        public static void OnPressMapButton()
        {
            startSwiftMapController();
        }

        public static bool TryGetMyLocation(out Location location)
        {
            location = new Location();
            var locText = GetLastKnownLocation();
            //Log: Answer from plugin: Optional(41.7217979394288),Optional(44.76156402383443)
            if (string.IsNullOrEmpty(locText) || locText.Contains("nil"))
            {
                return false;
            }

            if (locText.Contains("Optional"))
            {
                int startLat = Mathf.Clamp(locText.IndexOf("Optional(", 0) + 9, 0, int.MaxValue);
                int endLat = Mathf.Clamp(locText.IndexOf(")", startLat) - 1, 0, int.MaxValue);
                string latitude = locText.Substring(startLat, endLat - startLat + 1);

                int startLon = Mathf.Clamp(locText.IndexOf(",Optional(", 0) + 10, 0, int.MaxValue);
                int endLon = Mathf.Clamp(locText.IndexOf(")", startLon) - 1, 0, int.MaxValue);
                string longitude = locText.Substring(startLon, endLon - startLon + 1);

                location.latitude = latitude;
                location.longitude = longitude;
                return true;
            }

            return false;
        }

        public static void OnViewMap(string theirLocation)
        {
            startSwiftRouteController(theirLocation);
        }

        public static void OnAllowLocationPermission()
        {
            startTurnGeolocation();
        }
#endif
    }
}
