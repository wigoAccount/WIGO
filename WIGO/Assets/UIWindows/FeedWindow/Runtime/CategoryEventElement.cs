using TMPro;
using UnityEngine;

namespace WIGO.Userinterface
{
    public class CategoryEventElement : MonoBehaviour
    {
        [SerializeField] RectTransform _background;
        [SerializeField] TMP_Text _label;

        const float MIN_WIDTH = 32f;
        const float PADDING = 10f;

        public float GetWidth() => _background.sizeDelta.x;

        public void Setup(string category)
        {
            _label.text = category;
            float width = Mathf.Clamp(_label.preferredWidth + 2 * PADDING, MIN_WIDTH, float.MaxValue);
            _background.sizeDelta = new Vector2(width, _background.sizeDelta.y);
        }
    }
}
