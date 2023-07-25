using System;
using System.Collections;
using UnityEngine;

namespace WIGO.Userinterface
{
    public class ChatCategoriesPanel : MonoBehaviour
    {
        [SerializeField] UIChatCategoryElement[] _categories;

        Action<ChatCategory> _onCategorySelected;
        UIChatCategoryElement _selected;
        Coroutine _blockCoroutine;
        bool _isBuisy;

        const float SWITCH_DURATION = 0.32f;

        public void Init(Action<ChatCategory> onCategorySelected)
        {
            _onCategorySelected = onCategorySelected;
            foreach (var category in _categories)
            {
                category.Setup(OnCategoryClick);
            }

            ResetCategories();
        }

        public void SetUnreadMessages(int eventsUnreadCount, int requestsUnreadCount)
        {
            _categories[1].SetUnreadLabel(eventsUnreadCount);
            _categories[2].SetUnreadLabel(requestsUnreadCount);
        }

        public void ResetCategories()
        {
            if (_selected != _categories[0])
            {
                _selected?.SetSelected(false, false);
                _selected = _categories[0];
                _selected.SetSelected(true, false);
            }

            if (_blockCoroutine != null)
            {
                StopCoroutine(_blockCoroutine);
                _blockCoroutine = null;
                _isBuisy = false;
            }
        }

        void OnCategoryClick(UIChatCategoryElement element, ChatCategory category)
        {
            if (element == _selected || _isBuisy)
            {
                return;
            }

            _selected?.SetSelected(false);
            _selected = element;
            _selected.SetSelected(true);
            _onCategorySelected?.Invoke(category);

            _isBuisy = true;
            _blockCoroutine = StartCoroutine(DelayBlock());
        }

        IEnumerator DelayBlock()
        {
            yield return new WaitForSeconds(SWITCH_DURATION);
            _isBuisy = false;
            _blockCoroutine = null;
        }
    }
}
