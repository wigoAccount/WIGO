using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using WIGO.Core;
using DG.Tweening;
using Event = WIGO.Core.Event;

namespace WIGO.Userinterface
{
    public class UIEventCardElement : VideoEventElement, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] Image _overlay;
        [SerializeField] TMP_Text _usernnameLabel;
        [SerializeField] TMP_Text _descLabel;
        [SerializeField] TMP_Text _distanceTimeLabel;
        [SerializeField] TMP_Text _timeLabel;
        [SerializeField] TMP_Text _moreButton;
        [SerializeField] TMP_Text _locationLabel;
        //[SerializeField] TMP_Text _groupSizeLabel;
        //[SerializeField] Image _groupSizeIcon;
        [Space]
        [SerializeField] CategoryEventElement _categoryPrefab;
        [SerializeField] RectTransform _categoriesContent;
        [Space]
        [SerializeField] Color _acceptColor;
        [SerializeField] Color _declineColor;
        [Space]
        [SerializeField] string _editorVideoPath;

        CanvasGroup _cardGroup;
        Action<bool> _onCardSkip;
        Sequence _squeezeTween;
        float _timer;
        int _remainingSeconds;
        bool _isFullDesc;

        Vector3 _deltaPos;
        bool _isDragging;

        const float SWIPE_BOUNDS = 240f;
        const float SWIPE_BACK_VALUE = 64f;
        const float MIN_DESC_HEIGHT = 16f;

        public void Setup(Event card, Action<bool> onCardSkip)
        {
            _onCardSkip = onCardSkip;
            _cardGroup = GetComponent<CanvasGroup>();

            _usernnameLabel.text = card.author.nickname;
            float nameWidth = Mathf.Min(_usernnameLabel.preferredWidth + 0.4f, 128f);
            _usernnameLabel.rectTransform.sizeDelta = new Vector2(nameWidth, _usernnameLabel.rectTransform.sizeDelta.y);
            _descLabel.text = card.about;
            _locationLabel.text = card.address;
            _moreButton.gameObject.SetActive(_descLabel.preferredWidth > _descLabel.rectTransform.rect.width);
            _distanceTimeLabel.text = $"{card.waiting} min from me";        // [TODO]: change with config
            _remainingSeconds = card.waiting;
            //_groupSizeLabel.text = card.GetGroupSizeType().ToString();
            //_groupSizeIcon.color = card.GetGroupSizeType() == EventGroupSizeType.Single ? UIGameColors.Purple : UIGameColors.Blue;

            var model = ServiceLocator.Get<GameModel>();
            foreach (var category in card.tags)
            {
                var categoryBlock = Instantiate(_categoryPrefab, _categoriesContent);
                categoryBlock.Setup(model.GetCategoryNameWithIndex(category));
            }

            string url;
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            url = ServiceLocator.Get<S3ContentClient>().GetVideoURL(card.video);
#else
            url = System.IO.Path.Combine(Application.streamingAssetsPath, _editorVideoPath);
#endif
            SetupVideo(card.video, card.AspectRatio);
            OnOpen();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!_isDragging)
            {
                Vector3 inputPosition;
#if UNITY_EDITOR
                inputPosition = Input.mousePosition;
#else
                inputPosition = Input.GetTouch(0).position;
#endif
                inputPosition.z = transform.position.z;
                _deltaPos = inputPosition - transform.position;
                _isDragging = true;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_isDragging)
            {
                Vector3 inputPosition;
#if UNITY_EDITOR
                inputPosition = Input.mousePosition;
#else
                inputPosition = Input.GetTouch(0).position;
#endif
                inputPosition.z = transform.position.z;
                Vector3 localPos = transform.parent.InverseTransformPoint(inputPosition - _deltaPos);
                localPos.x = Mathf.Clamp(localPos.x, -SWIPE_BOUNDS, SWIPE_BOUNDS);
                localPos.y = transform.localPosition.y;
                transform.localPosition = localPos;

                Color color = localPos.x >= 0f ? _acceptColor : _declineColor;
                _overlay.color = color;
                float swipeValue = Mathf.Abs(localPos.x / SWIPE_BOUNDS);
                UIGameColors.SetTransparent(_overlay, Mathf.Clamp01(6.4f * swipeValue));
                _cardGroup.alpha = 1f - swipeValue;
                _cardRect.localScale = Vector3.one * (1f - 0.2f * swipeValue);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_isDragging)
            {
                _isDragging = false;
                float pos = _cardRect.anchoredPosition.x;
                if (pos >= SWIPE_BACK_VALUE || pos < -SWIPE_BACK_VALUE)
                {
                    SkipCard(pos > 0f ? 1 : -1);
                    return;
                }

                ReturnCard();
            }
        }

        public override void OnVideoClick()
        {
            if (_isDragging)
            {
                return;
            }

            base.OnVideoClick();
        }

        public void OnMoreButtonClick()
        {
            float height = _isFullDesc ? MIN_DESC_HEIGHT : _descLabel.preferredHeight + 0.4f;
            float pos = _descLabel.rectTransform.anchoredPosition.y + height + 16f;
            _isFullDesc = !_isFullDesc;
            _moreButton.text = _isFullDesc ? "Less" : "More";       // [TODO]: change with config
            CancelSqueeze();

            _squeezeTween = DOTween.Sequence().Append(_descLabel.rectTransform.DOSizeDelta(new Vector2(_descLabel.rectTransform.sizeDelta.x, height), 0.2f))
                .Join(_usernnameLabel.rectTransform.DOAnchorPosY(pos, 0.2f))
                .OnComplete(() => _squeezeTween = null);
        }

        void Update()
        {
            _timer += Time.deltaTime;
            if (_timer >= 1f)
            {
                _timer -= 1f;
                _remainingSeconds--;
                UpdateRemainingTime(_remainingSeconds);
            }
        }

        void OnOpen()
        {
            _cardGroup.alpha = 0f;
            transform.localScale = Vector3.one * 0.75f;

            var animation = DOTween.Sequence();
            animation.Append(_cardGroup.DOFade(1f, 0.4f))
                .Join(transform.DOScale(1f, 0.4f));
        }

        void SkipCard(int direction)
        {
            float pos = 320f * direction;
            bool accept = direction > 0;
            _onCardSkip?.Invoke(accept);

            var animation = DOTween.Sequence();
            animation.Append(_cardRect.DOAnchorPosX(pos, 0.28f))
                .Join(_cardRect.DOScale(0.8f, 0.28f))
                .Join(_cardGroup.DOFade(0f, 0.28f))
                .Join(_overlay.DOFade(1f, 0.28f))
                .OnComplete(() =>
                {
                    Clear();
                });
        }

        void ReturnCard()
        {
            var animation = DOTween.Sequence();
            animation.Append(_cardRect.DOAnchorPosX(0f, 0.2f))
                .Join(_cardRect.DOScale(1f, 0.2f))
                .Join(_cardGroup.DOFade(1f, 0.2f))
                .Join(_overlay.DOFade(0f, 0.2f));
        }

        void UpdateRemainingTime(int time)
        {
            int minutes = Mathf.FloorToInt((float)time / 60f);
            int seconds = time - minutes * 60;
            _timeLabel.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        public override void Clear()
        {
            base.Clear();
            RenderTexture.active = null;
            Destroy(gameObject);
        }

        void CancelSqueeze()
        {
            if (_squeezeTween != null)
            {
                _squeezeTween.Kill();
                _squeezeTween = null;
            }
        }
    }
}
