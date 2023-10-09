using System;
using System.Collections.Generic;
using UnityEngine;

namespace WIGO.Userinterface
{
    public class HashtagDoubleScroll : MonoBehaviour
    {
        [SerializeField] UIHashtagElement _hashtagPrefab;
        [SerializeField] RectTransform _content;

        Dictionary<UIHashtagElement, int> _selected = new Dictionary<UIHashtagElement, int>();
        Action _onCategorySelect;

        public void CreateHashtags(Action onCategorySelect)
        {
            _onCategorySelect = onCategorySelect;
            CreateCategories();
        }

        public void Clear()
        {
            _selected.Clear();
            _content.DestroyChildren();
        }

        public IEnumerable<int> GetSelectedCategories() => _selected.Values;

        void CreateCategories()
        {
            var tags = ServiceLocator.Get<GameModel>().GetAvailableTags();

            foreach (var tag in tags)
            {
                UIHashtagElement hashtag = Instantiate(_hashtagPrefab, _content);
                hashtag.Setup(OnHashtagClick);
                hashtag.SetHashtag(tag);
            }
        }

        void OnHashtagClick(UISelectableElement element, bool selected)
        {
            UIHashtagElement hashtag = (UIHashtagElement)element;
            hashtag.SetSelected(selected);
            if (selected && !_selected.ContainsKey(hashtag))
            {
                _selected.Add(hashtag, hashtag.GetTag().uid);
            }
            else if (!selected && _selected.ContainsKey(hashtag))
                _selected.Remove(hashtag);

            _onCategorySelect?.Invoke();
        }
    }
}
