using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIGO.RecyclableScroll
{
    /// <summary>
    /// Absract Class for creating a Recycling system.
    /// </summary>
    public abstract class RecyclingSystem
    {
        public Func<int, RectTransform> onCreateCell;
        public Func<int> onGetDataCount;
        public Action<int, int> onSetCell;

        protected float _horizontalSpacing;
        protected float _verticalSpacing;
        protected RectTransform _viewport; 
        protected RectTransform _content;
        protected RectTransform _header;
        protected bool _isGrid;

        //Cell dimensions
        protected float _cellWidth;
        protected float _cellHeight;

        // Pool Generation
        protected List<RectTransform> _cellPool;
        protected Bounds _recyclableViewBounds;

        // Temps, Flags 
        protected readonly Vector3[] _corners = new Vector3[4];
        protected bool _recycling;
        protected Vector2 zeroVector = Vector2.zero;                // Cached zero vector

        // Trackers
        protected int _currentItemCount;                            // item count corresponding to the datasource.
        protected ScrollSettings _scrollSettings;                   // Scroll value parameters
        protected float _emptyContentSize = 0f;                     // Content size when there is no any element in scroll

        public abstract IEnumerator InitCoroutine(RectTransform prototype, Action onInitialized = null, int selectedIndex = 0);
        public abstract Vector2 OnValueChangedListener(Vector2 direction);

        public virtual void SetupHeader(RectTransform header)
        {
            _header = header;
        }

        public virtual void ClearContent()
        {
            if (_cellPool != null)
            {
                _cellPool.ForEach((RectTransform item) => GameObject.Destroy(item.gameObject));
                _cellPool.Clear();
            }
            else
            {
                _cellPool = new List<RectTransform>();
            }

            _content.anchoredPosition = Vector3.zero;
        }

        protected int GetDataCount() => onGetDataCount == null ? 0 : onGetDataCount();
    }

    [Serializable]
    public class ScrollSettings
    {
        public float minPoolCoverage = 1.8f;                    // The recyclable pool must cover (viewPort * _poolCoverage) area.
        public int minPoolSize = 6;                             // Cell pool must have a min size
        public float recyclingThreshold = .5f;                  // Threshold for recycling above and below viewport
        public bool reverse;
    }
}