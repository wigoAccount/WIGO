using System;
using System.Collections.Generic;
using UnityEngine;

namespace WIGO.Userinterface
{
    public class HashtagDoubleScroll : MonoBehaviour
    {
        [SerializeField] UIHashtagElement _hashtagPrefab;
        [SerializeField] RectTransform _content;
        [SerializeField] string[] _tempTags;

        Dictionary<UIHashtagElement, string> _selected = new Dictionary<UIHashtagElement, string>();
        Action _onCategorySelect;
        //const float SPACING = 8f;
        //const float SECOND_LINE_POS = -36f;

        public void CreateHashtags(Action onCategorySelect)
        {
            //int firstLineCount = Mathf.CeilToInt(_tempTags.Length / 2f);

            //float firstLineWidth = CreateHashtagsLine(0, firstLineCount, 0f);
            //float secondLineWidth = CreateHashtagsLine(firstLineCount, _tempTags.Length, SECOND_LINE_POS);
            //_content.sizeDelta = new Vector2(Mathf.Max(firstLineWidth, secondLineWidth), _content.sizeDelta.y);
            _onCategorySelect = onCategorySelect;
            CreateCategories();
        }

        public void Clear()
        {
            _selected.Clear();
            _content.DestroyChildren();
        }

        public IEnumerable<string> GetSelectedCategories() => _selected.Values;

        //float CreateHashtagsLine(int startIndex, int endIndex, float yPos)
        //{
        //    float xPos = 0f;

        //    for (int i = startIndex; i < endIndex; i++)
        //    {
        //        UIHashtagElement hashtag = Instantiate(_hashtagPrefab, _content);
        //        RectTransform tagRect = hashtag.transform as RectTransform;
        //        tagRect.anchoredPosition = new Vector2(xPos, yPos);
        //        hashtag.Setup(OnHashtagClick);
        //        hashtag.SetHashtag(_tempTags[i], out float width);
        //        xPos += width + SPACING;
        //    }

        //    return xPos - SPACING;
        //}

        void CreateCategories()
        {
            for (int i = 0; i < _tempTags.Length; i++)
            {
                UIHashtagElement hashtag = Instantiate(_hashtagPrefab, _content);
                hashtag.Setup(OnHashtagClick);
                hashtag.SetHashtag(_tempTags[i]);
            }
        }

        void OnHashtagClick(UISelectableElement element, bool selected)
        {
            UIHashtagElement hashtag = (UIHashtagElement)element;
            hashtag.SetSelected(selected);
            if (selected && !_selected.ContainsKey(hashtag))
            {
                _selected.Add(hashtag, hashtag.GetTag());
            }
            else if (!selected && _selected.ContainsKey(hashtag))
                _selected.Remove(hashtag);

            _onCategorySelect?.Invoke();
        }
    }
}
