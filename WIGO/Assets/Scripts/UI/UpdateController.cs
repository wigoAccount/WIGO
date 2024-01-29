using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace WIGO.Userinterface
{
    public class UpdateController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] ScrollRect _mainScroll;
        [SerializeField] RectTransform _content;
        [SerializeField] UpdateArea _updateArea;
        [SerializeField] bool _reverse;
        
        bool _isUpdating;

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_updateArea.IsLoading())
            {
                return;
            }

            if ((_content.anchoredPosition.y <= -_content.sizeDelta.y + _mainScroll.viewport.rect.height && eventData.delta.y < 0f && _reverse)
                || (_content.anchoredPosition.y <= 0.01f && eventData.delta.y < 0f && !_reverse))
            {
                _isUpdating = true;
                _updateArea.OnBeginDrag(eventData);
                return;
            }

            _mainScroll.OnBeginDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_isUpdating)
            {
                _updateArea.OnDrag(eventData);
                return;
            }

            _mainScroll.OnDrag(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_isUpdating)
            {
                _isUpdating = false;
                _updateArea.OnEndDrag(eventData);
                return;
            }

            _mainScroll.OnEndDrag(eventData);
        }
    }
}
