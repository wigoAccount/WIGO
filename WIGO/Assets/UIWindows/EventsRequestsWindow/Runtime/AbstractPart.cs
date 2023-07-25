using UnityEngine;
using DG.Tweening;

namespace WIGO.Userinterface
{
    public class AbstractPart : MonoBehaviour
    {
        [SerializeField] float _outPosX;

        CanvasGroup _partGroup;
        RectTransform _partRect;
        Sequence _animation;
        protected bool _loaded;

        public void Initialize()
        {
            _partGroup = GetComponent<CanvasGroup>();
            _partRect = transform as RectTransform;
        }

        public virtual void SetPartActive(bool active, bool animate = true)
        {
            CancelAnimation();

            if (animate)
            {
                _animation = DOTween.Sequence();
                if (active)
                {
                    gameObject.SetActive(true);
                    _animation.Append(_partRect.DOAnchorPosX(0f, 0.28f))
                        .Join(_partGroup.DOFade(1f, 0.28f))
                        .OnComplete(() => _animation = null);
                }
                else
                {
                    _animation.Append(_partRect.DOAnchorPosX(_outPosX, 0.28f))
                        .Join(_partGroup.DOFade(0f, 0.28f))
                        .OnComplete(() =>
                        {
                            gameObject.SetActive(false);
                            _animation = null;
                        });
                }
                return;
            }

            _partGroup.alpha = active ? 1f : 0f;
            _partRect.anchoredPosition = new Vector2(active ? 0f : _outPosX, _partRect.anchoredPosition.y);
            gameObject.SetActive(active);
        }

        public virtual void ResetPart()
        {
            _loaded = false;
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
