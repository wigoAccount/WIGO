using System;
using UnityEngine;

#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

#if UNITY_IOS
using System.Collections;
#endif

namespace WIGO.Utility
{
    [Serializable]
    public struct PermissionsSaveData
    {
        public bool cameraOn;
        public bool microphoneOn;
    }

    public static class PermissionsRequestManager
    {
        public static void RequestBothPermissionsAtFirstTime(Action<bool, PermissionsSaveData> callback)
        {
            bool camAllowed = HasCameraPermission();
            bool micAllowed = HasMicrophonePermission();
            PermissionsSaveData newData = new PermissionsSaveData()
            {
                cameraOn = camAllowed,
                microphoneOn = micAllowed
            };

            if (!camAllowed)
            {
                CheckCameraAndMic(micAllowed, callback, newData);
            }
            else if (!micAllowed)
            {
                CheckMicOnly(callback, newData);
            }
            else
                callback?.Invoke(true, newData);
        }

        static void CheckCameraAndMic(bool micAllowed, Action<bool, PermissionsSaveData> callback, PermissionsSaveData newData)
        {
            RequestPermissionCamera((allow) =>
            {
                newData.cameraOn = allow;
                if (!allow)
                {
                    callback?.Invoke(false, newData);
                    return;
                }

                if (!micAllowed)
                    CheckMicOnly(callback, newData);
                else
                    callback?.Invoke(true, newData);
            });
        }

        static void CheckMicOnly(Action<bool, PermissionsSaveData> callback, PermissionsSaveData newData)
        {
            RequestPermissionMicrophone((isMicAllow) =>
            {
                newData.microphoneOn = isMicAllow;
                callback?.Invoke(isMicAllow, newData);
            });
        }

        #region PERMISSIONS CAMERA
        public static bool HasCameraPermission()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return Permission.HasUserAuthorizedPermission(Permission.Camera);
#elif UNITY_IOS && !UNITY_EDITOR
            return Application.HasUserAuthorization(UserAuthorization.WebCam);
#else
            return true;
#endif
        }

        public static void RequestPermissionCamera(Action<bool> callback)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            CheckPermissionsAndroidCam(callback);
#elif UNITY_IOS && !UNITY_EDITOR
            ServiceLocator.Get<CoroutineDispatcher>().StartCoroutine(CheckPermissionsIOSCam(callback));
#else
            callback?.Invoke(true);
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        static void CheckPermissionsAndroidCam(Action<bool> callback)
        {
            if (HasCameraPermission())
            {
                callback?.Invoke(true);
                return;
            }

            var callbacks = new PermissionCallbacks();
            callbacks.PermissionDenied += (permissionName) =>
            {
                UnityEngine.Debug.LogError($"{permissionName} Denied");
                callback?.Invoke(false);
            };
            callbacks.PermissionGranted += (permissionName) =>
            {
                UnityEngine.Debug.Log($"<color=green>{permissionName} Granted</color>");
                callback?.Invoke(true);
            };

            Permission.RequestUserPermission(Permission.Camera, callbacks);
        }
#endif

#if UNITY_IOS && !UNITY_EDITOR

        static IEnumerator CheckPermissionsIOSCam(Action<bool> callback)
        {
            if (HasCameraPermission())
            {
                callback?.Invoke(true);
                yield break;
            }

            yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
            if (Application.HasUserAuthorization(UserAuthorization.WebCam))
            {
                UnityEngine.Debug.Log("Webcam found");
                callback?.Invoke(true);
            }
            else
            {
                UnityEngine.Debug.Log("Have no camera permission");
                callback?.Invoke(false);
            }
        }
#endif
        #endregion

        #region PERMISSIONS MICROPHONE
        public static bool HasMicrophonePermission()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return Permission.HasUserAuthorizedPermission(Permission.Microphone);
#elif UNITY_IOS && !UNITY_EDITOR
            return Application.HasUserAuthorization(UserAuthorization.Microphone);
#else
            return true;
#endif
        }

        public static void RequestPermissionMicrophone(Action<bool> callback)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            CheckPermissionsAndroidMic(callback);
#elif UNITY_IOS && !UNITY_EDITOR
            ServiceLocator.Get<CoroutineDispatcher>().StartCoroutine(CheckPermissionsIOSMic(callback));
#else
            callback?.Invoke(true);
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        static void CheckPermissionsAndroidMic(Action<bool> callback)
        {
            if (HasMicrophonePermission())
            {
                callback?.Invoke(true);
                return;
            }

            var callbacks = new PermissionCallbacks();
            callbacks.PermissionDenied += (permissionName) =>
            {
                UnityEngine.Debug.LogError($"{permissionName} Denied");
                callback?.Invoke(false);
            };
            callbacks.PermissionGranted += (permissionName) =>
            {
                UnityEngine.Debug.Log($"<color=green>{permissionName} Granted</color>");
                callback?.Invoke(true);
            };

            Permission.RequestUserPermission(Permission.Microphone, callbacks);
        }
#endif

#if UNITY_IOS && !UNITY_EDITOR
        static IEnumerator CheckPermissionsIOSMic(Action<bool> callback)
        {
            if (HasMicrophonePermission())
            {
                callback?.Invoke(true);
                yield break;
            }

            yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
            if (Application.HasUserAuthorization(UserAuthorization.Microphone))
            {
                UnityEngine.Debug.Log("Microphone found");
                callback?.Invoke(true);
            }
            else
            {
                UnityEngine.Debug.Log("Have no microphone permission");
                callback?.Invoke(false);
            }
        }
#endif
        #endregion

        #region PERMISSIONS_LOCATION
        public static bool HasLocationPermission()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return Permission.HasUserAuthorizedPermission(Permission.FineLocation);
#elif UNITY_IOS && !UNITY_EDITOR
            return Input.location.isEnabledByUser;
#else
            return true;
#endif
        }

        public static void RequestPermissionLocation(Action<bool> callback)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            CheckPermissionsAndroidLocation(callback);
#elif UNITY_IOS && !UNITY_EDITOR
            ServiceLocator.Get<CoroutineDispatcher>().StartCoroutine(CheckPermissionsIOSLocation(callback));
#else
            callback?.Invoke(true);
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        static void CheckPermissionsAndroidLocation(Action<bool> callback)
        {
            if (HasLocationPermission())
            {
                callback?.Invoke(true);
                return;
            }

            var callbacks = new PermissionCallbacks();
            callbacks.PermissionDenied += (permissionName) =>
            {
                UnityEngine.Debug.LogError($"{permissionName} Denied");
                callback?.Invoke(false);
            };
            callbacks.PermissionGranted += (permissionName) =>
            {
                UnityEngine.Debug.Log($"<color=green>{permissionName} Granted</color>");
                callback?.Invoke(true);
            };

            Permission.RequestUserPermission(Permission.FineLocation, callbacks);
        }
#endif

#if UNITY_IOS && !UNITY_EDITOR
        static IEnumerator CheckPermissionsIOSLocation(Action<bool> callback)
        {
            if (HasLocationPermission())
            {
                callback?.Invoke(true);
                yield break;
            }

            Input.location.Start(1f, 5f);
            int maxWait = 1200;
            while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
            {
                yield return null;
                maxWait--;
            }

            if (maxWait < 1)
            {
                Debug.Log("Timed out location");
                callback?.Invoke(false);
                yield break;
            }

            if (Input.location.status == LocationServiceStatus.Failed)
            {
                Debug.Log("Unable to determine device location");
                callback?.Invoke(false);
                yield break;
            }
        }
#endif
        #endregion
    }
}
