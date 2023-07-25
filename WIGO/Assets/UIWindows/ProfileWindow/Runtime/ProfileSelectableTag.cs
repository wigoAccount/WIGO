using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace WIGO.Userinterface
{
    public class ProfileSelectableTag : MonoBehaviour
    {
        [SerializeField] Image _background;
        [SerializeField] TMP_Text _label;
        [SerializeField] ContainerData _data;

        Tween _selectTween;
        Action<ContainerData, ProfileSelectableTag> _onTagSelect;

        //const float MIN_WIDTH = 58f;
        //const float PADDING = 20f;

        public int GetUID() => _data.uid;

        public void Setup(Action<ContainerData, ProfileSelectableTag> onCategorySelect)
        {
            _onTagSelect = onCategorySelect;
            _label.text = _data.name;
            //float width = Mathf.Clamp(_label.preferredWidth + 2 * PADDING, MIN_WIDTH, float.MaxValue);
            //_background.rectTransform.sizeDelta = new Vector2(width, _background.rectTransform.sizeDelta.y);
        }

        public void SetSelected(bool selected, bool animate = true)
        {
            CancelTween();
            Color color = selected ? UIGameColors.Blue : UIGameColors.transparent10;

            if (animate)
            {
                _selectTween = _background.DOColor(color, 0.16f).OnComplete(() => _selectTween = null);
                return;
            }

            _background.color = color;
        }

        public void OnSelectClick()
        {
            _onTagSelect?.Invoke(_data, this);
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
