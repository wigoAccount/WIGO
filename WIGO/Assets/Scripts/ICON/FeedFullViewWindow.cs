using Crystal;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeedFullViewWindow : MonoBehaviour
{
    [SerializeField] FeedScrollView _feedScroll;
    [SerializeField] FeedPostElement _postPrefab;
    [SerializeField] RectTransform _content;
    [SerializeField] SafeArea _safeArea;
    [SerializeField] int _contentSize;

    const int SCROLL_ITEMS_COUNT = 3;

    public void Setup()
    {
        float[] aspects = new float[] { 9f / 16f, 1f, 16f / 9f };
        List<FeedPostData> posts = new List<FeedPostData>();
        for (int i = 0; i < _contentSize; i++)
        {
            int rnd = Random.Range(0, aspects.Length);
            FeedPostData data = new FeedPostData()
            {
                photoColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f)),
                photoAspect = aspects[rnd],
                likesCount = Random.Range(0, 1000000),
                commentsCount = Random.Range(0, 10000)
            };
            posts.Add(data);
        }

        float height = 720f; //ServiceLocator.Get<UIManager>().GetCanvasSize().y;
        int itemsCount = Mathf.Min(SCROLL_ITEMS_COUNT, posts.Count);
        for (int i = 0; i < itemsCount; i++)
        {
            var item = Instantiate(_postPrefab, _content);
            RectTransform itemRect = item.transform as RectTransform;
            itemRect.sizeDelta = new Vector2(itemRect.sizeDelta.x, height);
            itemRect.anchoredPosition = Vector2.down * i * height;

            item.Initialize(0f, OnCommentsOpen);
            item.Setup(posts[i]);
        }

        _content.sizeDelta = new Vector2(_content.sizeDelta.x, itemsCount * height);
        _feedScroll.Setup(posts);
    }

    private void Start()
    {
        Setup();
    }

    void OnCommentsOpen(FeedPostData data)
    {

    }
}
