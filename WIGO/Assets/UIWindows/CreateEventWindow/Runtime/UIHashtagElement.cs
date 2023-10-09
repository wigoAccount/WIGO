using TMPro;
using UnityEngine;
using WIGO.Core;

namespace WIGO.Userinterface
{
    public class UIHashtagElement : UISelectableElement
    {
        [SerializeField] TMP_Text _label;

        GeneralData _tag;

        public void SetHashtag(GeneralData data)
        {
            RectTransform hashtag = transform as RectTransform;

            _tag = data;
            _label.text = data.name;
            float textWidth = _label.preferredWidth + 2f;
            float width = textWidth + 32f;
            hashtag.sizeDelta = new Vector2(width, hashtag.sizeDelta.y);
        }

        public GeneralData GetTag() => _tag;
    }
}
