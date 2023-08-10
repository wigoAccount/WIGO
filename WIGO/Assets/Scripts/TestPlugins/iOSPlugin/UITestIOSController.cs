using TMPro;
using UnityEngine;
using WIGO.Utility;

namespace WIGO.Test
{
    public class UITestIOSController : MonoBehaviour
    {
        [SerializeField] GameObject _startWindow;
        [SerializeField] UITestVideoWindow _resultWindow;
        [SerializeField] GameObject _locationWindow;
        [SerializeField] TMP_Text _locationResultLabel;
        [SerializeField] TMP_Text _receivedPathLabel;
        [SerializeField] string _iosVideoPath;
        [SerializeField] string _videoPath;

        string _receivedPath;

        public void OnTestClick()
        {
#if UNITY_EDITOR
            Debug.Log("Editor version: OK");
#elif UNITY_IOS
            MessageIOSHandler.OnPressTestButton();
#endif
        }

        public void OnCameraClick()
        {
#if UNITY_EDITOR
            string path = System.IO.Path.Combine(Application.streamingAssetsPath, _videoPath);
            OnRecordComplete(path);
#elif UNITY_IOS
            MessageIOSHandler.OnPressCameraButton();
#endif
        }

        public void OnLocationClick()
        {
#if UNITY_EDITOR
            _locationResultLabel.SetText("My location: -5421.67; 268.1");
            OnDisplayLocation("-5421.67; 268.1");
#elif UNITY_IOS
            MessageIOSHandler.OnPressMapButton();
#endif
        }

        public void OnLocationBackClick()
        {
            _startWindow.SetActive(true);
            _locationWindow.SetActive(false);
        }

        public void OnLoadVideoClick()
        {
            _startWindow.SetActive(false);
#if UNITY_EDITOR
            string path = System.IO.Path.Combine(Application.streamingAssetsPath, _videoPath);
            _resultWindow.OnOpen(path);
#elif UNITY_IOS
            _resultWindow.OnOpen(_receivedPath);
#endif
        }

        private void Awake()
        {
#if UNITY_IOS && !UNITY_EDITOR
            MessageIOSHandler.Initialize();
#endif

            MessageRouter.onMessageReceive += OnReceiveMessage;
            _resultWindow.Init(OnBackClick);
        }

        private void OnApplicationQuit()
        {
            MessageRouter.onMessageReceive -= OnReceiveMessage;
        }

        void OnBackClick()
        {
            _startWindow.SetActive(true);
            _resultWindow.OnClose();
        }

        void OnRecordComplete(string path)
        {
            _receivedPath = path;
            Debug.LogFormat("Received path: {0}", path);
            //_startWindow.SetActive(false);
            //_resultWindow.OnOpen(path);
            _receivedPathLabel.SetText("Video: " + path);
        }

        void OnDisplayLocation(string location)
        {
            _locationResultLabel.SetText($"My location: {location}");
            _startWindow.SetActive(false);
            _locationWindow.SetActive(true);
        }

        void OnReceiveMessage(NativeMessageType type, string message)
        {
            switch (type)
            {
                case NativeMessageType.Video:
                    OnRecordComplete(message);
                    break;
                case NativeMessageType.Location:
                    OnDisplayLocation(message);
                    break;
                case NativeMessageType.MyLocation:
                case NativeMessageType.Other:
                    Debug.LogFormat("Message: {0}", message);
                    break;
                default:
                    break;
            }
        }
    }
}
