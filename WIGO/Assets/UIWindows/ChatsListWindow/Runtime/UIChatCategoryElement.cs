using System;
using TMPro;
using UnityEngine;
using DG.Tweening;

namespace WIGO.Userinterface
{
    public enum ChatCategory
    {
        All,
        MyEvents,
        MyRequests
    }

    public class UIChatCategoryElement : MonoBehaviour
    {
        [SerializeField] ChatCategory _category;
        [SerializeField] TMP_Text _label;
        [SerializeField] RectTransform _selector;
        [SerializeField] GameObject _notificator;
        [SerializeField] TMP_Text _notificatorCountLabel;

        Tween _animation;
        Action<UIChatCategoryElement, ChatCategory> _onCategoryClick;
        float _selectorWidth;

        public void Setup(Action<UIChatCategoryElement, ChatCategory> onClickCallback)
        {
            _onCategoryClick = onClickCallback;
            _selectorWidth = _selector.sizeDelta.x;
        }

        public void OnSelectCategory()
        {
            _onCategoryClick?.Invoke(this, _category);
        }

        public void SetSelected(bool selected, bool animate = true)
        {
            CancelAnimation();

            _label.color = selected ? UIGameColors.Blue : Color.white;
            if (!animate)
            {
                _selector.sizeDelta = new Vector2(_selectorWidth, _selector.sizeDelta.y);
                _selector.gameObject.SetActive(selected);
                return;
            }

            if (selected)
            {
                _selector.sizeDelta = new Vector2(_selector.sizeDelta.y, _selector.sizeDelta.y);
                _selector.gameObject.SetActive(true);
                _animation = _selector.DOSizeDelta(new Vector2(_selectorWidth, _selector.sizeDelta.y), 0.2f)
                    .OnComplete(() => _animation = null);
            }
            else
            {
                _animation = _selector.DOSizeDelta(new Vector2(_selector.sizeDelta.y, _selector.sizeDelta.y), 0.2f)
                    .OnComplete(() =>
                    {
                        _selector.gameObject.SetActive(false);
                        _animation = null;
                    });
            }
        }

        public void SetUnreadLabel(int count)
        {
            _notificator.SetActive(count > 0);
            if (count > 0)
            {
                _notificatorCountLabel.text = count.ToString();
            }
        }

        void CancelAnimation()
        {
            if (_animation != null)
            {
                _animation.Kill();
                _animation = null;
            }
        }
    }
}
