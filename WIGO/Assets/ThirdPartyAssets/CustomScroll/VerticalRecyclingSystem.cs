using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIGO.RecyclableScroll
{
    /// <summary>
    /// Recyling system for Vertical type.
    /// </summary>
    public class VerticalRecyclingSystem : RecyclingSystem
    {
        int _coloumns;                                                  // Assigned by constructor

        int topMostCellIndex;                                           // Topmost cell in the heirarchy
        int bottomMostCellIndex;                                        // Bottommost cell in the heirarchy
        int _topMostCellColoumn, _bottomMostCellColoumn;                // Used for recyling in Grid layout. top-most and bottom-most coloumn
        float _filledContentHeight;

        public VerticalRecyclingSystem(RectTransform prototype, RectTransform viewport, RectTransform content, 
            bool isGrid, int coloumns, float xSpacing, float ySpacing, ScrollSettings scrollSettings)
        {
            _viewport = viewport;
            _content = content;
            _isGrid = isGrid;
            _coloumns = isGrid ? coloumns : 1;
            _recyclableViewBounds = new Bounds();

            _verticalSpacing = ySpacing;
            _cellWidth = prototype.sizeDelta.x + xSpacing;
            _cellHeight = prototype.sizeDelta.y + ySpacing;
            _scrollSettings = scrollSettings;
        }

        public override void SetupHeader(RectTransform header)
        {
            base.SetupHeader(header);
            _emptyContentSize = _header == null ? 0f : _header.sizeDelta.y + 0f;
        }

        /// <summary>
        /// Using this method for clearing content in scroll
        /// </summary>
        public override void ClearContent()
        {
            base.ClearContent();
            _filledContentHeight = _emptyContentSize;
            _content.sizeDelta = new Vector2(_content.sizeDelta.x, _emptyContentSize);
        }

        #region INIT
        /// <summary>
        /// Corotuine for initiazation.
        /// Using coroutine for init because few UI stuff requires a frame to update
        /// </summary>
        /// <param name="onInitialized">callback when init done</param>
        /// <returns></returns>>
        public override IEnumerator InitCoroutine(RectTransform prototype, Action onInitialized = null, int selectedIndex = 0)
        {
            SetTopAnchor(_content);
            _content.anchoredPosition = Vector3.zero;
            yield return null;
            //_emptyContentSize = _header == null ? 0f : _header.sizeDelta.y;
            SetRecyclingBounds();

            //Cell Poool
            CreateCellPool(prototype);
            _currentItemCount = _cellPool.Count;
            topMostCellIndex = _scrollSettings.reverse ? _cellPool.Count - 1 : 0;
            bottomMostCellIndex = _scrollSettings.reverse ? 0 : _cellPool.Count - 1;

            //Set content height according to no of rows
            int noOfRows = (int)Mathf.Ceil((float)_cellPool.Count / (float)_coloumns);
            float coverage = _isGrid ? noOfRows * _cellHeight : _filledContentHeight;
            float contentYSize = coverage /*- _verticalSpacing*/ + _emptyContentSize;
            _content.sizeDelta = new Vector2(_content.sizeDelta.x, contentYSize);
            SetTopAnchor(_content);

            onInitialized?.Invoke();
        }

        /// <summary>
        /// Sets the uppper and lower bounds for recycling cells.
        /// </summary>
        void SetRecyclingBounds()
        {
            _viewport.GetWorldCorners(_corners);
            float threshHold = _scrollSettings.recyclingThreshold * (_corners[2].y - _corners[0].y);
            _recyclableViewBounds.min = new Vector3(_corners[0].x, _corners[0].y - threshHold);
            _recyclableViewBounds.max = new Vector3(_corners[2].x, _corners[2].y + threshHold);
        }

        /// <summary>
        /// Creates cell Pool for recycling, Caches ICells
        /// </summary>
        void CreateCellPool(RectTransform prototype)
        {
            // Reseting Pool
            if (_cellPool != null)
            {
                _cellPool.ForEach((RectTransform item) => GameObject.Destroy(item.gameObject));
                _cellPool.Clear();
            }
            else
            {
                _cellPool = new List<RectTransform>();
            }

            if (_isGrid)
            {
                SetTopLeftAnchor(prototype);
            }
            else
            {
                SetTopAnchor(prototype);
            }

            // Reset
            _topMostCellColoumn = _bottomMostCellColoumn = 0;
            _filledContentHeight = 0f;

            // Temps
            float currentPoolCoverage = 0;
            int poolSize = 0;
            float posX = 0;
            float posY = _header == null ? 0 : -_header.sizeDelta.y;

            // Get the required pool coverage and mininum size for the Cell pool
            float requriedCoverage = _scrollSettings.minPoolCoverage * _viewport.rect.height;
            int minPoolSize = Math.Min(_scrollSettings.minPoolSize, GetDataCount());
            int direction = _scrollSettings.reverse ? -1 : 1;

            // create cells untill the Pool area is covered and pool size is the minimum required
            while ((poolSize < minPoolSize || currentPoolCoverage < requriedCoverage) && poolSize < GetDataCount())
            {
                // Instantiate and add to Pool
                RectTransform item = onCreateCell?.Invoke(poolSize);
                _cellPool.Add(item);

                if (_isGrid)
                {
                    posX = _bottomMostCellColoumn * _cellWidth;
                    item.anchoredPosition = new Vector2(posX, posY);
                    if (++_bottomMostCellColoumn >= _coloumns)
                    {
                        _bottomMostCellColoumn = 0;
                        posY -= _cellHeight;
                        currentPoolCoverage += _cellHeight;
                    }
                }
                else
                {
                    item.anchoredPosition = new Vector2(0, posY);
                    posY = item.anchoredPosition.y - (item.sizeDelta.y + _verticalSpacing) * direction;
                    currentPoolCoverage += _cellHeight;
                    _filledContentHeight += item.sizeDelta.y + _verticalSpacing;
                }

                //Update the Pool size
                poolSize++;
            }

            // [TODO]: you already have a _currentColoumn varaiable. Why this calculation?????
            if (_isGrid)
            {
                _bottomMostCellColoumn = (_bottomMostCellColoumn - 1 + _coloumns) % _coloumns;
            }
        }
        #endregion

        #region RECYCLING
        /// <summary>
        /// Recyling entry point
        /// </summary>
        /// <param name="direction">scroll direction </param>
        /// <returns></returns>
        public override Vector2 OnValueChangedListener(Vector2 direction)
        {
            if (_recycling || _cellPool == null || _cellPool.Count == 0) return zeroVector;

            //Updating Recyclable view bounds since it can change with resolution changes.
            SetRecyclingBounds();

            if (_scrollSettings.reverse)
            {
                if (direction.y > 0 && _cellPool[topMostCellIndex].MinY() > _recyclableViewBounds.max.y)
                {
                    return RecycleTopToBottom();
                }
                else if (direction.y < 0 && _cellPool[bottomMostCellIndex].MaxY() < _recyclableViewBounds.min.y)
                {
                    return RecycleBottomToTop();
                }
            }
            else
            {
                if (direction.y > 0 && _cellPool[bottomMostCellIndex].MinY() > _recyclableViewBounds.min.y)
                {
                    return RecycleTopToBottom();
                }
                else if (direction.y < 0 && _cellPool[topMostCellIndex].MaxY() < _recyclableViewBounds.max.y)
                {
                    return RecycleBottomToTop();
                }
            }

            return zeroVector;
        }

        /// <summary>
        /// Recycles cells from top to bottom in the List heirarchy
        /// </summary>
        Vector2 RecycleTopToBottom()
        {
            _recycling = true;

            float posY = _isGrid ? _cellPool[bottomMostCellIndex].anchoredPosition.y : 0;
            float posX = 0;

            //to determine if content size needs to be updated
            int additionalRows = 0;
            float deltaY = 0f;
            //Recycle until cell at Top is avaiable and current item count smaller than datasource
            while (_cellPool[topMostCellIndex].MinY() > _recyclableViewBounds.max.y && IsNotEndOfContentTopToBottom())//_currentItemCount < GetDataCount())
            {
                if (_scrollSettings.reverse)
                {
                    _currentItemCount--;
                    onSetCell?.Invoke(topMostCellIndex, _currentItemCount - _cellPool.Count);
                }

                if (_isGrid)
                {
                    if (++_bottomMostCellColoumn >= _coloumns)
                    {
                        deltaY += _cellHeight;
                        _bottomMostCellColoumn = 0;
                        posY = _cellPool[bottomMostCellIndex].anchoredPosition.y - _cellHeight;
                        additionalRows++;
                    }

                    //Move top cell to bottom
                    posX = _bottomMostCellColoumn * _cellWidth;
                    _cellPool[topMostCellIndex].anchoredPosition = new Vector2(posX, posY);

                    if (++_topMostCellColoumn >= _coloumns)
                    {
                        _topMostCellColoumn = 0;
                        additionalRows--;
                    }
                }
                else
                {
                    //Move top cell to bottom
                    posY = _scrollSettings.reverse
                        ? _cellPool[bottomMostCellIndex].anchoredPosition.y - _cellPool[topMostCellIndex].sizeDelta.y - _verticalSpacing
                        : _cellPool[bottomMostCellIndex].anchoredPosition.y - _cellPool[bottomMostCellIndex].sizeDelta.y - _verticalSpacing;
                    _cellPool[topMostCellIndex].anchoredPosition = new Vector2(_cellPool[topMostCellIndex].anchoredPosition.x, posY);
                    deltaY += _cellPool[topMostCellIndex].sizeDelta.y + _verticalSpacing;
                }

                if (!_scrollSettings.reverse)
                {
                    onSetCell?.Invoke(topMostCellIndex, _currentItemCount);
                    _currentItemCount++;
                }

                

                //set new indices
                bottomMostCellIndex = topMostCellIndex;
                topMostCellIndex = _scrollSettings.reverse
                    ? (topMostCellIndex - 1 + _cellPool.Count) % _cellPool.Count
                    : (topMostCellIndex + 1) % _cellPool.Count;
            }

            //Content size adjustment 
            if (_isGrid)
            {
                _content.sizeDelta += additionalRows * Vector2.up * _cellHeight;
                //TODO : check if it is supposed to be done only when > 0
                if (additionalRows > 0)
                {
                    deltaY -= additionalRows * _cellHeight;
                }
            }

            //Content anchor position adjustment.
            _cellPool.ForEach((RectTransform cell) => cell.anchoredPosition += Vector2.up * deltaY);
            _content.anchoredPosition -= Vector2.up * deltaY;
            AdaptContentSize();
            _recycling = false;

            return -new Vector2(0, deltaY);

        }

        /// <summary>
        /// Recycles cells from bottom to top in the List heirarchy
        /// </summary>
        Vector2 RecycleBottomToTop()
        {
            _recycling = true;

            float posY = _isGrid ? _cellPool[topMostCellIndex].anchoredPosition.y : 0;
            float posX = 0;

            //to determine if content size needs to be updated
            int additionalRows = 0;
            float deltaY = 0f;
            //Recycle until cell at bottom is avaiable and current item count is greater than cellpool size
            while (_cellPool[bottomMostCellIndex].MaxY() - _verticalSpacing < _recyclableViewBounds.min.y && IsNotEndOfContentBottomToTop())// _currentItemCount > _cellPool.Count)
            {
                if (!_scrollSettings.reverse)
                {
                    _currentItemCount--;
                    onSetCell?.Invoke(bottomMostCellIndex, _currentItemCount - _cellPool.Count);
                }

                if (_isGrid)
                {
                    if (--_topMostCellColoumn < 0)
                    {
                        deltaY += _cellHeight;
                        _topMostCellColoumn = _coloumns - 1;
                        posY = _cellPool[topMostCellIndex].anchoredPosition.y + _cellHeight;
                        additionalRows++;
                    }

                    //Move bottom cell to top
                    posX = _topMostCellColoumn * _cellWidth;
                    _cellPool[bottomMostCellIndex].anchoredPosition = new Vector2(posX, posY);

                    if (--_bottomMostCellColoumn < 0)
                    {
                        _bottomMostCellColoumn = _coloumns - 1;
                        additionalRows--;
                    }
                }
                else
                {
                    //Move bottom cell to top
                    posY = _scrollSettings.reverse
                        ? _cellPool[topMostCellIndex].anchoredPosition.y + _cellPool[topMostCellIndex].sizeDelta.y + _verticalSpacing
                        : _cellPool[topMostCellIndex].anchoredPosition.y + _cellPool[bottomMostCellIndex].sizeDelta.y + _verticalSpacing;
                    _cellPool[bottomMostCellIndex].anchoredPosition = new Vector2(_cellPool[bottomMostCellIndex].anchoredPosition.x, posY);
                    deltaY += _cellPool[bottomMostCellIndex].sizeDelta.y + _verticalSpacing;
                }

                if (_scrollSettings.reverse)
                {
                    onSetCell?.Invoke(bottomMostCellIndex, _currentItemCount);
                    _currentItemCount++;
                }

                //set new indices
                topMostCellIndex = bottomMostCellIndex;
                bottomMostCellIndex = _scrollSettings.reverse 
                    ? (bottomMostCellIndex + 1) % _cellPool.Count
                    : (bottomMostCellIndex - 1 + _cellPool.Count) % _cellPool.Count;
            }

            if (_isGrid)
            {
                _content.sizeDelta += additionalRows * Vector2.up * _cellHeight;
                // [TODO]: check if it is supposed to be done only when > 0
                if (additionalRows > 0)
                {
                    deltaY -= additionalRows * _cellHeight;
                }
            }

            _cellPool.ForEach((RectTransform cell) => cell.anchoredPosition -= Vector2.up * deltaY);
            _content.anchoredPosition += Vector2.up * deltaY;
            AdaptContentSize();
            _recycling = false;

            return new Vector2(0, deltaY);
        }
        #endregion

        #region  HELPERS
        bool IsNotEndOfContentTopToBottom()
        {
            return _scrollSettings.reverse ? _currentItemCount > _cellPool.Count : _currentItemCount < GetDataCount();
        }

        bool IsNotEndOfContentBottomToTop()
        {
            return _scrollSettings.reverse ? _currentItemCount < GetDataCount() : _currentItemCount > _cellPool.Count;
        }

        /// <summary>
        /// Anchoring cell and content rect transforms to top preset. Makes repositioning easy.
        /// </summary>
        /// <param name="rectTransform"></param>
        void SetTopAnchor(RectTransform rectTransform)
        {
            float y = _scrollSettings.reverse ? 0f : 1f;
            //Saving to reapply after anchoring. Width and height changes if anchoring is change. 
            float width = rectTransform.rect.width;
            float height = rectTransform.rect.height;

            //Setting top anchor 
            rectTransform.anchorMin = new Vector2(0.5f, y);
            rectTransform.anchorMax = new Vector2(0.5f, y);
            rectTransform.pivot = new Vector2(0.5f, y);

            //Reapply size
            rectTransform.sizeDelta = new Vector2(width, height);
        }

        void SetTopLeftAnchor(RectTransform rectTransform)
        {
            //Saving to reapply after anchoring. Width and height changes if anchoring is change. 
            float width = rectTransform.rect.width;
            float height = rectTransform.rect.height;

            //Setting top anchor 
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 1);

            //Reapply size
            rectTransform.sizeDelta = new Vector2(width, height);
        }

        void AdaptContentSize()
        {
            if (_isGrid)
                return;

            if (_cellPool.Count == 0)
            {
                _content.sizeDelta = new Vector2(_content.sizeDelta.x, _emptyContentSize);
                return;
            }

            float coverage = _emptyContentSize;
            foreach (var cell in _cellPool)
            {
                coverage += cell.sizeDelta.y + _verticalSpacing;
            }

            _content.sizeDelta = new Vector2(_content.sizeDelta.x, coverage);
        }
        #endregion

        #region TESTING
        public void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(_recyclableViewBounds.min - new Vector3(2000, 0), _recyclableViewBounds.min + new Vector3(2000, 0));
            Gizmos.color = Color.red;
            Gizmos.DrawLine(_recyclableViewBounds.max - new Vector3(2000, 0), _recyclableViewBounds.max + new Vector3(2000, 0));
        }
        #endregion
    }
}
