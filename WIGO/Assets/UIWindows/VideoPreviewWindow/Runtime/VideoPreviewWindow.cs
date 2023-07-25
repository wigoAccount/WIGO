using System.Collections;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Threading.Tasks;

namespace WIGO.Userinterface
{
    public class VideoPreviewWindow : UIWindow
    {
        [SerializeField] VideoPlayer _videoPlayer;
        [SerializeField] RectTransform _maskBounds;
        [SerializeField] RawImage _preview;
        [SerializeField] GameObject _playButton;
        [SerializeField] GameObject _loader;

        RenderTexture _videoTexture;
        Coroutine _videoLoadRoutine;
        string _videoPath;
        bool _isPlaying;
        bool _isResponse;

        const float UPPER_DEFAULT_PADDING = 56f;
        const float BOTTOM_DEFAULT_PADDING = 116f;

        public override void OnBack(WindowId previous, Action callback = null)
        {
            ClearData();
            callback?.Invoke();
        }

        public override void CloseUnactive()
        {
            ClearData();
        }

        public async void Setup(string path, bool response)
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("Path is empty");
                return;
            }
			
			if (path.StartsWith("\"file"))
            {
                int found = Mathf.Clamp(path.IndexOf("var") - 1, 0, int.MaxValue);
                _videoPath = path.Substring(found).Replace("\"", "").Replace(@"\", "");
            }
			else
				_videoPath = path;
			
			if (!File.Exists(_videoPath))
            {
                Debug.LogErrorFormat("Can't find video at path: {0}\r\nOriginal: {1}", _videoPath, path);
                return;
            }

            _isResponse = response;
            //_videoPath = path;
            _playButton.SetActive(false);
            _loader.SetActive(true);
            await Task.Delay(400);
            _loader.SetActive(false);
            _playButton.SetActive(true);

            var videoSize = GetVideoSize(_videoPath);
            SetupCardTextureSize(videoSize.x, videoSize.y);
            _videoTexture = new RenderTexture(videoSize.x, videoSize.y, 32);
            UIGameColors.SetTransparent(_preview, 1f);
            _preview.texture = _videoTexture;

            StartCoroutine(LoadVideoContent(_videoPath));
        }

        public void OnBackButtonClick()
        {
            ServiceLocator.Get<UIManager>().CloseCurrent();
        }

        public void OnContinueClick()
        {
            _videoPlayer.Stop();
            _isPlaying = false;
            _playButton.SetActive(true);
            if (_isResponse)
                ServiceLocator.Get<UIManager>().Open<SaveEventWindow>(WindowId.SAVE_EVENT_SCREEN, (window) => window.Setup(_videoPath));
            else
                ServiceLocator.Get<UIManager>().Open<CreateEventWindow>(WindowId.CREATE_EVENT_SCREEN, (window) => window.Setup(_videoPath));
        }

        public void OnPlayControlClick()
        {
            _playButton.SetActive(_isPlaying);
            if (_isPlaying)
            {
                _videoPlayer.Pause();
            }
            else
            {
                _videoPlayer.Play();
            }

            _isPlaying = !_isPlaying;
        }

        void SetupCardTextureSize(int width, int height)
        {
            float aspect = (float)width / height;
            
            float cardWidth = _preview.rectTransform.rect.width;
            float cardHeight = cardWidth / aspect;

            _preview.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, cardHeight);
            float screenHeight = ServiceLocator.Get<UIManager>().GetCanvasSize().y;
            float maskHeight = _maskBounds.rect.height > cardHeight 
                ? cardHeight 
                : Mathf.Min(screenHeight - UPPER_DEFAULT_PADDING - BOTTOM_DEFAULT_PADDING, cardHeight);
            _maskBounds.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, maskHeight);
        }

        void ClearData()
        {
            if (_videoTexture != null)
            {
                _videoPlayer.Stop();
                _videoTexture.Release();
                Destroy(_videoTexture);
                _videoTexture = null;
                _videoPath = null;
                _isPlaying = false;
                _playButton.SetActive(true);
                UIGameColors.SetTransparent(_preview, 0.1f);
            }
        }

        Vector2Int GetVideoSize(string path)
        {
            var size = new Vector2Int();
#if UNITY_EDITOR
            size = new Vector2Int(720, 1280);
#elif UNITY_ANDROID || UNITY_IOS
            var info = NativeCamera.GetVideoProperties(path);
            size = new Vector2Int(info.width, info.height);
#endif
            return size;
        }

        void OnErrorReceived(VideoPlayer vp, string message)
        {
            _videoPlayer.errorReceived -= OnErrorReceived;
            if (_videoLoadRoutine != null)
            {
                StopCoroutine(_videoLoadRoutine);
                _videoLoadRoutine = null;
            }

            Debug.LogFormat("<color=orange>Error received: {0}</color>", message);
        }

        IEnumerator LoadVideoContent(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                _videoPlayer.targetTexture = _videoTexture;
                _videoPlayer.errorReceived += OnErrorReceived;
                _videoPlayer.url = url;
                _videoPlayer.loopPointReached += (player) =>
                {
                    player.Stop();
                    _isPlaying = false;
                    _playButton.SetActive(true);
                };
                _videoPlayer.Prepare();

                while (!_videoPlayer.isPrepared)
                {
                    yield return null;
                }

                _videoPlayer.Play();
                yield return null;// new WaitForEndOfFrame();
                _videoPlayer.Pause();

                _videoPlayer.errorReceived -= OnErrorReceived;
                _videoLoadRoutine = null;
            }
        }
    }
}
