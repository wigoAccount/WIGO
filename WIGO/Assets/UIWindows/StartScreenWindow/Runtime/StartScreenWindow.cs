using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace WIGO.Userinterface
{
    public class StartScreenWindow : UIWindow
    {
        [SerializeField] UIHorizontalSwipeScroll _placeholderScroll;
        [SerializeField] Image[] _points;
        [SerializeField] Image _button;
        [SerializeField] TMP_Text _btnLabel;

        Action _onStartCallback;
        bool _animating;

        public void Setup(Action callback)
        {
            _onStartCallback = callback;
        }

        public void OnNextClick()
        {
            if (_animating)
                return;

            if (_placeholderScroll.GetCurrentIndex() >= _points.Length - 1)
            {
                //ServiceLocator.Get<UIManager>().Open<RegistrationWindow>(WindowId.REGISTRATION_SCREEN);
                _onStartCallback?.Invoke();
                return;
            }

            int prevIndex = _placeholderScroll.GetCurrentIndex();
            int nextIndex = prevIndex + 1;
            OnSwitchToStage(nextIndex);
            _placeholderScroll.RecycleTo(nextIndex);
        }

        public void OnSkipClick()
        {
            ServiceLocator.Get<UIManager>().Open<RegistrationWindow>(WindowId.REGISTRATION_SCREEN);
            //_onStartCallback?.Invoke();
        }

        protected override void Awake()
        {
            _placeholderScroll.Initialize(OnSwitchToStage);
        }

        void OnSwitchToStage(int index)
        {
            int prevIndex = _placeholderScroll.GetCurrentIndex();
            int nextIndex = index;

            _button.color = index < _points.Length - 1 ? UIGameColors.transparent10 : UIGameColors.Blue;
            _btnLabel.SetText(index < _points.Length - 1 ? "Далее" : "Давай начнем!");

            _animating = true;
            DOTween.Sequence().Append(_points[prevIndex].DOFade(0.1f, 0.24f))
                .Join(_points[nextIndex].DOFade(1f, 0.24f))
                .OnComplete(() =>
                {
                    _animating = false;
                });
        }
    }
}
