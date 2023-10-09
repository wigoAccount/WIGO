using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using WIGO.Userinterface;

namespace WIGO.Test
{
    public class UITestVideoWindow : MonoBehaviour
    {
        enum VideoStatus
        {
            Empty,
            Pause,
            Play
        }

        [SerializeField] VideoPlayer _player;
        [SerializeField] RectTransform _mask;
        [SerializeField] RawImage _preview;
        [SerializeField] GameObject _playButton;

        Action _onBackClick;
        RenderTexture _videoTexture;
        Coroutine _videoLoadRoutine;
        VideoStatus _status = VideoStatus.Empty;

        public void Init(Action onBack)
        {
            _onBackClick = onBack;
        }

        public void OnOpen(string path)
        {
            gameObject.SetActive(true);
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                _status = VideoStatus.Empty;
                Debug.LogError("Video path is empty");
                return;
            }

            Debug.Log("Video exists!");
            UIGameColors.SetTransparent(_preview, 1f);
            var size = GetVideoSize(path);
            _videoTexture = new RenderTexture(size.x, size.y, 32);
            _preview.texture = _videoTexture;

            try
            {
                _player.targetTexture = _videoTexture;
                _videoLoadRoutine = StartCoroutine(LoadVideoContent(path));
            }
            catch (Exception)
            {
                _player.targetTexture = null;
                _videoTexture.Release();
                Destroy(_videoTexture);
                _videoTexture = null;
                _status = VideoStatus.Empty;
                _playButton.SetActive(false);
                UIGameColors.SetTransparent(_preview, 0.1f);
            }
            
        }

        public void OnClose()
        {
            if (_player.isPrepared || _player.isPlaying || _player.isPaused)
            {
                _player.Stop();
            }
            
            _player.targetTexture = null;
            if (_videoTexture != null)
            {
                _videoTexture.Release();
                Destroy(_videoTexture);
                _videoTexture = null;
            }
            
            _status = VideoStatus.Empty;
            _playButton.SetActive(true);
            UIGameColors.SetTransparent(_preview, 0.1f);
            gameObject.SetActive(false);
        }

        public void OnBackClick()
        {
            _onBackClick?.Invoke();
        }

        public void OnControlClick()
        {
            switch (_status)
            {
                case VideoStatus.Empty:
                    return;
                case VideoStatus.Pause:
                    _status = VideoStatus.Play;
                    _playButton.SetActive(false);
                    _player.Play();
                    break;
                case VideoStatus.Play:
                    _status = VideoStatus.Pause;
                    _playButton.SetActive(true);
                    _player.Pause();
                    break;
                default:
                    break;
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
            float aspect = (float)size.x / size.y;
            float height = _mask.rect.width / aspect;
            _mask.sizeDelta = new Vector2(_mask.sizeDelta.x, height);
            return size;
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
                Debug.Log("<color=cyan>Start prepare video...</color>");
                _player.errorReceived += OnErrorReceived;
                _player.url = url;
                _player.Prepare();

                while (!_player.isPrepared)
                {
                    yield return null;
                }

                Debug.Log("<color=green>Video loaded</color>");
                _player.Play();
                yield return new WaitForEndOfFrame();
                _player.Pause();
                _status = VideoStatus.Pause;

                _player.errorReceived -= OnErrorReceived;
                _videoLoadRoutine = null;
            }
        }

        IEnumerator StopVideoAfterFrame()
        {
            yield return new WaitForEndOfFrame();
            _player.Pause();
            _status = VideoStatus.Pause;
        }
    }
}
