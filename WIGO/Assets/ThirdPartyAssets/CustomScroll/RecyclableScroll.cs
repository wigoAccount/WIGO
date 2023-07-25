using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WIGO.RecyclableScroll
{
    public abstract class RecyclableScroll<TContent> : ScrollRect
    {
        protected RecyclableScrollElement _elementPrefab;       //main cell prefab
        protected RectTransform _header;                        //adding top element in scroll
        protected Transform _postsParent;                       //optional parent for elements. If null Scroll content is used

        protected List<TContent> _contentList = new List<TContent>();                                       //full content list
        protected List<RecyclableScrollElement> _visibleElements = new List<RecyclableScrollElement>();     //created elements in scroll

        protected int _topMostCellIndex, _bottomMostCellIndex;  //topmost and bottommost cell in the heirarchy
        protected int _currentItemCount;                        //item count corresponding to the datasource.
        protected int _maxElementsCount, _minElementsCount;     //max and min cell count. Min is used for start creation
        protected bool _active = true;                          //can switch on/off scroll logic
        protected Bounds _recyclableViewBounds;                 //if out of this area - load content

        Vector2 _prevAnchoredPos;                               //content position in previous frame
        bool _recycling;                                        //recycling processbool

        protected readonly Vector3[] _corners = new Vector3[4]; //static corners of window rectangle

        protected const float RECYCLING_TRESHOLD = .35f;        //threshold for recycling above and below window
        protected const int MIN_ELEMENTS = 4;                   //default min cell count

        /// <summary>
        /// Cache main parameters
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="header"></param>
        /// <param name="postsParent"></param>
        internal void SetMainStuff(RecyclableScrollElement prefab, RectTransform header = null, Transform postsParent = null)
        {
            _elementPrefab = prefab;
            _header = header;
            _postsParent = (postsParent == null) ? content : postsParent;
        }

        /// <summary>
        /// Initialization
        /// </summary>
        /// <param name="content"></param>
        internal void Init(List<TContent> content)
        {
            _active = true;
            _contentList = content;
            SetMaxAndMinCount(content.Count);

            if (_visibleElements.Count > 0)
            {
                ReloadExistingContent();
                return;
            }
            _recyclableViewBounds = new Bounds();
            SetupContent();
            onValueChanged.AddListener(OnValueChangedListener);
        }

        public void ClearContent()
        {
            if (_visibleElements.Count > 0)
            {
                foreach (var element in _visibleElements)
                {
                    Destroy(element.gameObject);
                }

                _visibleElements.Clear();
                content.anchoredPosition = Vector3.zero;
                SetContentSize();
            }
        }

        public virtual void OnClose()
        {
            _active = false;
            content.anchoredPosition = Vector3.zero;
        }

        /// <summary>
        /// If load next part of content, it adds to existing
        /// </summary>
        /// <param name="addingList"></param>
        public void AddElements(List<TContent> addingList)
        {
            if (addingList != null && addingList.Count > 0)
            {
                _contentList.AddRange(addingList);
                _maxElementsCount += addingList.Count;
            }
        }

        /// <summary>
        /// Remove element from content list and update scroll
        /// </summary>
        /// <param name="element"></param>
        /// <param name="contentIndex"></param>
        public void RemoveElement(RecyclableScrollElement element, int contentIndex)
        {
            _contentList.RemoveAt(contentIndex);
            _maxElementsCount--;

            if (_maxElementsCount - _currentItemCount < 0)
            {
                UpdateFromUpToBottom();
                return;
            }

            UpdateFromRemovingToBottom(element, contentIndex);
        }

        protected abstract void AddToBottom(int index);
        protected abstract void UpdateElement(int index, int contentIndex);

        /// <summary>
        /// This function was made for overriding from inherited classes
        /// </summary>
        /// <param name="contentCount"></param>
        protected virtual void SetMaxAndMinCount(int contentCount)
        {
            _maxElementsCount = contentCount;
            _minElementsCount = Mathf.Min(MIN_ELEMENTS, contentCount);
        }

        /// <summary>
        /// Instantiate and initialize prefab
        /// </summary>
        /// <typeparam name="TElement"></typeparam>
        /// <param name="callback"></param>
        /// <param name="prefab"></param>
        /// <returns></returns>
        protected TElement CreateElement<TElement>(Action<TElement> callback, RecyclableScrollElement prefab = null) where TElement : RecyclableScrollElement
        {
            var finalPrefab = (prefab == null) ? _elementPrefab : prefab;
            var element = Instantiate(finalPrefab, _postsParent);

            RectTransform elementRect = element.transform as RectTransform;
            float addingHeight = _header != null ? _header.sizeDelta.y : 0f;
            elementRect.anchoredPosition = (_visibleElements.Count == 0) ? Vector2.down * addingHeight : Vector2.up * _visibleElements[_visibleElements.Count - 1].GetBottomPoint();
            element.Init();
            _visibleElements.Add(element);

            TElement telement = element as TElement;
            callback?.Invoke(telement);

            return telement;
        }

        protected virtual float GetDeltaAfterRecycle(int direction, float yDelta) => 0f;

        /// <summary>
        /// Set safe scroll area
        /// </summary>
        protected virtual void SetRecyclingBounds()
        {
            RectTransform scrollRect = transform as RectTransform;
            scrollRect.GetWorldCorners(_corners);
            float threshHold = RECYCLING_TRESHOLD * (_corners[2].y - _corners[0].y);
            _recyclableViewBounds.min = new Vector3(_corners[0].x, _corners[0].y - threshHold);
            _recyclableViewBounds.max = new Vector3(_corners[2].x, _corners[2].y + threshHold);
        }

        /// <summary>
        /// Update content size if elements size was changed
        /// </summary>
        protected virtual void SetContentSize()
        {
            float contentSize = (_header != null) ? _header.sizeDelta.y : 0f;
            foreach (var element in _visibleElements)
            {
                contentSize += element.GetElementSize();
            }

            content.sizeDelta = new Vector2(content.sizeDelta.x, contentSize);
        }

        protected virtual void CheckElementInReload(RecyclableScrollElement element, int contentIndex) { }

        /// <summary>
        /// Creating elements in scroll
        /// </summary>
        void SetupContent()
        {
            for (int i = 0; i < _minElementsCount; i++)
            {
                AddToBottom(i);
            }

            SetContentSize();
            _currentItemCount = _visibleElements.Count;
            _topMostCellIndex = 0;
            _bottomMostCellIndex = _visibleElements.Count - 1;
            SetRecyclingBounds();
        }

        /// <summary>
        /// If there are cached elements, just update them
        /// </summary>
        void ReloadExistingContent()
        {
            if (_visibleElements.Count > _minElementsCount)
            {
                int delta = _visibleElements.Count - _minElementsCount;
                for (int i = 0; i < delta; i++)
                {
                    Destroy(_visibleElements[_visibleElements.Count - 1].gameObject);
                    _visibleElements.RemoveAt(_visibleElements.Count - 1);
                }
            }

            float lastPosition = (_header != null) ? -_header.sizeDelta.y : 0f;
            for (int i = 0; i < _visibleElements.Count; i++)
            {
                UpdateElement(i, i);
                _visibleElements[i].SetPosition(lastPosition);
                CheckElementInReload(_visibleElements[i], i);
                lastPosition = _visibleElements[i].GetBottomPoint();
            }

            if (_visibleElements.Count < _minElementsCount)
            {
                for (int i = 0; i < _minElementsCount - _visibleElements.Count; i++)
                {
                    AddToBottom(_visibleElements.Count + i);
                }
            }

            SetContentSize();
            _currentItemCount = _visibleElements.Count;
            _topMostCellIndex = 0;
            _bottomMostCellIndex = _visibleElements.Count - 1;
        }

        /// <summary>
        /// If scrolling, call this function every frame
        /// </summary>
        /// <param name="normalizedPos"></param>
        void OnValueChangedListener(Vector2 normalizedPos)
        {
            Vector2 dir = content.anchoredPosition - _prevAnchoredPos;
            m_ContentStartPosition += OnValueChange(dir);
            _prevAnchoredPos = content.anchoredPosition;
        }

        /// <summary>
        /// If out of safe area bounds, move m_ContentStartPosition parameter
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        Vector2 OnValueChange(Vector2 direction)
        {
            if (_recycling || !_active || _visibleElements == null || _visibleElements.Count == 0) return Vector2.zero;

            if (direction.y > 0 && _visibleElements[_bottomMostCellIndex].MaxY() > _recyclableViewBounds.min.y)
            {
                return RecycleTopToBottom();
            }
            else if (direction.y < 0 && _visibleElements[_topMostCellIndex].MinY() < _recyclableViewBounds.max.y)
            {
                return RecycleBottomToTop();
            }

            return Vector2.zero;
        }

        /// <summary>
        /// Recycles cells from top to bottom in the List heirarchy
        /// </summary>
        Vector2 RecycleTopToBottom()
        {
            _recycling = true;

            float posY = 0;
            float yDelta = 0f;
            bool swaped = false;

            while (_visibleElements[_topMostCellIndex].MinY() > _recyclableViewBounds.max.y && _currentItemCount < _maxElementsCount)
            {
                posY = _visibleElements[_bottomMostCellIndex].GetBottomPoint();
                _visibleElements[_topMostCellIndex].SetPosition(posY);
                float delta = _visibleElements[_topMostCellIndex].GetElementSize();
                yDelta += delta;

                UpdateElement(_topMostCellIndex, _currentItemCount);

                _bottomMostCellIndex = _topMostCellIndex;
                _topMostCellIndex = (_topMostCellIndex + 1) % _visibleElements.Count;

                _currentItemCount++;
                yDelta += GetDeltaAfterRecycle(1, delta);
                swaped = true;
            }

            if (!swaped)
            {
                _recycling = false;
                return Vector2.zero;
            }

            _visibleElements.ForEach((RecyclableScrollElement cell) => cell.AddPosition(Vector2.up * yDelta));
            content.anchoredPosition -= Vector2.up * yDelta;
            SetContentSize();
            _recycling = false;
            return -new Vector2(0, yDelta);

        }

        /// <summary>
        /// Recycles cells from bottom to top in the List heirarchy
        /// </summary>
        Vector2 RecycleBottomToTop()
        {
            _recycling = true;

            float posY = 0;
            float yDelta = 0f;
            bool swaped = false;

            while (_visibleElements[_bottomMostCellIndex].MaxY() < _recyclableViewBounds.min.y && _currentItemCount > _visibleElements.Count)
            {
                _currentItemCount--;
                UpdateElement(_bottomMostCellIndex, _currentItemCount - _visibleElements.Count);
                float delta = _visibleElements[_bottomMostCellIndex].GetElementSize();
                yDelta += _visibleElements[_bottomMostCellIndex].GetElementSize();

                posY = _visibleElements[_topMostCellIndex].GetUpperPoint() + _visibleElements[_bottomMostCellIndex].GetElementSize();
                _visibleElements[_bottomMostCellIndex].SetPosition(posY);

                _topMostCellIndex = _bottomMostCellIndex;
                _bottomMostCellIndex = (_bottomMostCellIndex - 1 + _visibleElements.Count) % _visibleElements.Count;
                yDelta += GetDeltaAfterRecycle(-1, delta);
                swaped = true;
            }

            if (!swaped)
            {
                _recycling = false;
                return Vector2.zero;
            }

            _visibleElements.ForEach((RecyclableScrollElement cell) => cell.AddPosition(-Vector2.up * yDelta));
            content.anchoredPosition += Vector2.up * yDelta;
            SetContentSize();
            _recycling = false;
            return new Vector2(0, yDelta);
        }

        /// <summary>
        /// If there are no more elements, update all elements in scroll
        /// </summary>
        void UpdateFromUpToBottom()
        {
            int startIndex = (_maxElementsCount - _visibleElements.Count >= 0) ? _maxElementsCount - _visibleElements.Count : 0;
            int elementIndex = _topMostCellIndex;
            for (int i = startIndex; i < _maxElementsCount; i++)
            {
                if (elementIndex != _topMostCellIndex)
                {
                    int prevIndex = (elementIndex - 1 < 0) ? _visibleElements.Count - 1 : elementIndex - 1;
                    _visibleElements[elementIndex].SetPosition(_visibleElements[prevIndex].GetBottomPoint());
                }

                UpdateElement(elementIndex, i);
                CheckElementInReload(_visibleElements[elementIndex], i);
                elementIndex = (elementIndex + 1) % _visibleElements.Count;
            }

            if (_maxElementsCount - _visibleElements.Count < 0)
            {
                int delta = _visibleElements.Count - _maxElementsCount;
                for (int i = 0; i < delta; i++)
                {
                    Destroy(_visibleElements[_bottomMostCellIndex].gameObject);
                    _visibleElements.RemoveAt(_bottomMostCellIndex);
                    _bottomMostCellIndex = (_bottomMostCellIndex - 1 < 0) ? _visibleElements.Count - 1 : _bottomMostCellIndex - 1;
                    if (_topMostCellIndex != 0)
                    {
                        _topMostCellIndex = (_topMostCellIndex - 1 < 0) ? _visibleElements.Count - 1 : _topMostCellIndex - 1;
                    }
                }
            }

            SetContentSize();
            _currentItemCount--;
        }

        /// <summary>
        /// If there are more elements in content list, update from removing element to bottom
        /// </summary>
        /// <param name="element"></param>
        /// <param name="contentIndex"></param>
        void UpdateFromRemovingToBottom(RecyclableScrollElement element, int contentIndex)
        {
            int delIndex = _visibleElements.IndexOf(element);
            while (true)
            {
                if (delIndex != _topMostCellIndex)
                {
                    int prevIndex = (delIndex - 1 < 0) ? _visibleElements.Count - 1 : delIndex - 1;
                    _visibleElements[delIndex].SetPosition(_visibleElements[prevIndex].GetBottomPoint());
                }

                UpdateElement(delIndex, contentIndex);
                CheckElementInReload(_visibleElements[delIndex], contentIndex);
                delIndex = (delIndex + 1) % _visibleElements.Count;
                contentIndex++;

                if (delIndex == _bottomMostCellIndex)
                {
                    if (contentIndex < _currentItemCount)
                    {
                        int prevIndex = (delIndex - 1 < 0) ? _visibleElements.Count - 1 : delIndex - 1;
                        _visibleElements[delIndex].SetPosition(_visibleElements[prevIndex].GetBottomPoint());
                        UpdateElement(delIndex, contentIndex);
                        CheckElementInReload(_visibleElements[delIndex], contentIndex);
                    }

                    break;
                }
            }

            SetContentSize();
        }
    }
}
