using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using System;

namespace WIGO.Userinterface
{
    public class UpdateArea : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] RectTransform _updater;
        [SerializeField] Image _updateIcon;

        float _deltaPos;
        float _screenMultiplier;
        float _startUpdaterUpperPos;
        bool _loadProcess;
        Action _onStartUpdate;
        Sequence _animation;

        const float MAX_BOTTOM_POINT = -96f;
        const float LOAD_SPEED = 360f;

        public bool IsLoading() => _loadProcess;

        internal void Init(Action onStartUpdate)
        {
            _onStartUpdate = onStartUpdate;
            _startUpdaterUpperPos = _updater.sizeDelta.y / 2f;
            _screenMultiplier = ServiceLocator.Get<UIManager>().GetCanvasSize().y / Screen.height;
            _updater.anchoredPosition = Vector2.up * _startUpdaterUpperPos;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_loadProcess)
            {
                return;
            }

            CancelAnimation();
            float inputPosition = 0f;
#if UNITY_EDITOR
            inputPosition = (Input.mousePosition.y - Screen.height) * _screenMultiplier;
#elif UNITY_ANDROID || UNITY_IOS
            inputPosition = (Input.GetTouch(0).position.y - Screen.height) * _screenMultiplier;
#else
            inputPosition = (Input.mousePosition.y - Screen.height) * _screenMultiplier;
#endif
            _deltaPos = inputPosition - _updater.anchoredPosition.y;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_loadProcess)
            {
                return;
            }

            float inputPosition = 0f;
#if UNITY_EDITOR
            inputPosition = (Input.mousePosition.y - Screen.height) * _screenMultiplier;
#elif UNITY_ANDROID || UNITY_IOS
            inputPosition = (Input.GetTouch(0).position.y - Screen.height) * _screenMultiplier;
#else
            inputPosition = (Input.mousePosition.y - Screen.height) * _screenMultiplier;
#endif

            float lerpPosition = inputPosition - _deltaPos;
            lerpPosition = Mathf.Clamp(lerpPosition, MAX_BOTTOM_POINT, _startUpdaterUpperPos);
            _updater.anchoredPosition = Vector2.up * lerpPosition;
            float normalizePos = Mathf.Abs((lerpPosition - _startUpdaterUpperPos) / (MAX_BOTTOM_POINT - _startUpdaterUpperPos));
            AnimateUpdater(normalizePos);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_loadProcess)
            {
                return;
            }

            bool needReturn = _updater.anchoredPosition.y >= MAX_BOTTOM_POINT / 3f;
            float startPos = _startUpdaterUpperPos;
            float rotatePos = MAX_BOTTOM_POINT / 3f;

            if (needReturn)
            {
                MoveUpdater(startPos, () =>
                {
                    _updateIcon.fillAmount = 0f;
                    UIGameColors.SetTransparent(_updateIcon);
                    _updateIcon.rectTransform.localRotation = Quaternion.Euler(Vector3.zero);
                });
            }
            else
            {
                MoveUpdater(rotatePos, () =>
                {
                    _loadProcess = true;
                    _onStartUpdate?.Invoke();
                });
            }
        }

        public void DeactivateLoading()
        {
            if (gameObject.activeSelf && _loadProcess)
            {
                _loadProcess = false;
                StopLoad();
            }
        }

        private void Update()
        {
            if (_loadProcess && _updateIcon != null)
            {
                _updateIcon.rectTransform.Rotate(Vector3.back * LOAD_SPEED * Time.deltaTime);
            }
        }

        private void OnDisable()
        {
            if (_animation != null)
            {
                _animation.Kill();
                ResetUpdater();
            }
        }

        void AnimateUpdater(float normalizedPos)
        {
            float progress = Mathf.Clamp01(normalizedPos * 3f);
            _updateIcon.fillAmount = progress;
            UIGameColors.SetTransparent(_updateIcon, progress);
            if (normalizedPos >= 0.25f)
            {
                _updateIcon.rectTransform.localRotation = Quaternion.Euler(Vector3.back * ((normalizedPos - 0.25f) / 0.75f) * 360f);
            }
        }

        void MoveUpdater(float pos, TweenCallback callback = null)
        {
            CancelAnimation();
            _animation = DOTween.Sequence().Append(_updater.DOAnchorPosY(pos, 0.1f)).OnComplete(() =>
            {
                callback?.Invoke();
                _animation = null;
            });
        }

        void StopLoad()
        {
            CancelAnimation();
            Image updaterImg = _updater.GetComponent<Image>();

            _animation = DOTween.Sequence();
            _animation.Append(updaterImg.DOFade(0f, 0.16f))
                .Join(_updateIcon.DOFade(0f, 0.16f))
                .OnComplete(() =>
                {
                    ResetUpdater();
                });
        }

        void ResetUpdater()
        {
            _updater.anchoredPosition = Vector2.up * _startUpdaterUpperPos;
            Image updaterImg = _updater.GetComponent<Image>();
            UIGameColors.SetTransparent(updaterImg, 1f);
            UIGameColors.SetTransparent(_updateIcon, 1f);
            _updater.localScale = Vector3.one;
            _updateIcon.rectTransform.localRotation = Quaternion.Euler(Vector3.zero);
            _animation = null;
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
