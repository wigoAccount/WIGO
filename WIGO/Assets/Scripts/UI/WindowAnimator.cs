using UnityEngine;
using DG.Tweening;

namespace WIGO.Userinterface
{
    public class WindowAnimator : MonoBehaviour
    {
        [SerializeField] CanvasGroup _windowGroup;

        RectTransform _window;

        const float ANIMATION_TIME = 0.1f;

        public void OnOpen()
        {
            _windowGroup.alpha = 0f;
            _window.localScale = Vector3.one * 0.9f;
            DOTween.Sequence().Append(_window.DOScale(1f, ANIMATION_TIME))
                .Join(_windowGroup.DOFade(1f, ANIMATION_TIME));
        }

        public void OnReopen()
        {
            _windowGroup.alpha = 0f;
            _window.localScale = Vector3.one * 1.1f;
            DOTween.Sequence().Append(_window.DOScale(1f, ANIMATION_TIME))
                .Join(_windowGroup.DOFade(1f, ANIMATION_TIME));
        }

        private void Awake()
        {
            _window = _windowGroup.transform as RectTransform;
        }
    }
}
