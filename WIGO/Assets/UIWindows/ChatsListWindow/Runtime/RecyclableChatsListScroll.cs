using System;
using System.Collections.Generic;
using UnityEngine;
using WIGO.RecyclableScroll;
using DG.Tweening;

namespace WIGO.Userinterface
{
    public class RecyclableChatsListScroll : RecyclableDataSource<UIChatElement, UIChatInfo>
    {
        [SerializeField] CanvasGroup _scrollGroup;

        public override void CreateScroll(IEnumerable<UIChatInfo> data, int selectedIndex = 0, Action callback = null)
        {
            if (_cachedCells.Count > 0)
            {
                _scrollGroup.DOFade(0f, 0.15f).OnComplete(() =>
                {
                    base.CreateScroll(data, selectedIndex, callback);
                    _scrollGroup.DOFade(1f, 0.15f);
                });

                return;
            }

            _scrollGroup.alpha = 0f;
            base.CreateScroll(data, selectedIndex, callback);
            _scrollGroup.DOFade(1f, 0.15f);
        }

        public override RectTransform CreateCell(int index)
        {
            var widget = Instantiate(_cellPrefab, _scrollRect.content);
            //widget.Init(_scrollRect);
            widget.ConfigureCell(_contentData[index], index);
            _cachedCells.Add(widget);

            return widget.GetComponent<RectTransform>();
        }

        public override void SetCell(int cell, int index)
        {
            _cachedCells[cell].ConfigureCell(_contentData[index], index);
        }
    }
}
