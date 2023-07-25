using System;
using UnityEngine;
using UnityEngine.UI;

public class FeedPostElement : MonoBehaviour
{
    [SerializeField] RawImage _photo;
    [SerializeField] RectTransform _bottomPanel;

    FeedPostData _postData;
    Action<FeedPostData> _onCommentsOpen;
    RectTransform _postRect;

    public RectTransform GetTransform() => _postRect;

    public void Initialize(float padding, Action<FeedPostData> onCommentsOpen)
    {
        _onCommentsOpen = onCommentsOpen;
        _postRect = transform as RectTransform;
        _bottomPanel.anchoredPosition += Vector2.up * padding;
    }

    public void Setup(FeedPostData postData)
    {
        _postData = postData;

        SetupData();
    }

    public void OnCommentsOpen()
    {
        _onCommentsOpen?.Invoke(_postData);
    }

    void SetupData()
    {
        float height = _photo.rectTransform.rect.width / _postData.photoAspect;
        _photo.rectTransform.sizeDelta = new Vector2(_photo.rectTransform.sizeDelta.x, height);
        _photo.color = _postData.photoColor;
    }
}
