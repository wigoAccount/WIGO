using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using System;

namespace WIGO.Userinterface
{
    public class UIChatHorizontalMoveHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        ScrollRect _mainScroll;
        Image _chat;
        Vector3 _deltaPos;
        Vector2 _restrictions = new Vector2(-152f, 0f);
        Sequence _snapper;
        Action _clickCallback;
        bool _isDragging;
        bool _draggingParent;

        const float MOVE_DURATION = 0.2f;
        const float MAX_ALPHA = 0.05f;

        public void Init(ScrollRect scroll, Action onClickCallback)
        {
            _mainScroll = scroll;
            _clickCallback = onClickCallback;
            _chat = GetComponent<Image>();
        }

        public void ResetPosition()
        {
            _chat.rectTransform.anchoredPosition = Vector2.zero;
            UIGameColors.SetTransparent(_chat);
        }

        public void OnSelectChat()
        {
            if (!_isDragging && !_draggingParent)
            {
                _clickCallback?.Invoke();
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (IsPotentialParentDrag(eventData.delta))
            {
                _mainScroll.OnBeginDrag(eventData);
                _draggingParent = true;
                return;
            }

            _draggingParent = false;
            if (_snapper != null)
            {
                _snapper.Kill();
                _snapper = null;
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

        public void OnDrag(PointerEventData eventData)
        {
            if (_draggingParent)
            {
                _mainScroll.OnDrag(eventData);
                return;
            }

            if (_isDragging)
            {
                Vector3 inputPosition;
#if UNITY_EDITOR
                inputPosition = Input.mousePosition;
#else
                inputPosition = Input.GetTouch(0).position;
#endif
                inputPosition.z = transform.position.z;

                Vector3 position = transform.parent.InverseTransformPoint(new Vector3(inputPosition.x - _deltaPos.x, transform.position.y, 0f));
                position.x = Mathf.Clamp(position.x + _chat.rectTransform.sizeDelta.x / 2f, _restrictions.x, _restrictions.y);
                _chat.rectTransform.anchoredPosition = position;

                float relativeValue = Mathf.Clamp01(-_chat.rectTransform.anchoredPosition.x / (_restrictions.y - _restrictions.x) * 4f);
                float alpha = Mathf.Lerp(0f, MAX_ALPHA, relativeValue);
                UIGameColors.SetTransparent(_chat, alpha);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_draggingParent)
            {
                _mainScroll.OnEndDrag(eventData);
            }

            _draggingParent = false;
            if (_snapper != null)
            {
                _snapper.Kill();
            }

            float pos = 0f;
            float alpha = 0f;
            if (-_chat.rectTransform.anchoredPosition.x >= (_restrictions.y - _restrictions.x) / 2f - 16f)
            {
                pos = _restrictions.x;
                alpha = MAX_ALPHA;
            }
            else
            {
                pos = _restrictions.y;
                alpha = 0f;
            }

            _snapper = DOTween.Sequence();
            _snapper.Append(_chat.rectTransform.DOAnchorPosX(pos, MOVE_DURATION))
                .Join(_chat.DOFade(alpha, MOVE_DURATION))
                .OnComplete(() => _snapper = null);
            _isDragging = false;
        }

        bool IsPotentialParentDrag(Vector2 inputDelta)
        {
            if (_mainScroll != null)
            {
                if (_mainScroll.horizontal && !_mainScroll.vertical)
                {
                    return Mathf.Abs(inputDelta.x) > Mathf.Abs(inputDelta.y);
                }
                if (!_mainScroll.horizontal && _mainScroll.vertical)
                {
                    return Mathf.Abs(inputDelta.x) < Mathf.Abs(inputDelta.y);
                }
                else return true;
            }

            return false;
        }
    }
}
