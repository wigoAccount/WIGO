using System.Collections;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;
using UnityEngine.Video;
using WIGO.Core;

namespace WIGO.Userinterface
{
    public class VideoEventElement : MonoBehaviour
    {
        protected enum VideoMode
        {
            Muted = 0,
            SoundOn = 1,
            Paused = 2
        }

        [SerializeField] protected VideoPlayer _player;
        [SerializeField] protected RawImage _videoTexture;
        [SerializeField] protected Image _soundStatusIcon;
        [SerializeField] protected Sprite[] _soundSprites;

        protected VideoMode _currentMode = VideoMode.Muted;
        protected RenderTexture _renderTexture;
        protected RectTransform _cardRect;
        Coroutine _videoLoadRoutine;

        const int VIDEO_SIZE = 720;

        private void Awake()
        {
            _cardRect = transform as RectTransform;
        }

        public virtual void OnVideoClick()
        {
            if (!_player.isPrepared)
            {
                return;
            }

            switch (_currentMode)
            {
                case VideoMode.Muted:
                    _player.SetDirectAudioMute(0, false);
                    _player.time = 0f;
                    _currentMode = VideoMode.SoundOn;
                    _soundStatusIcon.sprite = _soundSprites[1];
                    break;
                case VideoMode.SoundOn:
                    _player.Pause();
                    _currentMode = VideoMode.Paused;
                    break;
                case VideoMode.Paused:
                    _player.Play();
                    _currentMode = VideoMode.SoundOn;
                    break;
                default:
                    break;
            }
        }

        public virtual void Clear()
        {
            if (_videoLoadRoutine != null)
            {
                StopCoroutine(_videoLoadRoutine);
                _videoLoadRoutine = null;
            }

            _player.SetDirectAudioMute(0, true);
            _player.Stop();
            _player.targetTexture = null;
            _videoTexture.texture = null;
            _renderTexture?.Release();
        }

        public virtual void SetupVideo(string url, float aspect)
        {
            if (_cardRect == null)
            {
                _cardRect = transform as RectTransform;
            }

            aspect = aspect <= 0f ? 9f / 16f : aspect;
            _currentMode = VideoMode.Muted;
            string path = ServiceLocator.Get<S3ContentClient>().GetVideoURL(url);
            int height = Mathf.RoundToInt(VIDEO_SIZE / aspect);

            _renderTexture = RenderTexture.GetTemporary(VIDEO_SIZE, height, 32, GraphicsFormat.R16G16B16A16_SFloat);
            _renderTexture.Create();
            _player.targetTexture = _renderTexture;
            _videoTexture.texture = _renderTexture;

            float needWidth = _videoTexture.rectTransform.rect.width;
            float needHeight = needWidth / aspect;
            float videoWidth = needWidth;
            float videoHeight = needHeight;
            if (needHeight < _cardRect.rect.height)
            {
                videoHeight = _cardRect.rect.height;
                videoWidth = videoHeight * aspect;
            }

            _videoTexture.rectTransform.sizeDelta = new Vector2(videoWidth, videoHeight);
            _videoLoadRoutine = StartCoroutine(LoadVideoContent(path));
        }

        public void Play()
        {
            _player.Play();
        }

        public void Pause()
        {
            if (_player.isPlaying)
            {
                _player.Pause();
            }
        }

        public void ResetVideo()
        {
            if (_player.isPrepared)
            {
                _player.Pause();
                _player.time = 0f;
                _player.SetDirectAudioMute(0, true);
                _currentMode = VideoMode.Muted;
                _soundStatusIcon.sprite = _soundSprites[0];
            }
        }

        void OnErrorReceived(VideoPlayer vp, string message)
        {
            _player.errorReceived -= OnErrorReceived;
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
                _player.errorReceived += OnErrorReceived;
                _player.url = url;
                _player.Prepare();

                while (!_player.isPrepared)
                {
                    yield return null;
                }

                _player.Play();
                _player.errorReceived -= OnErrorReceived;
                _videoLoadRoutine = null;
            }
        }
    }
}
