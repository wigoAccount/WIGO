using UnityEngine;
using UnityEngine.UI;
using WIGO.Core;

namespace WIGO.Userinterface
{
    public class MessageContentUser : MessageContent
    {
        [SerializeField] Image _statusIcon;
        [SerializeField] Sprite[] _statusSprites;

        public override void Setup(ChatMessage data, out float height)
        {
            _message.rectTransform.sizeDelta = new Vector2(MAX_WIDTH, _message.rectTransform.sizeDelta.y);
            base.Setup(data, out height);
            float width = Mathf.Min(MAX_WIDTH, _message.preferredWidth + 0.4f);
            _message.rectTransform.sizeDelta = new Vector2(width, _message.rectTransform.sizeDelta.y);
            SetLastMessageStatus(data.GetStatus());
        }

        void SetLastMessageStatus(int status)
        {
            switch (status)
            {
                case 0:
                    _statusIcon.sprite = _statusSprites[0];
                    break;
                case 1:
                case 2:
                    _statusIcon.sprite = _statusSprites[1];
                    break;
                default:
                    _statusIcon.sprite = _statusSprites[0];
                    break;
            }
        }
    }
}
