using System.Threading;
using TMPro;
using UnityEngine;
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
        [SerializeField] bool _isEmpty;
        [Space]
        [SerializeField] GameObject _loadingWindow;

        Event _myEvent;
        float _timer;
        int _remainingSeconds;

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

        public async void OnDeleteEventClick()
        {
            _loadingWindow.SetActive(true);
            var model = ServiceLocator.Get<GameModel>();
            var cts = new CancellationTokenSource();
            cts.CancelAfter(8000);
            await NetService.TryRemoveEvent(_myEvent.uid, model.GetUserLinks().data.address, _myEvent.IsResponse(), model.ShortToken, cts.Token);

            _loadingWindow.SetActive(false);
            DeleteEvent();
            ServiceLocator.Get<UIManager>().CloseCurrent();
        }

        void Update()
        {
            if (_remainingSeconds <= 0)
            {
                return;
            }

            _timer += Time.deltaTime;
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
            if (_isEmpty)
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

            float.TryParse(_myEvent.preview, out float aspect);
            _videoElement.SetupVideo(_myEvent.video, aspect < 0f ? 0.5625f : aspect);
        }

        void DeleteEvent()
        {
            _videoElement.Clear();
            _myEventContent.SetActive(false);
            _emptyEventContent.SetActive(true);
            _isEmpty = true;
        }

        void UpdateRemainingTime(int time)
        {
            int minutes = Mathf.FloorToInt((float)time / 60f);
            int seconds = time - minutes * 60;
            _timerLabel.text = string.Format("00:{0:00}:{1:00}", minutes, seconds);
            _timerLabelGradient.ApplyGradient();
        }
    }
}
