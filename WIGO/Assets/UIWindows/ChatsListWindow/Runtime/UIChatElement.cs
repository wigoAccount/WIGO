using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using WIGO.RecyclableScroll;
using System;
using WIGO.Core;

using Event = WIGO.Core.Event;
namespace WIGO.Userinterface
{
    public class UIChatInfo : ContactInfo
    {
        public Action onInfoLoaded;

        AbstractEvent _cardData;
        Request _request;
        ProfileData _profile;
        Action<Request, bool> _onSelectChat;

        bool _loaded = true;
        bool _isEvent;

        public UIChatInfo(Request data, Action<Request, bool> onSelectChat, bool isEvent = false)
        {
            _request = data;
            _isEvent = isEvent;
            _cardData = isEvent ? (AbstractEvent)data.@event : data;
            _onSelectChat = onSelectChat;
            _profile = isEvent ? data.@event.author : data.author;
        }

        public AbstractEvent GetCard() => _cardData;
        public ProfileData GetProfile() => _profile;
        public bool IsLoaded() => _loaded;

        public void RaiseSelectCallback() => _onSelectChat?.Invoke(_request, _isEvent);

        public Request.RequestStatus GetRequestStatus() => _request.GetStatus();
        // [TODO]: Check 'THEIR REQUEST' watched or new
        public WatchRequestStatus GetWatchStatus() => WatchRequestStatus.Watched;
    }

    public class UIChatElement : MonoBehaviour, ICell<UIChatInfo>
    {
        [SerializeField] UserProfileElement _profilePhoto;
        [SerializeField] TMP_Text _nameLabel;
        [SerializeField] TMP_Text _messageLabel;
        [SerializeField] Image _statusIcon;
        [SerializeField] TMP_Text _statusLabel;
        [SerializeField] GameObject _frame;
        [SerializeField] Image _background;
        [Space]
        [SerializeField] GameObject _infoBlock;
        [SerializeField] GameObject _templateBlock;
        [Space]
        [SerializeField] string[] _statusTexts;

        int _index;
        UIChatInfo _info;

        public int GetIndex() => _index;
        public UIChatInfo GetInfo() => _info;

        public void InitCell(Action<ICell<UIChatInfo>> onClick) { }

        public Task ConfigureCell(UIChatInfo contactInfo, int cellIndex)
        {
            if (_info != null)
            {
                _info.onInfoLoaded -= SetupInfo;
            }

            _info = contactInfo;
            _index = cellIndex;

            if (_info.IsLoaded())
            {
                SetupInfo();
            }
            else
            {
                _infoBlock.SetActive(false);
                _templateBlock.SetActive(true);
                _info.onInfoLoaded += SetupInfo;
            }

            return Task.CompletedTask;
        }

        public void OnSelectClick()
        {
            _info.RaiseSelectCallback();
        }

        void SetupInfo()
        {
            var card = _info.GetCard();
            _nameLabel.text = _info.GetProfile() == null ? "username" : _info.GetProfile().firstname;
            _profilePhoto.Setup(card.preview);
            _messageLabel.SetText(card.about);
            _frame.SetActive(card.IsResponse());
            UIGameColors.SetTransparent(_background, card.IsResponse() ? 0.05f : 0.1f);

            if (card.IsResponse())
            {
                var watchStatus = _info.GetWatchStatus();
                _statusIcon.color = watchStatus == WatchRequestStatus.Watched ? UIGameColors.transparentBlue : UIGameColors.Blue;
                _statusLabel.text = watchStatus == WatchRequestStatus.Watched ? _statusTexts[0] : _statusTexts[1];
            }
            else
            {
                var status = _info.GetRequestStatus();
                _statusIcon.color = status == Request.RequestStatus.accept ? UIGameColors.Green : UIGameColors.transparent20;
                _statusLabel.text = status == Request.RequestStatus.accept ? _statusTexts[2] : _statusTexts[3];
            }

            _infoBlock.SetActive(true);
            _templateBlock.SetActive(false);
            _info.onInfoLoaded -= SetupInfo;
        }

        void OnDestroy()
        {
            if (_info != null)
            {
                _info.onInfoLoaded -= SetupInfo;
            }
        }
    }
}
