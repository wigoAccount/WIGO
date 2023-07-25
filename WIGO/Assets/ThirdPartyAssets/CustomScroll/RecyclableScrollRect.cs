using System;
using UnityEngine;
using UnityEngine.UI;

namespace WIGO.RecyclableScroll
{
    public enum DirectionType
    {
        Vertical,
        Horizontal
    }

    public class RecyclableScrollRect : ScrollRect
    {
        public int Segments
        {
            set
            {
                _segments = Math.Max(value, 2);
            }
            get
            {
                return _segments;
            }
        }

        public DirectionType Direction
        {
            get { return _direction; }
        }

        public bool IsGrid
        {
            get { return _isGrid; }
        }

        [SerializeField] bool _isGrid;
        [SerializeField] DirectionType _direction;
        [SerializeField] int _segments;                     // segments : coloums for vertical and rows for horizontal.

        RecyclingSystem _recyclingSystem;
        Vector2 _prevAnchoredPos;

        /// <summary>
        /// Initialization when selfInitalize is true. Assumes that data source is set in controller's Awake.
        /// </summary>
        public void Initialize(RecyclingSystem recyclingSystem)
        {
            vertical = _direction == DirectionType.Vertical;
            horizontal = _direction == DirectionType.Horizontal;
            _recyclingSystem = recyclingSystem;

            _prevAnchoredPos = content.anchoredPosition;
            onValueChanged.RemoveListener(OnValueChangedListener);
        }

        public void SetValueChangeListener()
        {
            onValueChanged.AddListener(OnValueChangedListener);
        }

        /// <summary>
        /// Added as a listener to the OnValueChanged event of Scroll rect.
        /// Recycling entry point for recyling systems.
        /// </summary>
        /// <param name="direction">scroll direction</param>
        public void OnValueChangedListener(Vector2 normalizedPos)
        {
            Vector2 dir = content.anchoredPosition - _prevAnchoredPos;
            m_ContentStartPosition += _recyclingSystem.OnValueChangedListener(dir);
            _prevAnchoredPos = content.anchoredPosition;
        }

        /// <summary>
        /// Overloaded ReloadData with dataSource param
        /// Reloads the data. Call this if a new datasource is assigned.
        /// </summary>
        public void ReloadData()
        {
            StopMovement();
            onValueChanged.RemoveListener(OnValueChangedListener);
            _prevAnchoredPos = content.anchoredPosition;
        }
    }
}
