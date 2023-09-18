using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace WIGO.Userinterface
{
    public class CreateEventFailMessage : MonoBehaviour
    {
        CanvasGroup _messageGroup;
        RectTransform _message;

        Sequence _animation;

        public void Init()
        {
            _message = transform as RectTransform;
            _messageGroup = GetComponent<CanvasGroup>();
        }

        public void Show()
        {
            _message.anchoredPosition = Vector2.down * _message.sizeDelta.y;
            _messageGroup.alpha = 0f;
            gameObject.SetActive(true);

            _animation = DOTween.Sequence();
            _animation.Append(_message.DOAnchorPosY(16f, 0.28f))
                .Join(_messageGroup.DOFade(1f, 0.28f))
                .AppendInterval(2f)
                .Append(_messageGroup.DOFade(0f, 0.28f))
                .OnComplete(() =>
                {
                    gameObject.SetActive(false);
                    _animation = null;
                });
        }

        public void OnCloseClick()
        {
            Close();
        }

        public void Close(bool animate = true)
        {
            if (!gameObject.activeSelf)
            {
                return;
            }

            if (_animation != null)
            {
                _animation.Kill();
                _animation = null;
            }

            if (!animate)
            {
                gameObject.SetActive(false);
                return;
            }

            _animation = DOTween.Sequence().Append(_messageGroup.DOFade(0f, 0.28f))
                    .OnComplete(() =>
                    {
                        gameObject.SetActive(false);
                        _animation = null;
                    });
        }
    }
}
