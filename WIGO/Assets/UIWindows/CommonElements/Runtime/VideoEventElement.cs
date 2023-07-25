using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;
using UnityEngine.Video;

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

        const int VIDEO_SIZE = 720;

        private void Awake()
        {
            _cardRect = transform as RectTransform;
        }

        public virtual void OnVideoClick()
        {
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
            _player.SetDirectAudioMute(0, true);
            _player.Stop();
            _player.targetTexture = null;
            _videoTexture.texture = null;
            _renderTexture.Release();
        }

        public virtual void SetupVideo(string path, float aspect)
        {
            if (_cardRect == null)
            {
                _cardRect = transform as RectTransform;
            }

            _currentMode = VideoMode.Muted;
            string fullPath = System.IO.Path.Combine(Application.streamingAssetsPath, path);
            _player.url = fullPath;

            int height = Mathf.RoundToInt(VIDEO_SIZE / aspect);

            _renderTexture = RenderTexture.GetTemporary(VIDEO_SIZE, height, 32, GraphicsFormat.R16G16B16A16_SFloat);
            _renderTexture.Create();
            //RenderTexture.active = _renderTexture;
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
            _player.Play();
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
            //if (_player.isPrepared)
            //{
                _player.Pause();
                _player.time = 0f;
                _player.SetDirectAudioMute(0, true);
                _currentMode = VideoMode.Muted;
                _soundStatusIcon.sprite = _soundSprites[0];
            //}
        }
    }
}
