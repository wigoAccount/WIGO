using System;
using UnityEngine;
using UnityEngine.UI;
//using NatML.Devices;
//using NatML.Devices.Outputs;
using NatSuite.Recorders;
using NatSuite.Recorders.Clocks;
using System.Threading.Tasks;
using System.IO;
using WIGO.Core;
using System.Collections;

#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

namespace WIGO.Userinterface
{
    public class RecordEventWindow : UIWindow
    {
//        [SerializeField] RecordEventView _view;
//        [SerializeField] RawImage _camImage;
//        [SerializeField] RecordUIButton _recordButton;
//        [SerializeField] WindowAnimator _animator;

//        //CameraDevice _currentCamera;
//        //AudioDevice _currentMicrophone;
//        MP4Recorder _recorder;
//        Texture2D _streamingTexture;
//        Coroutine _recordCoroutine;
//        Color32[] _committedBytes;
//        int _framesCounter;
//        int _recorderedFrames;
//        float _recordTimer;
//        bool _recording;
//        bool _isResponse;
//        bool _closing;
//        bool _isFrontFacing;

//        const int FPS = 30;
//        const int QUALITY = 720;

//        public override void OnOpen(WindowId previous)
//        {
//            _animator.OnOpen();
//#if UNITY_ANDROID && !UNITY_EDITOR
//            CheckPermissionsAndroid();
//#elif UNITY_IOS
//            StartCoroutine(CheckPermissionsIOS());
//#else
//            SetupNatDevice();
//#endif
//        }

//        public override void OnReopen(WindowId previous, UIWindowModel cachedModel)
//        {
//            _animator.OnReopen();
//            SetupNatDevice();
//        }

//        public void Setup(bool response)
//        {
//            _isResponse = response;
//            _closing = false;

//            if (!response)
//            {
//                var myEvent = EventCard.CreateEmpty();
//                ServiceLocator.Get<GameModel>().SetMyEvent(myEvent);
//            }
//        }

//        public void OnRecordStart()
//        {
//            //if (!_recording)
//            //{
//            //    _recordButton.StartRecordMode();
//            //    _recordButton.SetTimeText(GameConsts.RECORD_VIDEO_SECONDS);
//            //    _recordCoroutine = StartCoroutine(DelayStartRecord());
//            //    //OnStartRecordVideo();
//            //}
//            ServiceLocator.Get<UIManager>().Open<VideoPreviewWindow>(WindowId.VIDEO_PREVIEW_SCREEN,
//                (window) => window.Setup(null, /*new Vector2Int(800, 1600),*/ _isResponse));
//        }

//        IEnumerator DelayStartRecord()
//        {
//            yield return new WaitForSeconds(0.6f);
//            OnStartRecordVideo();
//            _recordCoroutine = null;
//        }

//        public void OnRecordStop()
//        {
//            if (_recordCoroutine != null)
//            {
//                _recordButton.StopRecordMode();
//                StopCoroutine(_recordCoroutine);
//                _recordCoroutine = null;
//            }
//            _recording = false;
//        }

//        public void OnBackButtonClick()
//        {
//            if (_recording)
//            {
//                _recording = false;
//                _closing = true;
//                _recordButton.ResetButton();
//            }
//            else
//            {
//                ClearTexture();
//            }

//            ServiceLocator.Get<UIManager>().SwitchTo(WindowId.FEED_SCREEN);
//        }

//        public async void OnSwitchCamera()
//        {
//            _isFrontFacing = !_isFrontFacing;
//            ClearTexture();
//            await SetupNatCamera();
//        }

//        async void SetupNatDevice()
//        {
//            _isFrontFacing = true;
//            await SetupNatCamera();

//            var queryMic = new MediaDeviceQuery(MediaDeviceCriteria.AudioDevice);
//            _currentMicrophone = queryMic.current as AudioDevice;

//            if (_currentMicrophone == null)
//            {
//                Debug.LogError("Current mic is null");
//            }
//        }

//        async Task SetupNatCamera()
//        {
//#if UNITY_EDITOR
//            var camFilter = MediaDeviceCriteria.CameraDevice;
//#elif UNITY_IOS || UNITY_ANDROID
//            var camFilter = _isFrontFacing ? MediaDeviceCriteria.FrontCamera : MediaDeviceCriteria.CameraDevice;
//#endif
//            var query = new MediaDeviceQuery(camFilter);
//            _currentCamera = query.current as CameraDevice;

//            if (_currentCamera == null)
//            {
//                Debug.LogError("Current camera is null");
//                return;
//            }

//            _view.SetActiveLoader(true);
//            SetCameraQuality();

//            var textureOutput = new TextureOutput();
//            _currentCamera.StartRunning(textureOutput);

//            _streamingTexture = await textureOutput;
//            SetTexture();
//            _view.SetActiveLoader(false);
//        }

//        void SetCameraQuality()
//        {
//            var resolution = _currentCamera.previewResolution;
//            Debug.LogFormat("Default resolution: {0} x {1}", resolution.width, resolution.height);
//            float aspect = (float)resolution.width / resolution.height;
//            int width = Mathf.Min(resolution.width, QUALITY);
//            int height = Mathf.FloorToInt(width / aspect);
//            _currentCamera.previewResolution = (width, height);
//        }

//        void SetTexture()
//        {
//            float aspect = (float)_streamingTexture.width / _streamingTexture.height;
//            float cardHeight = _camImage.rectTransform.rect.width / aspect;
//            _camImage.rectTransform.sizeDelta = new Vector2(_camImage.rectTransform.sizeDelta.x, cardHeight);
//            _view.AdaptMaskBounds(cardHeight);
//            _camImage.texture = _streamingTexture;
//            UIGameColors.SetTransparent(_camImage, 1f);
//        }

//        void ClearTexture()
//        {
//            _currentCamera.StopRunning();
//            Destroy(_streamingTexture);
//            _camImage.texture = null;
//            UIGameColors.SetTransparent(_camImage, 0.05f);
//            _currentCamera = null;
//        }

//        async void OnStartRecordVideo()
//        {
//            _view.SetActiveHeader(false);

//            _recording = true;
//            _recorder = new MP4Recorder(_streamingTexture.width, _streamingTexture.height, FPS, 
//                _currentMicrophone.sampleRate, _currentMicrophone.channelCount);
//            //_recordButton.StartRecordMode();
//            _framesCounter = 0;
//            _recorderedFrames = 0;
//            _recordTimer = 0f;
//            Application.targetFrameRate = FPS;

//            await Record();
//            FinishWriting();
//        }

//        async void FinishWriting()
//        {
//            var path = await StopRecordingProcess();

//            if (string.IsNullOrEmpty(path))
//            {
//                return;
//            }

//            if (_closing)
//            {
//                if (File.Exists(path))
//                    File.Delete(path);

//                ClearTexture();
//                Application.targetFrameRate = 60;
//                return;
//            }

//            ClearTexture();
//            Application.targetFrameRate = 60;
//            ServiceLocator.Get<UIManager>().Open<VideoPreviewWindow>(WindowId.VIDEO_PREVIEW_SCREEN, 
//                (window) => window.Setup(path, /*new Vector2Int(_streamingTexture.width, _streamingTexture.height),*/ _isResponse));
//        }

//        async Task<string> StopRecordingProcess()
//        {
//            _recording = false;
//            _recordButton.StopRecordMode();
//            _currentMicrophone?.StopRunning();
//            _view.SetActiveHeader(true, false);

//            var path = await _recorder.FinishWriting();
//            Debug.LogFormat("Record complete: {0}", path);
//            return _recordTimer < 0.8f ? null : path;
//        }

//        void OnSaveFile(string path)
//        {
//            var filename = Path.GetFileName(path);
//#if UNITY_EDITOR || UNITY_STANDALONE
//            var newPath = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
//            newPath = Path.Combine(newPath, filename);
//            File.Copy(path, newPath, true);
//            if (File.Exists(path))
//                File.Delete(path);

//#elif UNITY_ANDROID || UNITY_IOS
//            NativeGallery.SaveVideoToGallery(path, "WIGO", filename, (successs, newPath) =>
//            {
//                if (successs)
//                {
//                    if (File.Exists(path))
//                        File.Delete(path);
//                }
//            });
//#endif
//        }

//        async Task Record()
//        {
//            var clock = new RealtimeClock();
//            _currentMicrophone.StartRunning(audioBuffer => _recorder.CommitSamples(audioBuffer.sampleBuffer.ToArray(), clock.timestamp));
//            _recordButton.SetTimeText(GameConsts.RECORD_VIDEO_SECONDS);

//            await Task.Run(() =>
//            {
//                while (_recording)
//                {
//                    if (_recorderedFrames < _framesCounter)
//                    {
//                        _recorder.CommitFrame(_committedBytes, clock.timestamp);
//                        _recorderedFrames = _framesCounter;
//                    }
//                }
//            });
//        }

//        private void Update()
//        {
//            if (_recording)
//            {
//                if (_streamingTexture != null)
//                {
//                    _committedBytes = _streamingTexture.GetPixels32();
//                }

//                _framesCounter++;
//                _recordTimer += Time.deltaTime;
//                int seconds = Mathf.CeilToInt(GameConsts.RECORD_VIDEO_SECONDS - _recordTimer);
//                float progress = _recordTimer / GameConsts.RECORD_VIDEO_SECONDS;
//                _recordButton.SetTimeText(seconds);
//                _recordButton.SetProgress(progress);

//                if (_recordTimer >= GameConsts.RECORD_VIDEO_SECONDS)
//                {
//                    _recording = false;
//                }
//            }
//        }

//        #region PERMISSIONS
//#if UNITY_ANDROID && !UNITY_EDITOR
//        void CheckPermissionsAndroid()
//        {
//            if (Permission.HasUserAuthorizedPermission(Permission.Camera) && Permission.HasUserAuthorizedPermission(Permission.Microphone))
//            {
//                SetupNatDevice();
//                return;
//            }

//            var callbacks = new PermissionCallbacks();
//            callbacks.PermissionDenied += PermissionCallbackDenied;
//            callbacks.PermissionGranted += PermissionCallbackGranted;

//            Permission.RequestUserPermission(Permission.Camera, callbacks);
//        }

//        void PermissionCallbackDenied(string permissionName)
//        {
//            Debug.Log($"<color=red>{permissionName} Denied</color>");
//            ServiceLocator.Get<UIManager>().SwitchTo(WindowId.FEED_SCREEN);
//        }

//        void PermissionCallbackGranted(string permissionName)
//        {
//            Debug.Log($"<color=green>{permissionName} Granted</color>");

//            var callbacks = new PermissionCallbacks();
//            callbacks.PermissionDenied += (name) =>
//            {
//                Debug.Log($"<color=red>{name} Denied</color>");
//                ServiceLocator.Get<UIManager>().SwitchTo(WindowId.FEED_SCREEN);
//            };

//            callbacks.PermissionGranted += (name) =>
//            {
//                Debug.Log($"<color=green>{permissionName} Granted</color>");
//                SetupNatDevice();
//            };
//            Permission.RequestUserPermission(Permission.Microphone, callbacks);
//        }
//#endif

//#if UNITY_IOS
//        IEnumerator CheckPermissionsIOS()
//        {
//            yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
//            if (Application.HasUserAuthorization(UserAuthorization.WebCam))
//            {
//                Debug.Log("webcam found");
//            }
//            else
//            {
//                Debug.Log("webcam not found");
//            }

//            yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
//            if (Application.HasUserAuthorization(UserAuthorization.Microphone))
//            {
//                Debug.Log("Microphone found");
//                SetupNatDevice();
//            }
//            else
//            {
//                Debug.Log("Microphone not found");
//            }
//        }
//#endif
//        #endregion
    }
}
