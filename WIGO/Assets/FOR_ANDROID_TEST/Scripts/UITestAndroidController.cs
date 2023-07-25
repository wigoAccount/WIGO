using System;
using UnityEngine;

namespace WIGO.Test.Android
{
    public class UITestAndroidController : MonoBehaviour
    {
        [SerializeField] GameObject _startWindow;
        [SerializeField] UITestVideoWindow _resultWindow;
        [SerializeField] string _videoPath;

#if UNITY_ANDROID && !UNITY_EDITOR
        AndroidJavaObject _pluginInstance;

        const string TEST_CHECK_TEXT = "Test check Android: Success";
        const string PLUGIN_NAME = "ru.sysdyn.mylibrary.PluginInstance";
        const string TEST_FUNCTION_NAME = "toast";
        const string CAMERA_FUNCTION_NAME = "startActivity";
        const string INITIALIZE_NAME = "receiveUnityActivity";
        const string CALLBACK_FUNCTION_NAME = "setCallback";
#endif

        public void OnTestClick()
        {
#if UNITY_EDITOR
            Debug.Log("Editor version: OK");
#elif UNITY_ANDROID
            _pluginInstance.Call(TEST_FUNCTION_NAME, TEST_CHECK_TEXT);
#endif
        }

        public void OnCameraClick()
        {
#if UNITY_EDITOR
            string path = System.IO.Path.Combine(Application.streamingAssetsPath, _videoPath);
            OnRecordComplete(path);
#elif UNITY_ANDROID
            _pluginInstance.Call(CALLBACK_FUNCTION_NAME, new AndroidPluginCallback(OnRecordComplete));
            _pluginInstance.Call(CAMERA_FUNCTION_NAME);
#endif
        }

        private void Awake()
        {
            _resultWindow.Init(OnBackClick);
#if UNITY_ANDROID && !UNITY_EDITOR
            InitializePlugin(PLUGIN_NAME);
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        void InitializePlugin(string pluginName)
        {
            using AndroidJavaClass unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            using AndroidJavaObject unityActivity = unityClass.GetStatic<AndroidJavaObject>("currentActivity");
            _pluginInstance = new AndroidJavaObject(pluginName);
            if (_pluginInstance == null)
            {
                Debug.LogError("Error: Plugin instance is NULL");
                return;
            }
            _pluginInstance.CallStatic(INITIALIZE_NAME, unityActivity);
        }
#endif

        void OnBackClick()
        {
            _startWindow.SetActive(true);
            _resultWindow.OnClose();
        }

        void OnRecordComplete(string path)
        {
            Debug.LogFormat("Path: {0}", path);
            _startWindow.SetActive(false);
            _resultWindow.OnOpen(path);
        }

        class AndroidPluginCallback : AndroidJavaProxy
        {
            Action<string> _onPathReceived;

            public AndroidPluginCallback(Action<string> onPathReceived) : base("ru.sysdyn.mylibrary.PluginCallback") 
            {
                _onPathReceived = onPathReceived;
            }

            public void onSuccess(string path)
            {
                _onPathReceived?.Invoke(path);
            }
        }
    }
}
