using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

namespace WIGO.Userinterface
{
    public class UIHorizontalSwipeScroll : ScrollRect
    {
        RectTransform[] _created;
        Action<int> _onSwitchBlock;
        Tween _moveTween;

        int _currentIndex;
        int _startScrollDirection;
        bool _isDragging;

        const float VELOCITY_THRESHOLD = 130f;
        const float MOVE_DURATION = 0.32f;

        public int GetCurrentIndex() => _currentIndex;

        public void Initialize(Action<int> onSwitchBlock)
        {
            _onSwitchBlock = onSwitchBlock;
            _created = new RectTransform[content.childCount];
            for (int i = 0; i < _created.Length; i++)
            {
                var item = content.GetChild(i) as RectTransform;
                _created[i] = item;
                if (i > 1)
                {
                    item.gameObject.SetActive(false);
                }
            }
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            if ((_currentIndex == 0 && eventData.delta.x > 0f)
                || (_currentIndex == _created.Length - 1 && eventData.delta.x < 0f))
            {
                return;
            }

            CancelMove();
            inertia = true;
            _isDragging = true;
            _startScrollDirection = eventData.delta.x > 0f ? 1 : -1;

            base.OnBeginDrag(eventData);
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging)
            {
                return;
            }

            base.OnDrag(eventData);
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            if (!_isDragging)
            {
                return;
            }

            _isDragging = false;
            DefineScrollMove();

            base.OnEndDrag(eventData);
            inertia = false;
            _startScrollDirection = 0;
        }

        public void RecycleTo(int index)
        {
            _currentIndex = index;
            ScrollToCurrent();
        }

        void DefineScrollMove()
        {
            if ((velocity.x < 0f && velocity.x > -VELOCITY_THRESHOLD) || (velocity.x > 0f && velocity.x < VELOCITY_THRESHOLD))
            {
                float currentItemPos = viewport.InverseTransformPoint(_created[_currentIndex].transform.position).x;
                float currentItemDelta = Mathf.Abs(currentItemPos);

                int nextIndex = currentItemPos > 0f ? _currentIndex - 1 : _currentIndex + 1;
                float nextItemDelta = Mathf.Abs(viewport.InverseTransformPoint(_created[nextIndex].transform.position).x);

                if (currentItemDelta < nextItemDelta)
                {
                    ScrollToCurrent();
                }
                else
                {
                    if (nextIndex > _currentIndex)
                    {
                        RecycleLeftToRight();
                    }
                    else
                    {
                        RecycleRightToLeft();
                    }
                }
            }
            else
            {
                int direction = velocity.x > 0f ? 1 : -1;
                if (direction != _startScrollDirection)
                {
                    ScrollToCurrent();
                    return;
                }

                if (velocity.x > 0f)
                {
                    RecycleRightToLeft();
                }
                else
                {
                    RecycleLeftToRight();
                }
            }
        }

        void RecycleLeftToRight()
        {
            int nextIndex = _currentIndex + 1;
            if (nextIndex >= _created.Length)
            {
                return;
            }

            _onSwitchBlock?.Invoke(nextIndex);
            _currentIndex = nextIndex;
            ScrollToCurrent();
        }

        void RecycleRightToLeft()
        {
            int nextIndex = _currentIndex - 1;
            if (nextIndex >= _created.Length)
            {
                return;
            }

            _onSwitchBlock?.Invoke(nextIndex);
            _currentIndex = nextIndex;
            ScrollToCurrent();
        }

        void ScrollToCurrent(Action callback = null)
        {
            for (int i = 0; i < _created.Length; i++)
            {
                _created[i].gameObject.SetActive(Mathf.Abs(_currentIndex - i) < 2);
            }

            float pos = -_created[_currentIndex].anchoredPosition.x;
            _moveTween = content.DOAnchorPosX(pos, MOVE_DURATION)
                .OnComplete(() =>
                {
                    callback?.Invoke();
                    _moveTween = null;
                });
        }

        void CancelMove()
        {
            if (_moveTween != null)
            {
                _moveTween.Kill();
                _moveTween = null;
            }
        }
    }
}
