using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class FeedScrollView : ScrollRect
{
    List<FeedPostData> _cachedData;
    FeedPostElement[] _created;
    Tween _moveTween;

    int _currentIndex;
    int _dataIndex;
    int _startScrollDirection;
    bool _isDragging;

    const float VELOCITY_THRESHOLD = 130f;

    public void Setup(IReadOnlyList<FeedPostData> posts)
    {
        _cachedData = new List<FeedPostData>(posts);
        _currentIndex = 0;
        _dataIndex = 0;

        _created = new FeedPostElement[content.childCount];
        for (int i = 0; i < _created.Length; i++)
        {
            var item = content.GetChild(i).GetComponent<FeedPostElement>();
            _created[i] = item;
        }
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        if (content.anchoredPosition.y < 0.5f && eventData.delta.y < 0f)
        {
            //_updateManager.OnBeginDrag(eventData);
            return;
        }
        else if (content.anchoredPosition.y > content.sizeDelta.y - 720f - 0.5f && eventData.delta.y > 0f)
        {
            return;
        }

        CancelMove();
        _isDragging = true;
        inertia = true;
        _startScrollDirection = eventData.delta.y > 0f ? 1 : -1;

        base.OnBeginDrag(eventData);
    }

    public override void OnDrag(PointerEventData eventData)
    {
        if (!_isDragging)
        {
            //_updateManager.OnDrag(eventData);
            return;
        }

        base.OnDrag(eventData);
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        if (!_isDragging)
        {
            //_updateManager.OnEndDrag(eventData);
            return;
        }

        _isDragging = false;
        DefineScrollMove();
        
        base.OnEndDrag(eventData);
        inertia = false;
        _startScrollDirection = 0;
    }

    void DefineScrollMove()
    {
        if ((velocity.y > 0f && velocity.y < VELOCITY_THRESHOLD) || (velocity.y < 0f && velocity.y > -VELOCITY_THRESHOLD))
        {
            float currentItemPos = viewport.InverseTransformPoint(_created[_currentIndex].transform.position).y -
                _created[_currentIndex].GetTransform().sizeDelta.y / 2f;
            float currentItemDelta = Mathf.Abs(currentItemPos);

            int nextIndex = currentItemPos > 0f ? _currentIndex + 1 : _currentIndex - 1;
            float nextItemDelta = Mathf.Abs(viewport.InverseTransformPoint(_created[nextIndex].transform.position).y -
                _created[nextIndex].GetTransform().sizeDelta.y / 2f);

            if (currentItemDelta < nextItemDelta)
            {
                ScrollToCurrent();
            }
            else
            {
                if (nextIndex > _currentIndex)
                {
                    RecycleTopToBottom();
                }
                else
                {
                    RecycleBottomToTop();
                }
            }
        }
        else
        {
            int direction = velocity.y > 0f ? 1 : -1;
            if (direction != _startScrollDirection)
            {
                ScrollToCurrent();
                return;
            }

            if (velocity.y > 0f)
            {
                RecycleTopToBottom();
            }
            else
            {
                RecycleBottomToTop();
            }
        }
    }

    void RecycleTopToBottom()
    {
        _dataIndex++;
        if (_currentIndex == 0)
        {
            _currentIndex = 1;
            ScrollToCurrent();
            return;
        }
        else if (_dataIndex == _cachedData.Count - 1)
        {
            _currentIndex++;
            ScrollToCurrent();
            return;
        }

        var tmp = _created[0];
        for (int i = 0; i < _created.Length - 1; i++)
        {
            _created[i] = _created[i + 1];
        }

        _created[_created.Length - 1] = tmp;
        _created[_created.Length - 1].GetTransform().anchoredPosition -= Vector2.up * _created.Length * 720f;
        _created[_created.Length - 1].Setup(_cachedData[_dataIndex + 1]);

        foreach (var item in _created)
        {
            item.GetTransform().anchoredPosition += Vector2.up * 720f;
        }

        content.anchoredPosition -= Vector2.up * 720f;
        ScrollToCurrent();
    }

    void RecycleBottomToTop()
    {
        _dataIndex--;
        if (_currentIndex == 2)
        {
            _currentIndex = 1;
            ScrollToCurrent();
            return;
        }
        else if (_dataIndex == 0)
        {
            _currentIndex--;
            ScrollToCurrent();
            return;
        }

        var tmp = _created[_created.Length - 1];
        for (int i = _created.Length - 1; i > 0; i--)
        {
            _created[i] = _created[i - 1];
        }

        _created[0] = tmp;
        _created[0].GetTransform().anchoredPosition += Vector2.up * _created.Length * 720f;
        _created[0].Setup(_cachedData[_dataIndex - 1]);

        foreach (var item in _created)
        {
            item.GetTransform().anchoredPosition -= Vector2.up * 720f;
        }

        content.anchoredPosition += Vector2.up * 720f;
        ScrollToCurrent();
    }

    void ScrollToCurrent(Action callback = null)
    {
        float pos = -_created[_currentIndex].GetTransform().anchoredPosition.y;
        _moveTween = content.DOAnchorPosY(pos, 0.4f).OnComplete(() =>
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
