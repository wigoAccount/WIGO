using System;
using UnityEngine;
using UnityEngine.UI;

namespace WIGO.Userinterface
{
    public class UISelectableElement : MonoBehaviour
    {
        [SerializeField] GraphicElement[] _elements;

        Action<UISelectableElement, bool> _onSelect;
        bool _selected;

        public void Setup(Action<UISelectableElement, bool> onSelect, bool selected = false)
        {
            _onSelect = onSelect;
            _selected = selected;
            foreach (var element in _elements)
            {
                element.SetSelected(selected);
            }
        }

        public void OnElementClick()
        {
            _onSelect?.Invoke(this, !_selected);
        }

        public void SetSelected(bool selected)
        {
            _selected = selected;
            foreach (var element in _elements)
            {
                element.SetSelected(selected);
            }
        }
    }

    [Serializable]
    public class GraphicElement
    {
        [SerializeField] MaskableGraphic _element;
        [SerializeField] Color _selectColor;
        [SerializeField] Color _deselectColor;

        public void SetSelected(bool selected)
        {
            _element.color = selected ? _selectColor : _deselectColor;
        }
    }
}
