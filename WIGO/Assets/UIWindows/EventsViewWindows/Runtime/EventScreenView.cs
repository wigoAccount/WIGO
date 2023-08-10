using Crystal;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using WIGO.Core;

using Event = WIGO.Core.Event;
namespace WIGO.Userinterface
{
    public class EventScreenView : UIWindowView<EventViewModel>
    {
        [SerializeField] SafeArea _safeArea;
        [SerializeField] RectTransform _eventInfo;
        [SerializeField] RectTransform _acceptEventInfo;
        [SerializeField] RectTransform _eventInfoViewport;
        [SerializeField] RectTransform _acceptEventInfoViewport;
        [SerializeField] RectTransform _bottomGradient;
        [SerializeField] RectTransform _eventInfoContent;
        [SerializeField] RectTransform _acceptEventInfoContent;
        [Space]
        [SerializeField] TextGradient _timerView;
        [SerializeField] TMP_Text _timerLabel;
        [SerializeField] TMP_Text _eventDescription;
        [SerializeField] TMP_Text _location;
        [SerializeField] UIGradient _locationBtnGradient;
        [SerializeField] TMP_Text _usernameLabel;
        [SerializeField] TMP_Text _phoneNumber;
        [SerializeField] TMP_Text _fullDescription;
        [SerializeField] RectTransform _descriptionBackground;
        [SerializeField] RectTransform _acceptDescriptionBackground;
        [SerializeField] TMP_Text _infoTitle;
        [SerializeField] TMP_Text _acceptTitle;
        [SerializeField] TMP_Text _cancelLabel;
        [SerializeField] TMP_Text _eventCreatorTitle;
        [SerializeField] TMP_Text _eventDescTitle;
        [SerializeField] RectTransform _descBlock;
        [Space]
        [SerializeField] UserProfile _tempProfile;
        [SerializeField] ProfileData _tmpProfile;
        [SerializeField] TMP_ColorGradient[] _gradients;
        [SerializeField] string[] _infoTitleText;
        [SerializeField] string[] _acceptTitleText;
        [SerializeField] string[] cancelBtnText;
        [SerializeField] string[] _creatorTitleText;
        [SerializeField] string[] _descTitleText;

        public override void Init(EventViewModel model)
        {
            base.Init(model);

            float bottomPadding = _safeArea.GetSafeAreaBottomPadding();
            _eventInfo.sizeDelta += Vector2.up * bottomPadding;
            _acceptEventInfo.sizeDelta += Vector2.up * bottomPadding;
            _bottomGradient.sizeDelta += Vector2.up * bottomPadding;
            _eventInfoViewport.offsetMin += Vector2.up * bottomPadding;
            _acceptEventInfoViewport.offsetMin += Vector2.up * bottomPadding;
        }

        public async void SetView(AbstractEvent card)
        {
            _eventInfoContent.anchoredPosition = Vector2.zero;
            _acceptEventInfoContent.anchoredPosition = Vector2.zero;
            _eventDescription.SetText(card.about);
            _fullDescription.SetText(card.about);
            _location.SetText(card.address);

            EventStatus status = GetViewStatus(card);
            
            switch (status)
            {
                case EventStatus.NotAccepted:
                case EventStatus.Watched:
                    _eventInfo.gameObject.SetActive(true);
                    _acceptEventInfo.gameObject.SetActive(false);

                    //_eventDescription.SetText(card.GetDescription());
                    float height = _eventDescription.preferredHeight + 16f;
                    _descriptionBackground.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
                    _infoTitle.SetText(card.IsResponse() ? _infoTitleText[1] : _infoTitleText[0]);
                    break;
                case EventStatus.Accepted:
                    _eventInfo.gameObject.SetActive(false);
                    _acceptEventInfo.gameObject.SetActive(true);

                    //_fullDescription.SetText(card.GetDescription());
                    float size = _fullDescription.preferredHeight + 16f;
                    _acceptDescriptionBackground.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
                    _descBlock.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size + 64f);
                    _acceptTitle.SetText(card.IsResponse() ? _acceptTitleText[1] : _acceptTitleText[0]);
                    _cancelLabel.SetText(card.IsResponse() ? cancelBtnText[1] : cancelBtnText[0]);
                    _timerLabel.colorGradientPreset = card.IsResponse() ? _gradients[1] : _gradients[0];
                    _eventCreatorTitle.SetText(card.IsResponse() ? _creatorTitleText[0] : _creatorTitleText[1]);
                    _eventDescTitle.SetText(card.IsResponse() ? _descTitleText[0] : _descTitleText[1]);
                    var preset = card.IsResponse() ? _gradients[1] : _gradients[0];
                    _locationBtnGradient.m_color1 = preset.bottomLeft;
                    _locationBtnGradient.m_color2 = preset.bottomRight;
                    _timerLabel.colorGradient = new VertexGradient(preset.bottomLeft, preset.bottomRight, preset.topLeft, preset.topRight);
                    SetTime(card.waiting);
                    break;
                case EventStatus.Denied:
                    Debug.LogWarningFormat("Event denied: {0}", card.uid);
                    return;
                default:
                    break;
            }

            var profile = await GetProfile();
            _usernameLabel.SetText(profile.firstname);
            _phoneNumber.SetText(profile.phone);
        }

        public void SetTime(int time)
        {
            int minutes = Mathf.FloorToInt((float)time / 60f);
            int seconds = time - minutes * 60;
            _timerLabel.text = string.Format("00:{0:00}:{1:00}", minutes, seconds);
            _timerView.ApplyGradient();
        }

        async Task<ProfileData> GetProfile()
        {
            await Task.Delay(200);
            return _tmpProfile;
        }

        EventStatus GetViewStatus(AbstractEvent card)
        {
            if (card.IsResponse())
            {
                Request request = (Request)card;
                return (request.GetStatus()) switch
                {
                    Request.RequestStatus.decline => EventStatus.Denied,
                    Request.RequestStatus.wait => EventStatus.Watched,
                    Request.RequestStatus.accept => EventStatus.Accepted,
                    _ => EventStatus.NotAccepted,
                };
            }
            else
            {
                Event cardEvent = (Event)card;
                return (cardEvent.GetStatus()) switch
                {
                    Event.EventStatus.active => EventStatus.Accepted,
                    Event.EventStatus.closed => EventStatus.Denied,
                    _ => EventStatus.NotAccepted,
                };
            }
        }
    }
}
