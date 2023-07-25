using System;
using System.Collections.Generic;
using UnityEngine;

namespace WIGO.RecyclableScroll
{
    [RequireComponent(typeof(RecyclableScrollRect))]
    public abstract class RecyclableDataSource<TObject, TData> : MonoBehaviour where TObject : MonoBehaviour, ICell<TData> where TData : ContactInfo
    {
        [SerializeField] protected RectTransform _header;
        [SerializeField] protected float _horizontalSpacing;
        [SerializeField] protected float _verticalSpacing;
        [SerializeField] protected TObject _cellPrefab;
        [SerializeField] ScrollSettings _scrollSettings;

        protected List<TObject> _cachedCells = new List<TObject>();
        protected List<TData> _contentData = new List<TData>();

        protected RecyclableScrollRect _scrollRect;
        protected RecyclingSystem _recyclingSystem;

        public int GetItemCount() => _contentData.Count;
        public abstract RectTransform CreateCell(int index);
        public abstract void SetCell(int cell, int index);

        protected virtual void Awake()
        {
            _scrollRect = GetComponent<RecyclableScrollRect>();
            RectTransform prototype = _cellPrefab.transform as RectTransform;

            if (_scrollRect.Direction == DirectionType.Horizontal)
            {
                _recyclingSystem = new HorizontalRecyclingSystem(prototype, _scrollRect.viewport, _scrollRect.content, _scrollRect.IsGrid, _scrollRect.Segments, _horizontalSpacing, _verticalSpacing, _scrollSettings);
            }
            else
            {
                _recyclingSystem = new VerticalRecyclingSystem(prototype, _scrollRect.viewport, _scrollRect.content, _scrollRect.IsGrid, _scrollRect.Segments, _horizontalSpacing, _verticalSpacing, _scrollSettings);
            }

            _recyclingSystem.onCreateCell += CreateCell;
            _recyclingSystem.onGetDataCount += GetItemCount;
            _recyclingSystem.onSetCell += SetCell;
            _recyclingSystem.SetupHeader(_header);
        }

        public virtual void CreateScroll(IEnumerable<TData> data, int selectedIndex = 0, Action callback = null)
        {
            if (_cachedCells.Count > 0)
            {
                _cachedCells.Clear();
                _contentData.Clear();
            }

            _contentData = new List<TData>(data);
            RectTransform prototype = _cellPrefab.transform as RectTransform;
            _scrollRect.Initialize(_recyclingSystem);
            StartCoroutine(_recyclingSystem.InitCoroutine(prototype, () =>
            {
                _scrollRect.SetValueChangeListener();
                callback?.Invoke();
            }, selectedIndex));
        }

        public virtual void ClearScroll()
        {
            _scrollRect?.ReloadData();
            _recyclingSystem?.ClearContent();
            _cachedCells.Clear();
            _contentData.Clear();
        }

        private void OnDestroy()
        {
            _recyclingSystem.onCreateCell -= CreateCell;
            _recyclingSystem.onGetDataCount -= GetItemCount;
            _recyclingSystem.onSetCell -= SetCell;
        }
    }
}
