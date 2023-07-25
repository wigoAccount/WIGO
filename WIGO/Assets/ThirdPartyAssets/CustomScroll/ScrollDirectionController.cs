using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ScrollDirectionController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] ScrollRect _currentScroll;         //parent scroll
    [SerializeField] ScrollRect _mainScroll;            //child scroll controlled by this script

    bool _draggingParent;                               //indicates is parent scroll controlled by user

    public void SetupMainScroll(ScrollRect scroll) => _mainScroll = scroll;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (IsPotentialParentDrag(eventData.delta))
        {
            _mainScroll.OnBeginDrag(eventData);
            _draggingParent = true;
        }
        else
        {
            _currentScroll.OnBeginDrag(eventData);
            _draggingParent = false;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_draggingParent)
        {
            _mainScroll.OnDrag(eventData);
        }
        else
        {
            _currentScroll.OnDrag(eventData);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_draggingParent)
        {
            _mainScroll.OnEndDrag(eventData);
        }
        else
        {
            _currentScroll.OnEndDrag(eventData);
        }

        _draggingParent = false;
    }

    /// <summary>
    /// Define if user scrolls current Scroll or parent using drag direction
    /// </summary>
    /// <param name="inputDelta"></param>
    /// <returns></returns>
    bool IsPotentialParentDrag(Vector2 inputDelta)
    {
        if (_mainScroll != null)
        {
            if (_mainScroll.horizontal && !_mainScroll.vertical)
            {
                return Mathf.Abs(inputDelta.x) > Mathf.Abs(inputDelta.y);
            }
            if (!_mainScroll.horizontal && _mainScroll.vertical)
            {
                return Mathf.Abs(inputDelta.x) < Mathf.Abs(inputDelta.y);
            }
            else return true;
        }

        return false;
    }
}
