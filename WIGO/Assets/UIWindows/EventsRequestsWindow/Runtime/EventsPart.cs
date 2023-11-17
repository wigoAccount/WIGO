using Crystal;
using DG.Tweening;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WIGO.Core;

using Event = WIGO.Core.Event;
namespace WIGO.Userinterface
{
    public class EventsPart : AbstractPart
    {
        [SerializeField] VideoEventElement _videoElement;
        [SerializeField] TextGradient _timerLabelGradient;
        [SerializeField] TMP_Text _timerLabel;
        [SerializeField] TMP_Text _eventDescLabel;
        [SerializeField] TMP_Text _addressLabel;
        [SerializeField] RectTransform _categoriesContent;
        [SerializeField] CategoryEventElement _categoryPrefab;
        [SerializeField] GameObject _emptyEventContent;
        [SerializeField] GameObject _myEventContent;
        [Space]
        [SerializeField] GameObject _loadingWindow;
        [SerializeField] PopupBottomPanel _deleteApprovePanel;
        [SerializeField] Image _overlay;
        [SerializeField] SafeArea _safeArea;

        Event _myEvent;
        float _timer;
        int _remainingSeconds;

        public override void Initialize()
        {
            base.Initialize();
            _deleteApprovePanel.Init(_safeArea.GetSafeAreaBottomPadding(), 
                () => _overlay.DOFade(0f, 0.28f).OnComplete(() => _overlay.gameObject.SetActive(false)));
        }

        public override void SetPartActive(bool active, bool animate = true)
        {
            if (active)
            {
                base.SetPartActive(active, animate);
                _timerLabelGradient.ApplyGradient();

                if (!_loaded)
                {
                    UpdateMyEvent();
                }
                else
                {
                    _videoElement.Play();
                }
            }
            else
            {
                _videoElement.ResetVideo();
                base.SetPartActive(active, animate);
            }
        }

        public override void ResetPart()
        {
            _videoElement.Clear();
            _remainingSeconds = 0;
            _timer = 0f;
            base.ResetPart();
        }

        public void OnDeleteEventClick()
        {
            UIGameColors.SetTransparent(_overlay);
            _overlay.gameObject.SetActive(true);
            _overlay.DOFade(0.88f, 0.28f);
            _deleteApprovePanel.OpenPanel(OnGetDeleteEventAnswer);
        }

        void Update()
        {
            if (_remainingSeconds <= 0)
            {
                return;
            }

            _timer += Time.unscaledDeltaTime;
            if (_timer >= 1f)
            {
                _timer -= 1f;
                _remainingSeconds--;
                UpdateRemainingTime(_remainingSeconds);
            }
        }

        async void UpdateMyEvent()
        {
            _myEventContent.SetActive(false);
            _emptyEventContent.SetActive(false);
            _categoriesContent.DestroyChildren();

            var model = ServiceLocator.Get<GameModel>();
            var cts = new CancellationTokenSource();
            cts.CancelAfter(8000);
            _myEvent = await NetService.TryGetMyEvent(model.GetUserLinks().data.address, model.ShortToken, cts.Token);

            _loaded = true;
            if (_myEvent == null)
            {
                _myEventContent.SetActive(false);
                _emptyEventContent.SetActive(true);
                return;
            }

            _myEventContent.SetActive(true);
            _emptyEventContent.SetActive(false);

            _remainingSeconds = _myEvent.waiting;
            _timer = 0f;
            _eventDescLabel.text = _myEvent.about;
            _addressLabel.text = _myEvent.address;

            foreach (var index in _myEvent.tags)
            {
                string category = model.GetCategoryNameWithIndex(index);
                var categoryBlock = Instantiate(_categoryPrefab, _categoriesContent);
                categoryBlock.Setup(category);
            }

            _videoElement.SetupVideo(_myEvent.video, _myEvent.AspectRatio);
        }

        void DeleteEvent()
        {
            _videoElement.Clear();
            _myEventContent.SetActive(false);
            _emptyEventContent.SetActive(true);
            ServiceLocator.Get<GameModel>().SetMyEvent(null);
        }

        void UpdateRemainingTime(int time)
        {
            int minutes = Mathf.FloorToInt((float)time / 60f);
            int seconds = time - minutes * 60;
            _timerLabel.text = string.Format("00:{0:00}:{1:00}", minutes, seconds);
            _timerLabelGradient.ApplyGradient();
        }

        async void OnGetDeleteEventAnswer(bool value)
        {
            if (value)
            {
                _deleteApprovePanel.gameObject.SetActive(false);
                _overlay.gameObject.SetActive(false);

                _loadingWindow.SetActive(true);
                var model = ServiceLocator.Get<GameModel>();
                var cts = new CancellationTokenSource();
                cts.CancelAfter(8000);
                await NetService.TryRemoveEvent(_myEvent.uid, model.GetUserLinks().data.address, _myEvent.IsResponse(), model.ShortToken, cts.Token);

                _loadingWindow.SetActive(false);
                DeleteEvent();
                ServiceLocator.Get<UIManager>().CloseCurrent();
            }
            else
            {
                _deleteApprovePanel.ClosePanel();
            }
        }
    }
}
