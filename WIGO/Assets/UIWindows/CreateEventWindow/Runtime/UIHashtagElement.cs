using TMPro;
using UnityEngine;

namespace WIGO.Userinterface
{
    public class UIHashtagElement : UISelectableElement
    {
        [SerializeField] TMP_Text _label;

        string _tag;

        public void SetHashtag(string tag)
        {
            RectTransform hashtag = transform as RectTransform;

            _tag = tag;
            _label.text = tag;
            float textWidth = _label.preferredWidth + 2f;
            float width = textWidth + 32f;
            hashtag.sizeDelta = new Vector2(width, hashtag.sizeDelta.y);
        }

        public string GetTag() => _tag;
    }
}
