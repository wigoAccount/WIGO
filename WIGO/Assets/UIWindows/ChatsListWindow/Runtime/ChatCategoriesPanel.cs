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
        ChatCategory _currentCategory = ChatCategory.All;
        Coroutine _blockCoroutine;
        bool _isBuisy;

        const float SWITCH_DURATION = 0.32f;

        public ChatCategory GetOpenedCategory() => _currentCategory;

        public void Init(Action<ChatCategory> onCategorySelected)
        {
            _onCategorySelected = onCategorySelected;
            foreach (var category in _categories)
            {
                category.Setup(OnCategoryClick);
            }

            ResetCategories();
        }

        public void SetUnreadMessages(bool forEvent, int count)
        {
            var category = forEvent ? _categories[1] : _categories[2];
            category.SetUnreadLabel(count);
        }

        public void ResetCategories()
        {
            _currentCategory = ChatCategory.All;
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
            _currentCategory = category;
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
