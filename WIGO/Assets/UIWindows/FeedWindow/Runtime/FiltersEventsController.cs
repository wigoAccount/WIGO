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
        [SerializeField] EventsFilterDatabase _database;

        List<CategoryFilterElement> _categories = new List<CategoryFilterElement>();
        CategoryFilterElement _selectedCategory;
        Action _onApplyCategory;
        EventCategory _appliedCategory = EventCategory.All;

        public EventCategory GetFilterCategory() => _appliedCategory;
        public bool FiltersApplied()
        {
            return _appliedCategory != EventCategory.All;
        }

        public void Initialize(Action onApplyCategory)
        {
            _onApplyCategory = onApplyCategory;

            var all = Instantiate(_categoryPrefab, _categoriesContent);
            all.Setup(EventCategory.All, GameConsts.GetCategoryLabel(EventCategory.All), OnSelectCategory);
            _categories.Add(all);
            all.SetSelected(true, false);
            _selectedCategory = all;

            foreach (var category in _database.GetAllCategories())
            {
                string label = GameConsts.GetCategoryLabel(category);
                var element = Instantiate(_categoryPrefab, _categoriesContent);
                element.Setup(category, label, OnSelectCategory);
                _categories.Add(element);
            }
        }

        public void ResetFilters()
        {
            foreach (var category in _categories)
            {
                category.SetSelected(false, false);
            }

            _appliedCategory = EventCategory.All;
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
