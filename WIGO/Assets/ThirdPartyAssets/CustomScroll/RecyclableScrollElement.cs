using UnityEngine;

public class RecyclableScrollElement : MonoBehaviour
{
    protected RectTransform _elementRect;     //RectTransform of element
    float _addingBottom;
    float _addingTop;

    /// <summary>
    /// Initialization
    /// </summary>
    internal virtual void Init()
    {
        _elementRect = transform as RectTransform;
    }

    public float GetElementSize() => _elementRect.sizeDelta.y;
    public float GetBottomPoint() => _elementRect.anchoredPosition.y - _elementRect.sizeDelta.y - _addingBottom;
    public float GetUpperPoint() => _elementRect.anchoredPosition.y + _addingTop;
    public float MaxY() => GetCorners()[1].y;
    public float MinY() => GetCorners()[0].y;

    /// <summary>
    /// Setting Y position
    /// </summary>
    /// <param name="yPos"></param>
    public void SetPosition(float yPos)
    {
        _elementRect.anchoredPosition = Vector2.up * yPos;
    }

    /// <summary>
    /// Adding position to existing
    /// </summary>
    /// <param name="position"></param>
    public void AddPosition(Vector2 position)
    {
        _elementRect.anchoredPosition += position;
    }

    public void SetAddingBottom(float adding) => _addingBottom = adding;
    public void SetAddingTop(float adding) => _addingTop = adding;

    /// <summary>
    /// Get 4 corners in world space of element rectangle
    /// </summary>
    /// <returns></returns>
    Vector3[] GetCorners()
    {
        var corners = new Vector3[4];
        _elementRect.GetWorldCorners(corners);
        return corners;
    }
}
