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

        int _seconds;
        bool _loaded = true;
        bool _isEvent;

        public UIChatInfo(Request data, Action<Request, bool> onSelectChat, bool isEvent = false)
        {
            _request = data;
            _isEvent = isEvent;
            _cardData = isEvent ? (AbstractEvent)data.@event : data;
            _onSelectChat = onSelectChat;
            _profile = isEvent ? data.@event.author : data.author;
            _seconds = data.TimeTo;
        }

        public AbstractEvent GetCard() => _cardData;
        public ProfileData GetProfile() => _profile;
        public bool IsLoaded() => _loaded;

        public void RaiseSelectCallback() => _onSelectChat?.Invoke(_request, _isEvent);
        public void DecreaseSeconds() => _seconds = Mathf.Clamp(_seconds - 1, 0, int.MaxValue);

        public Request.RequestStatus GetRequestStatus() => _request.GetStatus();
        public bool IsWatched() => _request.IsWatched();
        public int GetSeconds() => _seconds;
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
        [SerializeField] TMP_Text _timerLabel;
        [Space]
        [SerializeField] GameObject _infoBlock;
        [SerializeField] GameObject _templateBlock;
        [Space]
        [SerializeField] string[] _statusTexts;

        int _index;
        float _timer;
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

            var accepted = _info.GetRequestStatus() == Request.RequestStatus.accept;
            if (card.IsResponse())
            {
                var watchStatus = _info.IsWatched();
                _statusIcon.color = watchStatus ? UIGameColors.transparentBlue : UIGameColors.Blue;
                _statusLabel.text = watchStatus ? _statusTexts[0] : _statusTexts[1];

                _statusIcon.gameObject.SetActive(!accepted);
                _timerLabel.transform.parent.gameObject.SetActive(accepted);
                _timer = 0f;
                if (accepted)
                    SetTime(_info.GetSeconds());
            }
            else
            {
                _statusIcon.color = accepted ? UIGameColors.Green : UIGameColors.transparent20;
                _statusLabel.text = accepted ? _statusTexts[2] : _statusTexts[3];
            }

            _infoBlock.SetActive(true);
            _templateBlock.SetActive(false);
            _info.onInfoLoaded -= SetupInfo;
        }

        void SetTime(int time)
        {
            int correctTime = Mathf.Clamp(time, 0, int.MaxValue);
            int hours = Mathf.FloorToInt((float)correctTime / 3600f);
            int minutes = Mathf.FloorToInt((float)correctTime / 60f) - hours * 60;
            int seconds = time - minutes * 60 - hours * 3600;
            _timerLabel.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        void Update()
        {
            if (!_info.GetCard().IsResponse() || _info.GetRequestStatus() != Request.RequestStatus.accept || _info.GetSeconds() <= 0)
            {
                return;
            }

            _timer += Time.deltaTime;
            if (_timer >= 1f)
            {
                _timer -= 1f;
                _info.DecreaseSeconds();
                SetTime(_info.GetSeconds());
            }
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
