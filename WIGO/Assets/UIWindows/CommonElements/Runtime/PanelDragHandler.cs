using System;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

namespace WIGO.Userinterface
{
    public class PanelDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] protected RectTransform _panel;

        protected Action _onCloseInfo;
        protected float _screenMultiplier;
        protected float _startDragPos;

        public void Init(Action onCloseInfo)
        {
            _screenMultiplier = ServiceLocator.Get<UIManager>().GetCanvasSize().y / Screen.height;
            _onCloseInfo = onCloseInfo;
        }

        public virtual void OnOpen()
        {
            _panel.anchoredPosition = Vector2.down * _panel.sizeDelta.y;
            _panel.gameObject.SetActive(true);
            _panel.DOAnchorPosY(0f, 0.36f);
        }

        public virtual void OnClose()
        {
            _panel.DOAnchorPosY(-_panel.sizeDelta.y, 0.36f)
                .OnComplete(() =>
                {
                    _onCloseInfo?.Invoke();
                    _panel.gameObject.SetActive(false);
                });
        }

        public virtual void ResetView()
        {
            _panel.anchoredPosition = Vector2.down * _panel.sizeDelta.y;
            _panel.gameObject.SetActive(false);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _startDragPos = eventData.position.y;
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            float mousePos = 0f;
#if UNITY_EDITOR
            mousePos = Input.mousePosition.y;
#elif UNITY_ANDROID || UNITY_IOS
            mousePos = Input.GetTouch(0).position.y;
#endif
            float delta = (mousePos - _startDragPos) * _screenMultiplier;
            float yPos = Mathf.Clamp(delta, -_panel.sizeDelta.y, 0f);

            _panel.anchoredPosition = Vector2.up * yPos;
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            float mousePos = 0f;
#if UNITY_EDITOR
            mousePos = Input.mousePosition.y;
#elif UNITY_ANDROID || UNITY_IOS
            mousePos = Input.GetTouch(0).position.y;
#endif
            float delta = (mousePos - _startDragPos) * _screenMultiplier;
            if (delta > -40f)
            {
                _panel.DOAnchorPosY(0f, 0.2f);
                return;
            }

            if (eventData.delta.y > 0f)
            {
                _panel.DOAnchorPosY(0f, 0.2f);
            }
            else
            {
                _panel.DOAnchorPosY(-_panel.sizeDelta.y, 0.2f)
                    .OnComplete(() =>
                    {
                        _onCloseInfo?.Invoke();
                        _panel.gameObject.SetActive(false);
                    });
            }
        }
    }
}
