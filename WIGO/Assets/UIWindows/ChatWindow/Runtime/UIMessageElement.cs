using System;
using System.Threading.Tasks;
using UnityEngine;
using WIGO.Core;
using WIGO.RecyclableScroll;

namespace WIGO.Userinterface
{
    public class UIMessageInfo : ContactInfo
    {
        ChatMessage _messageData;

        public UIMessageInfo(ChatMessage data)
        {
            _messageData = data;
        }

        public ChatMessage GetMessageData() => _messageData;
    }

    public class UIMessageElement : MonoBehaviour, ICell<UIMessageInfo>
    {
        [SerializeField] MessageContentUser _contentUser;
        [SerializeField] MessageContentFriend _contentFriend;

        UIMessageInfo _info;
        RectTransform _messageRect;
        int _index;

        public void InitCell(Action<ICell<UIMessageInfo>> onClick)
        {
            _messageRect = transform as RectTransform;
        }

        public Task ConfigureCell(UIMessageInfo contactInfo, int cellIndex)
        {
            _info = contactInfo;
            _index = cellIndex;

            if (contactInfo.GetMessageData().IsMine())
            {
                _contentUser.gameObject.SetActive(true);
                _contentFriend.gameObject.SetActive(false);
                _contentUser.Setup(contactInfo.GetMessageData(), out float bubbleHeight);
                _messageRect.sizeDelta = new Vector2(_messageRect.sizeDelta.x, bubbleHeight);
            }
            else
            {
                _contentUser.gameObject.SetActive(false);
                _contentFriend.gameObject.SetActive(true);
                _contentFriend.Setup(contactInfo.GetMessageData(), out float bubbleHeight);
                _messageRect.sizeDelta = new Vector2(_messageRect.sizeDelta.x, bubbleHeight);
            }

            return Task.CompletedTask;
        }

        public int GetIndex() => _index;
        public UIMessageInfo GetInfo() => _info;
    }
}
