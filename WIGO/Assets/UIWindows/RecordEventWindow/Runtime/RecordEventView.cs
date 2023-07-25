using Crystal;
using UnityEngine;
using DG.Tweening;

namespace WIGO.Userinterface
{
    public class RecordEventView : UIWindowView<UIWindowModel>
    {
        [SerializeField] RectTransform _headerElement;
        [SerializeField] GameObject _loader;
        [SerializeField] RectTransform _maskBounds;
        [SerializeField] SafeArea _safeArea;

        const float UPPER_DEFAULT_PADDING = 80f;
        const float BOTTOM_DEFAULT_PADDING = 174f;

        public void SetActiveLoader(bool active)
        {
            _loader.SetActive(active);
        }

        public void SetActiveHeader(bool active, bool animate = true)
        {
            float position = active ? 0f : _headerElement.sizeDelta.y + _safeArea.GetSafeAreaUpperPadding();
            if (animate)
            {
                _headerElement.DOAnchorPosY(position, 0.24f);
                return;
            }

            _headerElement.anchoredPosition = Vector2.up * position;
        }

        public void AdaptMaskBounds(float cardHeight)
        {
            float screenHeight = ServiceLocator.Get<UIManager>().GetCanvasSize().y;
            float maskHeight = _maskBounds.rect.height > cardHeight
                ? cardHeight
                : Mathf.Min(screenHeight - UPPER_DEFAULT_PADDING - BOTTOM_DEFAULT_PADDING, cardHeight);
            _maskBounds.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, maskHeight);
        }
    }
}
