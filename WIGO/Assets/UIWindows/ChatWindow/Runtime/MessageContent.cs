using TMPro;
using UnityEngine;
using WIGO.Core;

namespace WIGO.Userinterface
{
    public class MessageContent : MonoBehaviour
    {
        [SerializeField] RectTransform _bubble;
        [SerializeField] protected TMP_Text _message;
        [SerializeField] TMP_Text _timeLabel;

        protected const float MIN_WIDTH = 40f;
        protected const float MAX_WIDTH = 240f;
        const float MIN_HEIGHT = 20f;
        const float PADDING_HORIZONTAL = 20f;
        const float PADDING_VERTICAL = 16f;

        public virtual void Setup(ChatMessage data, out float height)
        {
            _message.text = data.GetText();
            _timeLabel.text = data.GetSendTime();

            float textWidth = Mathf.Clamp(_message.preferredWidth, MIN_WIDTH, MAX_WIDTH);
            float textHeight = Mathf.Max(_message.preferredHeight, MIN_HEIGHT);
            _message.rectTransform.sizeDelta = new Vector2(_message.rectTransform.sizeDelta.x, textHeight);
            _bubble.sizeDelta = new Vector2(textWidth + 2 * PADDING_HORIZONTAL, textHeight + 2 * PADDING_VERTICAL);
            height = _bubble.sizeDelta.y;
        }
    }
}
