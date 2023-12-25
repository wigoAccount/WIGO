using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using WIGO.Core;
using DG.Tweening;
using System.Linq;
using System.Threading;
using WIGO.Utility;

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
        Request _request;
        AbstractEvent _currentCard;
        Sequence _copyAnimation;
        CancellationTokenSource _cts;
        Action<bool> _onMarkAsWatched;

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

        public void Setup(Request request, bool isMyRequest, Action<bool> onMarkAsWatched)
        {
            _request = request;
            _currentCard = isMyRequest ? (AbstractEvent)request.@event : request;
            _myRequest = isMyRequest;
            _onMarkAsWatched = onMarkAsWatched;
            UIGameColors.SetTransparent(_preview, 0.1f);
            SetupCardTextureSize(_currentCard.AspectRatio);
            _loader.SetActive(true);
            var videoSize = GetVideoSize(_currentCard.AspectRatio);
            _videoTexture = new RenderTexture(videoSize.x, videoSize.y, 32);
            UIGameColors.SetTransparent(_preview, 1f);
            _preview.texture = _videoTexture;
            _fullInfoView = request.GetStatus() == Request.RequestStatus.accept;

            string path = ServiceLocator.Get<S3ContentClient>().GetPublicVideoUrl(_currentCard.video);
            _videoLoadRoutine = StartCoroutine(LoadVideoContent(path));
            _view.SetupView(request, isMyRequest);
            _seconds = request.time_to;
            _timer = 0f;
            CheckNewRequest();
        }

        async void CheckNewRequest()
        {
            var model = ServiceLocator.Get<GameModel>();
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(5000);
            if (!_myRequest && !_request.IsWatched())
            {
                await NetService.TryMarkAbstractEventAsWatched(_currentCard.uid, false, model.GetUserLinks().data.address, model.ShortToken, cts.Token);
                if (!cts.IsCancellationRequested)
                {
                    _request.watched = "true";
                    model.DecreaseUpdatesCounter(false);
                    _onMarkAsWatched?.Invoke(false);
                }
            }
            else if (_myRequest && _request.GetStatus() == Request.RequestStatus.accept && model.GetUnreadEventsCount(true) > 0)
            {
                await NetService.TryMarkAbstractEventAsWatched(_currentCard.uid, true, model.GetUserLinks().data.address, model.ShortToken, cts.Token);
                if (!cts.IsCancellationRequested)
                {
                    _request.status = "accept";
                    model.DecreaseUpdatesCounter(true);
                    _onMarkAsWatched?.Invoke(true);
                }
            }

            cts.Dispose();
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

        // This is only for MY EVENT and THEIR REQUEST
        public async void OnAcceptClick()
        {
            _loadingWindow.SetActive(true);
            var model = ServiceLocator.Get<GameModel>();
            _cts = new CancellationTokenSource();
            _cts.CancelAfter(8000);
            var myEvent = await NetService.TryAcceptOrDeclineRequest(_request.uid, model.GetUserLinks().data.address, true, model.ShortToken, _cts.Token);

            _loadingWindow.SetActive(false);
            if (_cts.IsCancellationRequested)
            {
                _cts.Dispose();
                _cts = null;
                return;
            }

            _cts.Dispose();
            _cts = null;

            if (myEvent != null)
            {
                model.SetMyEvent(myEvent);
                var oldUid = _request.uid;
                _request = myEvent.requests.FirstOrDefault(x => x.uid == oldUid);
                
                if (_request == null)
                {
                    Debug.LogErrorFormat("Accepted request '{0}' isn't included in my Event", oldUid);
                    return;
                }

                _currentCard = _myRequest ? (AbstractEvent)_request.@event : _request;
                _fullInfoView = true;
                _view.SetupView(_request, false);
            }
        }

        // This is only for MY EVENT and THEIR REQUEST
        public async void OnDenyClick()
        {
            _loadingWindow.SetActive(true);
            var model = ServiceLocator.Get<GameModel>();
            _cts = new CancellationTokenSource();
            _cts.CancelAfter(8000);
            var myEvent = await NetService.TryAcceptOrDeclineRequest(_request.uid, model.GetUserLinks().data.address, false, model.ShortToken, _cts.Token);

            _loadingWindow.SetActive(false);
            if (_cts.IsCancellationRequested)
            {
                _cts.Dispose();
                _cts = null;
                return;
            }

            _cts.Dispose();
            _cts = null;

            if (myEvent != null)
            {
                model.SetMyEvent(myEvent);
                _request.status = "decline";
                ServiceLocator.Get<UIManager>().CloseCurrent();
            }
        }

        public async void OnRemoveClick()
        {
            _loadingWindow.SetActive(true);
            var model = ServiceLocator.Get<GameModel>();
            _cts = new CancellationTokenSource();
            _cts.CancelAfter(8000);
            string uid = _myRequest ? _request.uid : model.GetMyEventId();
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

            string theirLocation = _currentCard.location.ToString();
            if (_myRequest)
            {
#if UNITY_IOS && !UNITY_EDITOR
                MessageIOSHandler.OnViewMap(theirLocation);
#endif
            }
            else
            {
                var myEventLocation = ServiceLocator.Get<GameModel>().GetLocationFromMyEvent();
                if (string.IsNullOrEmpty(myEventLocation))
                {
                    Debug.LogError("Fail get my event location. Event seems to be null");
                    return;
                }

#if UNITY_IOS && !UNITY_EDITOR
                MessageIOSHandler.OnViewGuestRoute(myEventLocation, theirLocation);
#endif
            }
        }

        public void OnCopyPhoneNumber()
        {
            string phoneNumber = _currentCard == null ? "None" : string.Format("+7{0}", _currentCard.phone);
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
                _videoPlayer.targetTexture = _videoTexture;
                _videoPlayer.errorReceived += OnErrorReceived;
                _videoPlayer.url = url;
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
