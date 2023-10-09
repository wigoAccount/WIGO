using System;
using System.Collections.Generic;
using UnityEngine;
using WIGO.Core;

namespace WIGO.Userinterface
{
    public class FiltersEventsController : MonoBehaviour
    {
        [SerializeField] CategoryFilterElement _categoryPrefab;
        [SerializeField] RectTransform _categoriesContent;
        [SerializeField] string _categoryAllName;

        List<CategoryFilterElement> _categories = new List<CategoryFilterElement>();
        CategoryFilterElement _selectedCategory;
        Action _onApplyCategory;
        int _appliedCategory = 0;

        public int GetFilterCategory() => _appliedCategory;
        public bool FiltersApplied()
        {
            return _appliedCategory != 0;
        }

        public void Initialize(Action onApplyCategory)
        {
            _onApplyCategory = onApplyCategory;

            var model = ServiceLocator.Get<GameModel>();
            var all = Instantiate(_categoryPrefab, _categoriesContent);
            all.Setup(0, _categoryAllName, OnSelectCategory);
            _categories.Add(all);
            all.SetSelected(true, false);
            _selectedCategory = all;

            foreach (var category in model.GetAvailableTags())
            {
                string label = category.name;
                var element = Instantiate(_categoryPrefab, _categoriesContent);
                element.Setup(category.uid, label, OnSelectCategory);
                _categories.Add(element);
            }
        }

        public void ResetFilters()
        {
            foreach (var category in _categories)
            {
                category.SetSelected(false, false);
            }

            _appliedCategory = 0;
            _selectedCategory = _categories[0];
            _selectedCategory.SetSelected(true, false);
        }

        void OnSelectCategory(CategoryFilterElement category)
        {
            if (category == _selectedCategory)
            {
                return;
            }

            _selectedCategory?.SetSelected(false);
            _selectedCategory = category;
            category.SetSelected(true);
            _appliedCategory = category.GetCategory();

            _onApplyCategory?.Invoke();
        }
    }
}
