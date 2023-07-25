using UnityEngine;
using DG.Tweening;

namespace WIGO.Userinterface
{
    [RequireComponent(typeof(CanvasGroup))]
    public class SettingsLevelWindow : MonoBehaviour
    {
        [SerializeField] protected SettingsLevelWindow[] _children;

        CanvasGroup _windowGroup;
        RectTransform _window;
        protected SettingsLevelWindow _parent;

        public virtual void OnOpen(SettingsLevelWindow parent, bool quiet = false)
        {
            _parent = parent;
            if (quiet)
            {
                return;
            }

            gameObject.SetActive(true);
            _windowGroup.alpha = 0f;
            _window.localScale = Vector3.one * 0.9f;
            AnimateOpen();
        }

        public virtual void OnReopen()
        {
            gameObject.SetActive(true);
            _windowGroup.alpha = 0f;
            _window.localScale = Vector3.one * 1.1f;
            AnimateOpen();
        }

        public virtual void OnClose()
        {
            if (_children.Length > 0)
            {
                foreach (var child in _children)
                {
                    child.OnClose();
                }
            }

            gameObject.SetActive(false);
        }

        public void OnBackButtonClick()
        {
            OnClose();
            _parent?.OnReopen();
        }

        public virtual void OnChildClick(int index)
        {
            if (index < 0 || index >= _children.Length)
            {
                return;
            }

            OnClose();
            _children[index].OnOpen(this);
        }

        public virtual SettingsLevelWindow OpenChildQuiet(int index)
        {
            if (index < 0 || index >= _children.Length)
            {
                return null;
            }

            _children[index].OnOpen(this, true);
            return _children[index];
        }

        protected virtual void Awake()
        {
            _windowGroup = GetComponent<CanvasGroup>();
            _window = transform as RectTransform;
        }

        void AnimateOpen()
        {
            DOTween.Sequence().Append(_window.DOScale(1f, 0.1f))
                .Join(_windowGroup.DOFade(1f, 0.1f));
        }
    }
}
