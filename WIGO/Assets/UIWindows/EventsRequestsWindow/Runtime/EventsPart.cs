using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using WIGO.Core;

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
        [SerializeField] EventCard _myEventData;

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

        public void OnDeleteEventClick()
        {

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
            // set loading
            await Task.Delay(200);

            _loaded = true;
            if (_isEmpty)
            {
                _myEventContent.SetActive(false);
                _emptyEventContent.SetActive(true);
                return;
            }

            _myEventContent.SetActive(true);
            _emptyEventContent.SetActive(false);

            _remainingSeconds = _myEventData.GetRemainingTime();
            _timer = 0f;
            _eventDescLabel.text = _myEventData.GetDescription();
            _addressLabel.text = _myEventData.GetLocation();

            foreach (var category in _myEventData.GetHashtags())
            {
                var categoryBlock = Instantiate(_categoryPrefab, _categoriesContent);
                categoryBlock.Setup(category);
            }

            _videoElement.SetupVideo(_myEventData.GetVideoPath(), _myEventData.GetVideoAspect());
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
