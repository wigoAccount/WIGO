using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using WIGO.Core;
using WIGO.Utility;

using Event = WIGO.Core.Event;
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
        [SerializeField] EventCard _tempCard;

        EventViewModel _model;
        EventScreenView _view;
        AbstractEvent _currentCard;

        RenderTexture _videoTexture;
        Coroutine _videoLoadRoutine;
        VideoMode _currentMode = VideoMode.Muted;
        bool _fullInfoView;
        float _timer;
        int _seconds;

        public override void OnBack(WindowId previous, Action callback = null)
        {
            ResetWindow();
            callback?.Invoke();
        }

        public void Setup(AbstractEvent card)
        {
            //card = _tempCard;       // remove
            _currentCard = card;
            UIGameColors.SetTransparent(_preview, 0.1f);
            SetupCardTextureSize(card.AspectRatio);
            _loader.SetActive(true);
            var videoSize = GetVideoSize(card.AspectRatio);
            _videoTexture = new RenderTexture(videoSize.x, videoSize.y, 32);
            UIGameColors.SetTransparent(_preview, 1f);
            _preview.texture = _videoTexture;
            _fullInfoView = card.IsResponse() 
                ? ((Request)card).GetStatus() == Request.RequestStatus.accept 
                : ((Event)card).GetStatus() == Event.EventStatus.active;

            _videoLoadRoutine = StartCoroutine(LoadVideoContent(card.video));
            _view.SetView(card);
            _seconds = card.waiting;
            _timer = 0f;
        }

        public void OnBackButtonClick()
        {
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
            // wait server response
            _loadingWindow.SetActive(true);
            await Task.Delay(1000);

            _loadingWindow.SetActive(false);
            //_currentCard.UpdateStatus(EventStatus.Accepted);
            _fullInfoView = true;
            _view.SetView(_currentCard);
        }

        public async void OnDenyClick()
        {
            // wait server response
            _loadingWindow.SetActive(true);
            await Task.Delay(1000);

            _loadingWindow.SetActive(false);
            //_currentCard.UpdateStatus(EventStatus.Denied);
            ServiceLocator.Get<UIManager>().CloseCurrent();
        }

        public void OnViewMapClick()
        {
            string myLocation = ServiceLocator.Get<GameModel>().GetMyCurrentLocation().ToString();
            Location loc = string.IsNullOrEmpty(_currentCard.location.longitude)
                ? new Location() { longitude = "14.88", latitude = "29.2929" }
                : _currentCard.location;
            string theirLocation = loc.ToString();

            Debug.LogFormat("Open map: {0}", theirLocation);
#if UNITY_IOS && !UNITY_EDITOR
            MessageIOSHandler.OnViewMap(theirLocation);
#endif
        }

        public void OnCopyPhoneNumber()
        {
            string phoneNumber = _currentCard.author == null ? "9998887766" : _currentCard.author.phone;
            GUIUtility.systemCopyBuffer = phoneNumber;
            // [TODO]: show popup
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

        IEnumerator LoadVideoContent(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                string path = System.IO.Path.Combine(Application.streamingAssetsPath, url);
                _videoPlayer.targetTexture = _videoTexture;
                _videoPlayer.errorReceived += OnErrorReceived;
                _videoPlayer.url = path;
                //_videoPlayer.loopPointReached += (player) =>
                //{
                //    player.Stop();
                //    _playButton.SetActive(true);
                //};
                _videoPlayer.Prepare();

                while (!_videoPlayer.isPrepared)
                {
                    yield return null;
                }

                _videoPlayer.Play();
                //yield return new WaitForEndOfFrame();
                //_videoPlayer.Pause();

                UIGameColors.SetTransparent(_preview, 1f);
                _loader.SetActive(false);
                //_playButton.SetActive(true);

                _videoPlayer.errorReceived -= OnErrorReceived;
                _videoLoadRoutine = null;
            }
        }
    }

    public class EventViewModel : UIWindowModel
    {

    }
}
