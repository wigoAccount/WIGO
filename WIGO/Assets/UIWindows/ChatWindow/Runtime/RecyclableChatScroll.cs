using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WIGO.RecyclableScroll;
using DG.Tweening;

namespace WIGO.Userinterface
{
    public class RecyclableChatScroll : RecyclableDataSource<UIMessageElement, UIMessageInfo>
    {
        RectTransform _scroll;
        Tween _heightAnimation;
        float _scrollBottomHeight;
        float _addingBottomHeight;

        const float DEFAULT_BOTTOM_PADDING = 100f;

        public void Init()
        {
            _scroll = transform as RectTransform;
            _scrollBottomHeight = _scroll.offsetMin.y;
        }

        public override RectTransform CreateCell(int index)
        {
            var widget = Instantiate(_cellPrefab, _scrollRect.content);
            widget.InitCell(null);
            widget.ConfigureCell(_contentData[index], index);
            _cachedCells.Add(widget);

            return widget.GetComponent<RectTransform>();
        }

        public override void SetCell(int cell, int index)
        {
            _cachedCells[cell].ConfigureCell(_contentData[index], index);
        }

        public override void ClearScroll()
        {
            CancelAnimation();
            _addingBottomHeight = 0f;
            _scrollBottomHeight = DEFAULT_BOTTOM_PADDING;
            _scroll.offsetMin = new Vector2(_scroll.offsetMin.x, _scrollBottomHeight);
            base.ClearScroll();
        }

        public void SetAddingHeight(float delta)
        {
            _addingBottomHeight = delta;
            _scroll.offsetMin = new Vector2(_scroll.offsetMin.x, _scrollBottomHeight + delta);
        }

        public void SetBottomHeight(float delta)
        {
            CancelAnimation();
            _scrollBottomHeight = DEFAULT_BOTTOM_PADDING + delta;
            _scroll.offsetMin = new Vector2(_scroll.offsetMin.x, _scrollBottomHeight + _addingBottomHeight);
        }

        public void SetToDefaultBottom(bool animate = true)
        {
            CancelAnimation();
            _scrollBottomHeight = DEFAULT_BOTTOM_PADDING;
            if (animate)
            {
                Vector2 bounds = new Vector2(_scroll.offsetMin.x, _scrollBottomHeight + _addingBottomHeight);
                _heightAnimation = DOTween.To(() => _scroll.offsetMin, x => _scroll.offsetMin = x, bounds, 0.1f)
                    .SetEase(Ease.OutSine)
                    .OnComplete(() => _heightAnimation = null);
                return;
            }

            _scroll.offsetMin = new Vector2(_scroll.offsetMin.x, _scrollBottomHeight);
        }

        void CancelAnimation()
        {
            if (_heightAnimation != null)
            {
                _heightAnimation.Kill();
                _heightAnimation = null;
            }
        }
    }
}
