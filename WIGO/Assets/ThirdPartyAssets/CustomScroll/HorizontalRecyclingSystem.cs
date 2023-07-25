using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WIGO.RecyclableScroll
{
    /// <summary>
    /// Recyling system for horizontal type.
    /// </summary>
    public class HorizontalRecyclingSystem : RecyclingSystem
    {
        int _rows;                                          // Assigned by constructor

        int _leftMostCellIndex;                             // Leftmost cell in the List
        int _rightMostCellIndex;                            // Rightmost cell in the List
        int _leftMostCellRow, _rightMostCellRow;            // Used for recyling in Grid layout. leftmost and rightmost row

        public HorizontalRecyclingSystem(RectTransform prototype, RectTransform viewport, RectTransform content, bool isGrid, int rows, float xSpacing, float ySpacing, ScrollSettings scrollSettings)
        {
            _viewport = viewport;
            _content = content;
            _isGrid = isGrid;
            _rows = isGrid ? rows : 1;
            _recyclableViewBounds = new Bounds();

            _horizontalSpacing = xSpacing;
            _cellWidth = prototype.sizeDelta.x + xSpacing;
            _cellHeight = prototype.sizeDelta.y + ySpacing;
            _scrollSettings = scrollSettings;
        }

        /// <summary>
        /// Using this method for clearing content in scroll
        /// </summary>
        public override void ClearContent()
        {
            base.ClearContent();
            _content.sizeDelta = new Vector2(_emptyContentSize, _content.sizeDelta.y);
        }

        #region INIT
        /// <summary>
        /// Corotuine for initiazation.
        /// Using coroutine for init because few UI stuff requires a frame to update
        /// </summary>
        /// <param name="onInitialized">callback when init done</param>
        /// <returns></returns>
        public override IEnumerator InitCoroutine(RectTransform prototype, Action onInitialized = null, int selectedIndex = 0)
        {
            //Setting up container and bounds
            SetLeftAnchor(_content);
            _content.anchoredPosition = Vector3.zero;
            yield return null;
            SetRecyclingBounds();

            //Cell Poool
            CreateCellPool(prototype, selectedIndex);
            _leftMostCellIndex = 0;
            _rightMostCellIndex = _cellPool.Count - 1;

            //Set content width according to no of coloums
            int coloums = Mathf.CeilToInt((float)_cellPool.Count / _rows);
            float contentXSize = coloums * _cellWidth - _horizontalSpacing + _emptyContentSize;
            _content.sizeDelta = new Vector2(contentXSize, _content.sizeDelta.y);
            SetLeftAnchor(_content);

            onInitialized?.Invoke();
        }

        public override void SetupHeader(RectTransform header)
        {
            base.SetupHeader(header);
            _emptyContentSize = _header == null ? 0f : _header.sizeDelta.x + 0f;
        }

        /// <summary>
        /// Sets the uppper and lower bounds for recycling cells.
        /// </summary>
        void SetRecyclingBounds()
        {
            _viewport.GetWorldCorners(_corners);
            float threshHold = _scrollSettings.recyclingThreshold * (_corners[2].x - _corners[0].x);
            //float headerWidth = _header == null ? 0 : _header.sizeDelta.x;
            _recyclableViewBounds.min = new Vector3(_corners[0].x - threshHold /*+ headerWidth*/, _corners[0].y);
            _recyclableViewBounds.max = new Vector3(_corners[2].x + threshHold /*+ headerWidth*/, _corners[2].y);
        }

        /// <summary>
        /// Creates cell Pool for recycling, Caches ICells
        /// </summary>
        void CreateCellPool(RectTransform prototype, int selectedIndex = 0)
        {
            //Reseting Pool
            if (_cellPool != null)
            {
                _cellPool.ForEach((RectTransform item) => GameObject.Destroy(item.gameObject));
                _cellPool.Clear();
            }
            else
            {
                _cellPool = new List<RectTransform>();
            }

            //Set cell anchor as top
            SetLeftAnchor(prototype);

            //Reset
            _leftMostCellRow = _rightMostCellRow = 0;

            //Temps
            float currentPoolCoverage = 0;
            int poolSize = 0;
            int index = 0;
            float posX = _header == null ? 0 : _header.sizeDelta.x;
            float posY = 0;

            //Get the required pool coverage and mininum size for the Cell pool
            float requriedCoverage = _scrollSettings.minPoolCoverage * _viewport.rect.width;
            int minPoolSize = Math.Min(_scrollSettings.minPoolSize, GetDataCount());
            int itemsCount = GetDataCount() - selectedIndex;
            float expectedCoverage = _cellWidth * itemsCount;
            int itemsNeedToCoverage = Mathf.CeilToInt(requriedCoverage / _cellWidth);
            int needItemsCount = Mathf.Max(itemsNeedToCoverage, minPoolSize);

            index = (itemsCount >= minPoolSize && expectedCoverage > requriedCoverage) ? selectedIndex : Mathf.Clamp(GetDataCount() - needItemsCount, 0, int.MaxValue);

            //create cells untill the Pool area is covered and pool size is the minimum required
            while ((poolSize < minPoolSize || currentPoolCoverage < requriedCoverage) && index < GetDataCount())
            {
                //Instantiate and add to Pool
                RectTransform item = onCreateCell?.Invoke(index);
                _cellPool.Add(item);

                if (_isGrid)
                {
                    posY = -_rightMostCellRow * _cellHeight;
                    item.anchoredPosition = new Vector2(posX + item.pivot.x * item.sizeDelta.x, posY);
                    if (++_rightMostCellRow >= _rows)
                    {
                        _rightMostCellRow = 0;
                        posX += (_cellWidth - item.pivot.x * item.sizeDelta.x);
                        currentPoolCoverage += _cellWidth;
                    }
                }
                else
                {
                    item.anchoredPosition = new Vector2(posX + item.pivot.x * item.sizeDelta.x, 0);
                    posX = item.anchoredPosition.x - item.pivot.x * item.sizeDelta.x + _cellWidth;
                    currentPoolCoverage += _cellWidth;
                }

                //Update the Pool size
                poolSize++;
                index++;
            }

            if (_isGrid)
            {
                _rightMostCellRow = (_rightMostCellRow - 1 + _rows) % _rows;
            }

            _currentItemCount = index;
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

            if (direction.x < 0 && _cellPool[_rightMostCellIndex].MinX() < _recyclableViewBounds.max.x)
            {
                return RecycleLeftToRight();
            }
            else if (direction.x > 0 && _cellPool[_leftMostCellIndex].MaxX() > _recyclableViewBounds.min.x)
            {
                return RecycleRightToleft();
            }
            return zeroVector;
        }

        /// <summary>
        /// Recycles cells from Left to Right in the List heirarchy
        /// </summary>
        Vector2 RecycleLeftToRight()
        {
            _recycling = true;

            int n = 0;
            float posX = _isGrid ? _cellPool[_rightMostCellIndex].anchoredPosition.x : 0;
            float posY = 0;

            //to determine if content size needs to be updated
            int additionalColoums = 0;

            //Recycle until cell at left is avaiable and current item count smaller than datasource
            while (_cellPool[_leftMostCellIndex].MaxX() < _recyclableViewBounds.min.x && _currentItemCount < GetDataCount())
            {
                if (_isGrid)
                {
                    if (++_rightMostCellRow >= _rows)
                    {
                        n++;
                        _rightMostCellRow = 0;
                        posX = _cellPool[_rightMostCellIndex].anchoredPosition.x + _cellWidth;
                        additionalColoums++;
                    }

                    //Move Left most cell to right
                    posY = -_rightMostCellRow * _cellHeight;
                    _cellPool[_leftMostCellIndex].anchoredPosition = new Vector2(posX, posY);

                    if (++_leftMostCellRow >= _rows)
                    {
                        _leftMostCellRow = 0;
                        additionalColoums--;
                    }
                }
                else
                {
                    //Move Left most cell to right
                    posX = _cellPool[_rightMostCellIndex].anchoredPosition.x + _cellPool[_rightMostCellIndex].sizeDelta.x + _horizontalSpacing;
                    _cellPool[_leftMostCellIndex].anchoredPosition = new Vector2(posX, _cellPool[_leftMostCellIndex].anchoredPosition.y);
                }

                //Cell for row at
                onSetCell?.Invoke(_leftMostCellIndex, _currentItemCount);

                //set new indices
                _rightMostCellIndex = _leftMostCellIndex;
                _leftMostCellIndex = (_leftMostCellIndex + 1) % _cellPool.Count;

                _currentItemCount++;
                if (!_isGrid) n++;
            }

            //Content size adjustment 
            if (_isGrid)
            {
                _content.sizeDelta += additionalColoums * Vector2.right * _cellWidth;
                if (additionalColoums > 0)
                {
                    n -= additionalColoums;
                }
            }

            //Content anchor position adjustment.
            _cellPool.ForEach((RectTransform cell) => cell.anchoredPosition -= n * Vector2.right * _cellWidth);
            _content.anchoredPosition += n * Vector2.right * _cellWidth;
            _recycling = false;
            return n * Vector2.right * _cellWidth;

        }

        /// <summary>
        /// Recycles cells from Right to Left in the List heirarchy
        /// </summary>
        Vector2 RecycleRightToleft()
        {
            _recycling = true;

            int n = 0;
            float posX = _isGrid ? _cellPool[_leftMostCellIndex].anchoredPosition.x : 0;
            float posY = 0;

            //to determine if content size needs to be updated
            int additionalColoums = 0;
            //Recycle until cell at Right end is avaiable and current item count is greater than cellpool size
            while (_cellPool[_rightMostCellIndex].MinX() - _horizontalSpacing > _recyclableViewBounds.max.x && _currentItemCount > _cellPool.Count)
            {
                if (_isGrid)
                {
                    if (--_leftMostCellRow < 0)
                    {
                        n++;
                        _leftMostCellRow = _rows - 1;
                        posX = _cellPool[_leftMostCellIndex].anchoredPosition.x - _cellWidth;
                        additionalColoums++;
                    }

                    //Move Right most cell to left
                    posY = -_leftMostCellRow * _cellHeight;
                    _cellPool[_rightMostCellIndex].anchoredPosition = new Vector2(posX, posY);

                    if (--_rightMostCellRow < 0)
                    {
                        _rightMostCellRow = _rows - 1;
                        additionalColoums--;
                    }
                }
                else
                {
                    //Move Right most cell to left
                    posX = _cellPool[_leftMostCellIndex].anchoredPosition.x - _cellPool[_leftMostCellIndex].sizeDelta.x - _horizontalSpacing;
                    _cellPool[_rightMostCellIndex].anchoredPosition = new Vector2(posX, _cellPool[_rightMostCellIndex].anchoredPosition.y);
                    n++;
                }

                _currentItemCount--;
                //Cell for row at
                onSetCell?.Invoke(_rightMostCellIndex, _currentItemCount - _cellPool.Count);

                //set new indices
                _leftMostCellIndex = _rightMostCellIndex;
                _rightMostCellIndex = (_rightMostCellIndex - 1 + _cellPool.Count) % _cellPool.Count;
            }

            //Content size adjustment
            if (_isGrid)
            {
                _content.sizeDelta += additionalColoums * Vector2.right * _cellWidth;
                if (additionalColoums > 0)
                {
                    n -= additionalColoums;
                }
            }

            //Content anchor position adjustment.
            _cellPool.ForEach((RectTransform cell) => cell.anchoredPosition += n * Vector2.right * _cellWidth);
            _content.anchoredPosition -= n * Vector2.right * _cellWidth;
            _recycling = false;
            return -n * Vector2.right * _cellWidth;
        }
        #endregion

        #region  HELPERS
        /// <summary>
        /// Anchoring cell and content rect transforms to top preset. Makes repositioning easy.
        /// </summary>
        /// <param name="rectTransform"></param>
        void SetLeftAnchor(RectTransform rectTransform)
        {
            //Saving to reapply after anchoring. Width and height changes if anchoring is change. 
            float width = rectTransform.rect.width;
            float height = rectTransform.rect.height;

            Vector2 pos = _isGrid ? new Vector2(0f, 1f) : new Vector2(0f, 0.5f);
            Vector2 pivot = _isGrid ? new Vector2(rectTransform.pivot.x, 1) : new Vector2(rectTransform.pivot.x, 0.5f);

            //Setting top anchor 
            rectTransform.anchorMin = pos;
            rectTransform.anchorMax = pos;
            rectTransform.pivot = pivot;

            //Reapply size
            rectTransform.sizeDelta = new Vector2(width, height);
        }
        #endregion

        #region  TESTING
        public void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(_recyclableViewBounds.min - new Vector3(0, 2000), _recyclableViewBounds.min + new Vector3(0, 2000));
            Gizmos.color = Color.red;
            Gizmos.DrawLine(_recyclableViewBounds.max - new Vector3(0, 2000), _recyclableViewBounds.max + new Vector3(0, 2000));
        }
        #endregion
    }
}
