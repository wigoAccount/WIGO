using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace WIGO.Userinterface
{
    public class ProfilePhotoPointElement : MonoBehaviour
    {
        [SerializeField] Image _pointPrefab;

        List<Image> _points = new List<Image>();
        Image _selectedPoint;

        public void Setup(int count, int selected)
        {
            if (count > 1)
            {
                for (int i = 0; i < count; i++)
                {
                    var point = Instantiate(_pointPrefab, transform);
                    if (i == selected)
                    {
                        point.rectTransform.sizeDelta = new Vector2(30f, point.rectTransform.sizeDelta.y);
                        _selectedPoint = point;
                    }
                    else
                    {
                        UIGameColors.SetTransparent(point, 0.5f);
                    }
                    _points.Add(point);
                }
            }
        }

        public void SetSelected(int index)
        {
            int current = _points.IndexOf(_selectedPoint);
            if (current == index)
            {
                return;
            }

            var lastSelected = _selectedPoint;
            _selectedPoint = _points[index];

            var animation = DOTween.Sequence();
            animation.Append(lastSelected.rectTransform.DOSizeDelta(new Vector2(4f, lastSelected.rectTransform.sizeDelta.y), 0.16f))
                .Join(lastSelected.DOFade(0.5f, 0.16f))
                .Join(_selectedPoint.rectTransform.DOSizeDelta(new Vector2(30f, _selectedPoint.rectTransform.sizeDelta.y), 0.16f))
                .Join(_selectedPoint.DOFade(1f, 0.16f));
        }

        public void Clear()
        {
            foreach (var point in _points)
            {
                Destroy(point.gameObject);
            }

            _points.Clear();
            _selectedPoint = null;
        }
    }
}
