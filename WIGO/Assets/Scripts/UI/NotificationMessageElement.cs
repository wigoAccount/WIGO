using System;
using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.EventSystems;
using System.Collections;

namespace WIGO.Userinterface
{
    public enum NotificationType
    {
        DEFAULT,
        ALERT,
        INFO
    }

    public class NotificationMessageElement : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] RectTransform _panel;
        [SerializeField] TMP_Text _messageLabel;
        [SerializeField] CanvasGroup _canvasGroup;
        [SerializeField] NotificationType _notificationType;

        Sequence _openAnimation;
        Sequence _closeAnimation;
        Action _onClose;
        Coroutine _visibleCoroutine;
        bool _isActive;

        Sequence _snapper;
        Vector3 _deltaPos;
        Vector2 _restrictions = new Vector2(0f, 120f);
        bool _isDragging;

        const float LIFETIME = 3f;
        const float OPEN_DURATION = 0.2f;
        const float CLOSE_DURATION = 1f;
        const float MOVE_DURATION = 0.2f;

        public NotificationType GetNotificationType() => _notificationType;

        public void Setup(Action onClose)
        {
            _onClose = onClose;
            float yPos = _panel.anchoredPosition.y;
            _panel.anchoredPosition = Vector2.down * _panel.sizeDelta.y;
            _canvasGroup.alpha = 0f;

            _openAnimation = DOTween.Sequence();
            _openAnimation.OnComplete(() => { _isActive = true; });
            _openAnimation.Append(_canvasGroup.DOFade(1f, OPEN_DURATION))
                .Join(_panel.DOAnchorPosY(yPos, OPEN_DURATION));

            _closeAnimation = DOTween.Sequence();
            _closeAnimation.Append(_canvasGroup.DOFade(0f, CLOSE_DURATION)).OnComplete(CloseNotification);
            _closeAnimation.Pause();

            _visibleCoroutine = StartCoroutine(VisibleProcess());
        }

        public void Setup(string message, Action onClose)
        {
            _messageLabel.SetText(message);
            Setup(onClose);
        }

        public virtual void Rewind()
        {
            CancelAnimation();
            _canvasGroup.alpha = 1f;
            _visibleCoroutine = StartCoroutine(VisibleProcess());
        }

        public void Remove()
        {
            CancelAnimation();
            Destroy(gameObject);
        }

        private void CancelAnimation()
        {
            if (_closeAnimation != null && _closeAnimation.IsPlaying())
            {
                _closeAnimation.Pause();
            }
            if (_visibleCoroutine != null)
            {
                StopCoroutine(_visibleCoroutine);
            }
        }

        public void OnCloseButtonClick()
        {
            if (_isActive)
            {
                StopCoroutine(_visibleCoroutine);
                _isActive = false;
                _closeAnimation.Play();
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_isActive)
            {
                if (_snapper != null)
                {
                    _snapper.Kill();
                    _snapper = null;
                }

                if (_visibleCoroutine != null)
                {
                    StopCoroutine(_visibleCoroutine);
                    _visibleCoroutine = null;
                }

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
            if (_isDragging && _isActive)
            {
                Vector3 inputPosition;
#if UNITY_EDITOR
                inputPosition = Input.mousePosition;
#else
                inputPosition = Input.GetTouch(0).position;
#endif
                inputPosition.z = transform.position.z;

                Vector3 position = transform.parent.InverseTransformPoint(new Vector3(inputPosition.x - _deltaPos.x, transform.position.y, 0f));
                position.x = Mathf.Clamp(position.x, _restrictions.x, _restrictions.y);
                transform.localPosition = position;

                float relativeValue = transform.localPosition.x / (_restrictions.y - _restrictions.x);
                _canvasGroup.alpha = 1f - relativeValue;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_isActive)
            {
                return;
            }

            if (_snapper != null)
            {
                _snapper.Kill();
            }

            float pos = 0f;
            if (transform.localPosition.x >= (_restrictions.y - _restrictions.x) / 2f)
            {
                pos = _restrictions.y;
                SmoothClose();
            }
            else
            {
                pos = _restrictions.x;
                _canvasGroup.DOFade(1f, MOVE_DURATION);
                _visibleCoroutine = StartCoroutine(VisibleProcess());
            }
            _snapper = DOTween.Sequence();
            _snapper.OnComplete(() => { _snapper = null; });
            _snapper.Append(transform.DOLocalMoveX(pos, MOVE_DURATION));
            _isDragging = false;
        }

        public void SmoothClose()
        {
            _isActive = false;
            _canvasGroup.DOFade(0f, MOVE_DURATION).OnComplete(() =>
            {
                CloseNotification();
            });
        }

        protected void CloseNotification()
        {
            _onClose?.Invoke();
            Destroy(gameObject);
        }

        private IEnumerator VisibleProcess()
        {
            yield return new WaitForSeconds(LIFETIME);

            _isActive = false;
            _closeAnimation.Play();
            _visibleCoroutine = null;
        }
    }
}
