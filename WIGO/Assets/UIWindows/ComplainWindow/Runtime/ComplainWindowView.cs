using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace WIGO.Userinterface
{
    public class ComplainWindowView : UIWindowView<UIWindowModel>
    {
        [SerializeField] Image _overlay;
        [SerializeField] Image _fade;

        Sequence _animation;

        public void OnOpen()
        {
            UIGameColors.SetTransparent(_overlay);
            UIGameColors.SetTransparent(_fade);
            _animation = DOTween.Sequence().Append(_overlay.DOFade(0.8f, 0.4f))
                .Join(_fade.DOFade(0.1f, 0.4f))
                .OnComplete(() => _animation = null);
        }

        public void OnClose(Action callback = null)
        {
            CancelAnimation();

            _animation = DOTween.Sequence().Append(_overlay.DOFade(0f, 0.4f))
                .Join(_fade.DOFade(0f, 0.4f))
                .OnComplete(() =>
                {
                    callback?.Invoke();
                    _animation = null;
                });
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
