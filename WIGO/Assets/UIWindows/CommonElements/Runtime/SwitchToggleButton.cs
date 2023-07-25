using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace WIGO.Userinterface
{
    public class SwitchToggleButton : MonoBehaviour
    {
        [SerializeField] Image _background;
        [SerializeField] RectTransform _switcher;

        Action<bool> _onSwitch;
        Sequence _switchTween;
        bool _isOn;

        const float EDGE_POS = 6f;
        const float SWITCH_TIME = 0.2f;

        public bool IsOn() => _isOn;

        public void Subscribe(Action<bool> onSwitch)
        {
            _onSwitch = onSwitch;
        }

        public void OnClickSwitcher()
        {
            CancelTween();

            float pos = _isOn ? -EDGE_POS : EDGE_POS;
            Color color = _isOn ? UIGameColors.Gray : UIGameColors.Blue;

            _isOn = !_isOn;
            _onSwitch?.Invoke(_isOn);

            _switchTween = DOTween.Sequence();
            _switchTween.Append(_background.DOColor(color, SWITCH_TIME))
                .Join(_switcher.DOAnchorPosX(pos, SWITCH_TIME))
                .OnComplete(() => _switchTween = null);
        }

        public void SetOn(bool isOn)
        {
            _isOn = isOn;
            _switcher.anchoredPosition = Vector2.right * (isOn ? EDGE_POS : -EDGE_POS);
            _background.color = isOn ? UIGameColors.Blue : UIGameColors.Gray;
        }

        void CancelTween()
        {
            if (_switchTween != null)
            {
                _switchTween.Kill();
                _switchTween = null;
            }
        }
    }
}
