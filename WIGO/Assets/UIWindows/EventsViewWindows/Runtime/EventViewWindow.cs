using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using WIGO.Core;
using DG.Tweening;

using System.Threading;

namespace WIGO.Userinterface
{
    public enum VideoMode
    {
        Muted = 0,
        SoundOn = 1,
        Paused = 2
    }

    public class EventViewWindow : UIWindow
    {
        [SerializeField] VideoPlayer _videoPlayer;
        [SerializeField] RawImage _preview;
        [SerializeField] GameObject _playButton;
        [SerializeField] GameObject _loader;
        [SerializeField] GameObject _loadingWindow;
        [SerializeField] Image _soundStatusIcon;
        [SerializeField] Sprite[] _soundSprites;
        [SerializeField] CanvasGroup _copiedLabel;

        new EventViewModel _model;
        EventScreenView _view;
        Request _currentCard;
        Sequence _copyAnimation;
        CancellationTokenSource _cts;

        RenderTexture _videoTexture;
        Coroutine _videoLoadRoutine;
        VideoMode _currentMode = VideoMode.Muted;
        bool _fullInfoView;
        float _timer;
        int _seconds;
        bool _myRequest;

        public override void OnBack(WindowId previous, Action callback = null)
        {
            ResetWindow();
            callback?.Invoke();
        }

        public void Setup(Request card, bool isMyRequest)
        {
            _currentCard = card;
            _myRequest = isMyRequest;
            UIGameColors.SetTransparent(_preview, 0.1f);
            SetupCardTextureSize(card.AspectRatio);
            _loader.SetActive(true);
            var videoSize = GetVideoSize(card.AspectRatio);
            _videoTexture = new RenderTexture(videoSize.x, videoSize.y, 32);
            UIGameColors.SetTransparent(_preview, 1f);
            _preview.texture = _videoTexture;
            _fullInfoView = card.GetStatus() == Request.RequestStatus.accept;

            _videoLoadRoutine = StartCoroutine(LoadVideoContent(card.video));
            _view.SetupView(card, isMyRequest);
            _seconds = card.waiting;
            _timer = 0f;
        }

        public void OnBackButtonClick()
        {
            _cts?.Cancel();
            ServiceLocator.Get<UIManager>().CloseCurrent();
        }

        public void OnPlayVideoClick()
        {
            switch (_currentMode)
            {
                case VideoMode.Muted:
                    _videoPlayer.SetDirectAudioMute(0, false);
                    _videoPlayer.time = 0f;
                    _currentMode = VideoMode.SoundOn;
                    _soundStatusIcon.sprite = _soundSprites[1];
                    break;
                case VideoMode.SoundOn:
                    _videoPlayer.Pause();
                    _currentMode = VideoMode.Paused;
                    _playButton.SetActive(true);
                    break;
                case VideoMode.Paused:
                    _videoPlayer.Play();
                    _currentMode = VideoMode.SoundOn;
                    _playButton.SetActive(false);
                    break;
                default:
                    break;
            }
        }

        public async void OnAcceptClick()
        {
            _loadingWindow.SetActive(true);
            var model = ServiceLocator.Get<GameModel>();
            _cts = new CancellationTokenSource();
            _cts.CancelAfter(8000);
            await NetService.TryAcceptOrDeclineRequest(_currentCard.uid, model.GetUserLinks().data.address, true, model.ShortToken, _cts.Token);

            _loadingWindow.SetActive(false);
            if (_cts.IsCancellationRequested)
            {
                _cts.Dispose();
                _cts = null;
                return;
            }

            _cts.Dispose();
            _cts = null;
            _currentCard.status = "accept";
            _fullInfoView = true;
            _view.SetupView(_currentCard, false);
        }

        public async void OnDenyClick()
        {
            _loadingWindow.SetActive(true);
            var model = ServiceLocator.Get<GameModel>();
            _cts = new CancellationTokenSource();
            _cts.CancelAfter(8000);
            await NetService.TryAcceptOrDeclineRequest(_currentCard.uid, model.GetUserLinks().data.address, false, model.ShortToken, _cts.Token);

            _loadingWindow.SetActive(false);
            if (_cts.IsCancellationRequested)
            {
                _cts.Dispose();
                _cts = null;
                return;
            }

            _cts.Dispose();
            _cts = null;
            _currentCard.status = "decline";
            ServiceLocator.Get<UIManager>().CloseCurrent();
        }

        public async void OnRemoveClick()
        {
            _loadingWindow.SetActive(true);
            var model = ServiceLocator.Get<GameModel>();
            _cts = new CancellationTokenSource();
            _cts.CancelAfter(8000);
            string uid = _myRequest ? _currentCard.uid : model.GetMyEventId();
            if (string.IsNullOrEmpty(uid))
            {
                Debug.LogWarningFormat("My event is null");
                _loadingWindow.SetActive(false);
                _cts.Dispose();
                _cts = null;
                return;
            }

            await NetService.TryRemoveEvent(uid, model.GetUserLinks().data.address, _myRequest, model.ShortToken, _cts.Token);

            _loadingWindow.SetActive(false);
            if (_cts.IsCancellationRequested)
            {
                _cts.Dispose();
                _cts = null;
                return;
            }

            _cts.Dispose();
            _cts = null;
            ServiceLocator.Get<UIManager>().CloseCurrent();
        }

        public void OnViewMapClick()
        {
            if (_currentCard.location.Equals(default(Location)))
            {
                Debug.LogError("Card is empty");
                return;
            }

            string myLocation = ServiceLocator.Get<GameModel>().GetMyCurrentLocation().ToString();
            string theirLocation = _currentCard.location.ToString();

            Debug.LogFormat("Open map: {0}", theirLocation);
#if UNITY_IOS && !UNITY_EDITOR
            MessageIOSHandler.OnViewMap(theirLocation);
#endif
        }

        public void OnCopyPhoneNumber()
        {
            string phoneNumber = _currentCard.author == null ? "9998887766" : _currentCard.author.phone;
            GUIUtility.systemCopyBuffer = phoneNumber;
            
            if (_copyAnimation == null)
            {
                _copiedLabel.gameObject.SetActive(true);
                RectTransform label = _copiedLabel.transform as RectTransform;
                label.anchoredPosition = Vector2.down * 48f;
                _copiedLabel.alpha = 0f;

                _copyAnimation = DOTween.Sequence().Append(_copiedLabel.DOFade(1f, 0.28f))
                    .Join(label.DOAnchorPosY(-40f, 0.28f))
                    .AppendInterval(1.5f)
                    .Append(_copiedLabel.DOFade(0f, 0.28f))
                    .OnComplete(() =>
                    {
                        _copiedLabel.gameObject.SetActive(false);
                        _copyAnimation = null;
                    });
            }
        }

        protected override void Awake()
        {
            _model = new EventViewModel();
            _view = GetComponent<EventScreenView>();
            _view.Init(_model);
        }

        private void Update()
        {
            if (!_fullInfoView || _seconds <= 0)
            {
                return;
            }

            _timer += Time.deltaTime;
            if (_timer >= 1f)
            {
                _timer -= 1f;
                _seconds--;
                _view.SetTime(_seconds);
            }
        }

        void ResetWindow()
        {
            if (_videoLoadRoutine != null)
            {
                StopCoroutine(_videoLoadRoutine);
                _videoLoadRoutine = null;
            }

            _soundStatusIcon.sprite = _soundSprites[0];
            _videoPlayer.SetDirectAudioMute(0, true);
            _currentMode = VideoMode.Muted;
            _videoPlayer.Stop();
            _videoPlayer.targetTexture = null;
            _preview.texture = null;
            _videoTexture.Release();
            _loader.SetActive(false);
            CancelAnimation();
        }

        void SetupCardTextureSize(float aspect)
        {
            float cardWidth = _preview.rectTransform.rect.width;
            float cardHeight = cardWidth / aspect;

            _preview.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, cardHeight);
        }

        Vector2Int GetVideoSize(float aspect)
        {
            int width = Screen.width;
            int height = Mathf.RoundToInt(width / aspect);
            return new Vector2Int(width, height);
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

        void CancelAnimation()
        {
            if (_copyAnimation != null)
            {
                _copyAnimation.Kill();
                _copyAnimation = null;
                _copiedLabel.gameObject.SetActive(false);
            }
        }

        IEnumerator LoadVideoContent(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                string path = System.IO.Path.Combine(Application.streamingAssetsPath, url);
                _videoPlayer.targetTexture = _videoTexture;
                _videoPlayer.errorReceived += OnErrorReceived;
                _videoPlayer.url = path;
                _videoPlayer.Prepare();

                while (!_videoPlayer.isPrepared)
                {
                    yield return null;
                }

                _videoPlayer.Play();
                UIGameColors.SetTransparent(_preview, 1f);
                _loader.SetActive(false);

                _videoPlayer.errorReceived -= OnErrorReceived;
                _videoLoadRoutine = null;
            }
        }
    }

    public class EventViewModel : UIWindowModel
    {

    }
}
