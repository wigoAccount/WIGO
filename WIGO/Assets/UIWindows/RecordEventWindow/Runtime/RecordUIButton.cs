using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;

namespace WIGO.Userinterface
{
    public class RecordUIButton : MonoBehaviour
    {
        [SerializeField] CanvasGroup _timerGroup;
        [SerializeField] TMP_Text _timerLabel;
        [SerializeField] Image _circle;
        [SerializeField] Image _point;
        [SerializeField] Image _fill;

        Sequence _animation;

        public void StartRecordMode()
        {
            CancelAnimation();

            RectTransform timerElement = _timerGroup.transform as RectTransform;
            timerElement.anchoredPosition = Vector2.down * timerElement.sizeDelta.y;
            _timerGroup.alpha = 0f;
            _timerGroup.gameObject.SetActive(true);
            _fill.fillAmount = 0f;
            _fill.gameObject.SetActive(true);

            _animation = DOTween.Sequence();
            _animation.Append(_timerGroup.DOFade(1f, 0.24f))
                .Join(timerElement.DOAnchorPosY(8f, 0.24f).SetEase(Ease.OutBack))
                .Join(_circle.DOFade(0.2f, 0.12f))
                .Join(_point.DOFade(0.2f, 0.12f))
                .OnComplete(() => _animation = null);
        }

        public void StopRecordMode()
        {
            CancelAnimation();

            RectTransform timerElement = _timerGroup.transform as RectTransform;
            _fill.gameObject.SetActive(false);

            _animation = DOTween.Sequence();
            _animation.Append(_timerGroup.DOFade(0f, 0.24f))
                .Join(timerElement.DOAnchorPosY(-timerElement.sizeDelta.y, 0.24f))
                .Join(_circle.DOFade(1f, 0.12f))
                .Join(_point.DOFade(1f, 0.12f))
                .OnComplete(() =>
                {
                    _timerGroup.gameObject.SetActive(false);
                    _animation = null;
                });
        }

        public void SetTimeText(int time)
        {
            if (time < 0)
                time = 0;

            int minutes = Mathf.FloorToInt((float)time / 60f);
            int seconds = time - minutes * 60;
            _timerLabel.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        public void SetProgress(float value)
        {
            _fill.fillAmount = value;
        }

        public void ResetButton()
        {
            //_recorder.rectTransform.sizeDelta = Vector2.one * 52f;
            //_recorder.pixelsPerUnitMultiplier = 1f;
            _timerGroup.alpha = 0f;

            RectTransform timerElement = _timerGroup.transform as RectTransform;
            timerElement.anchoredPosition = Vector2.down * timerElement.sizeDelta.y;
            _timerGroup.gameObject.SetActive(false);
        }

        void CancelAnimation()
        {
            if (_animation != null)
            {
                _animation.Kill();
                _animation = null;
            }
        }
    }
}
