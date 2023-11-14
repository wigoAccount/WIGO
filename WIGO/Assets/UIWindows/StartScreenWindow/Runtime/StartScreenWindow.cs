using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;

namespace WIGO.Userinterface
{
    public class StartScreenWindow : UIWindow
    {
        [SerializeField] UIHorizontalSwipeScroll _placeholderScroll;
        [SerializeField] Image[] _points;
        //[SerializeField] Image _button;
        [SerializeField] TMP_Text _btnLabel;
        [Space]
        [SerializeField] string _nextText;
        [SerializeField] string _letsStartText;

        bool _animating;
        bool _startOnboarding;

        public void Setup(bool inStart)
        {
            _startOnboarding = inStart;
        }

        public override void OnClose(WindowId next, Action callback = null)
        {
            _placeholderScroll.Clear();
            //_button.color = UIGameColors.transparent10;
            _btnLabel.SetText(_nextText);
            for (int i = 0; i < _points.Length; i++)
            {
                float alpha = i == 0 ? 1f : 0.1f;
                UIGameColors.SetTransparent(_points[i], alpha);
            }
            _animating = false;
            callback?.Invoke();
        }

        public void OnNextClick()
        {
            if (_animating)
                return;

            if (_placeholderScroll.GetCurrentIndex() >= _points.Length - 1)
            {
                if (_startOnboarding)
                {
                    ServiceLocator.Get<UIManager>().Open<RegistrationWindow>(WindowId.REGISTRATION_SCREEN);
                }
                else
                {
                    ServiceLocator.Get<UIManager>().CloseCurrent();
                }
                return;
            }

            int prevIndex = _placeholderScroll.GetCurrentIndex();
            int nextIndex = prevIndex + 1;
            OnSwitchToStage(nextIndex);
            _placeholderScroll.RecycleTo(nextIndex);
        }

        public void OnSkipClick()
        {
            if (_startOnboarding)
            {
                ServiceLocator.Get<UIManager>().Open<RegistrationWindow>(WindowId.REGISTRATION_SCREEN);
            }
            else
            {
                ServiceLocator.Get<UIManager>().CloseCurrent();
            }
        }
            

        protected override void Awake()
        {
            _placeholderScroll.Initialize(OnSwitchToStage);
        }

        void OnSwitchToStage(int index)
        {
            int prevIndex = _placeholderScroll.GetCurrentIndex();
            int nextIndex = index;

            //_button.color = index < _points.Length - 1 ? UIGameColors.transparent10 : UIGameColors.Blue;
            _btnLabel.SetText(index < _points.Length - 1 ? _nextText : _letsStartText);

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
