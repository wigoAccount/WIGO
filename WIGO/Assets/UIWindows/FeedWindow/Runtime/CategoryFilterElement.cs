using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;

namespace WIGO.Userinterface
{
    public enum EventCategory
    {
        All,
        Party,
        Outside,
        Sport,
        Other
    }

    public class CategoryFilterElement : MonoBehaviour
    {
        [SerializeField] Image _background;
        [SerializeField] TMP_Text _label;

        Tween _selectTween;
        Action<CategoryFilterElement> _onCategorySelect;
        EventCategory _category;

        const float MIN_WIDTH = 58f;
        const float PADDING = 20f;

        public EventCategory GetCategory() => _category;

        public void Setup(EventCategory category, string label, Action<CategoryFilterElement> onCategorySelect)
        {
            _onCategorySelect = onCategorySelect;
            _category = category;
            _label.text = label;
            float width = Mathf.Clamp(_label.preferredWidth + 2 * PADDING, MIN_WIDTH, float.MaxValue);
            _background.rectTransform.sizeDelta = new Vector2(width, _background.rectTransform.sizeDelta.y);
        }

        public void SetSelected(bool selected, bool animate = true)
        {
            CancelTween();
            Color color = selected ? UIGameColors.Blue : UIGameColors.transparent20;

            if (animate)
            {
                _selectTween = _background.DOColor(color, 0.16f).OnComplete(() => _selectTween = null);
                return;
            }

            _background.color = color;
        }

        public void OnSelectClick()
        {
            _onCategorySelect?.Invoke(this);
        }

        void CancelTween()
        {
            if (_selectTween != null)
            {
                _selectTween.Kill();
                _selectTween = null;
            }
        }
    }
}
