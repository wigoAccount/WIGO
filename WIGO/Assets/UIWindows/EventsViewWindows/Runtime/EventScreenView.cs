using Crystal;
using TMPro;
using UnityEngine;
using WIGO.Core;

namespace WIGO.Userinterface
{
    public class EventScreenView : UIWindowView<EventViewModel>
    {
        [SerializeField] SafeArea _safeArea;
        [Header("Ask aprove part (for requests to my event)")]
        [SerializeField] RectTransform _eventInfo;
        [SerializeField] RectTransform _eventInfoViewport;
        [SerializeField] RectTransform _eventInfoContent;
        [SerializeField] TMP_Text _eventDescription;
        [SerializeField] RectTransform _descriptionBackground;
        [SerializeField] TMP_Text _infoTitle;
        [Space]
        [Header("Unaproved part (for my request to event)")]
        [SerializeField] RectTransform _unaprovedEventInfo;
        [SerializeField] TMP_Text _unaprovedDescription;
        [Space]
        [Header("Full view part (for events and requests)")]
        [SerializeField] RectTransform _acceptEventInfo;
        [SerializeField] RectTransform _acceptEventInfoViewport;
        [SerializeField] RectTransform _bottomGradient;
        [SerializeField] RectTransform _acceptEventInfoContent;
        [SerializeField] TextGradient _timerView;
        [SerializeField] TMP_Text _timerLabel;
        [SerializeField] TMP_Text _location;
        [SerializeField] UIGradient _locationBtnGradient;
        [SerializeField] TMP_Text _usernameLabel;
        [SerializeField] TMP_Text _phoneNumber;
        [SerializeField] TMP_Text _fullDescription;
        [SerializeField] RectTransform _acceptDescriptionBackground;
        [SerializeField] TMP_Text _acceptTitle;
        [SerializeField] TMP_Text _cancelLabel;
        [SerializeField] TMP_Text _eventCreatorTitle;
        [SerializeField] TMP_Text _eventCreatorDesc;
        [SerializeField] TMP_Text _eventDescTitle;
        [SerializeField] RectTransform _descBlock;
        [Space]
        [SerializeField] TMP_ColorGradient[] _gradients;
        [SerializeField] string[] _infoTitleText;
        [SerializeField] string[] _acceptTitleText;
        [SerializeField] string[] cancelBtnText;
        [SerializeField] string[] _creatorTitleText;
        [SerializeField] string[] _contactsDescText;
        [SerializeField] string[] _descTitleText;

        public override void Init(EventViewModel model)
        {
            base.Init(model);

            float bottomPadding = _safeArea.GetSafeAreaBottomPadding();
            _eventInfo.sizeDelta += Vector2.up * bottomPadding;
            _unaprovedEventInfo.sizeDelta += Vector2.up * bottomPadding;
            _acceptEventInfo.sizeDelta += Vector2.up * bottomPadding;
            _bottomGradient.sizeDelta += Vector2.up * bottomPadding;
            _eventInfoViewport.offsetMin += Vector2.up * bottomPadding;
            _acceptEventInfoViewport.offsetMin += Vector2.up * bottomPadding;
        }

        public void SetupView(Request request, bool isMyRequest)
        {
            AbstractEvent card = isMyRequest ? (AbstractEvent)request.@event : request;
            _eventInfoContent.anchoredPosition = Vector2.zero;
            _acceptEventInfoContent.anchoredPosition = Vector2.zero;
            _eventDescription.SetText(card.about);
            _fullDescription.SetText(card.about);
            _unaprovedDescription.SetText(card.about);
            _location.SetText(card.address);

            switch (request.GetStatus())
            {
                case Request.RequestStatus.decline:
                    Debug.LogWarningFormat("Request denied: {0}", request.uid);
                    break;
                case Request.RequestStatus.wait:
                    _eventInfo.gameObject.SetActive(!isMyRequest);
                    _unaprovedEventInfo.gameObject.SetActive(isMyRequest);
                    _acceptEventInfo.gameObject.SetActive(false);

                    float height = _eventDescription.preferredHeight + 16f;
                    _descriptionBackground.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
                    _infoTitle.SetText(card.IsResponse() ? _infoTitleText[1] : _infoTitleText[0]);
                    break;
                case Request.RequestStatus.accept:
                    _eventInfo.gameObject.SetActive(false);
                    _unaprovedEventInfo.gameObject.SetActive(false);
                    _acceptEventInfo.gameObject.SetActive(true);

                    float size = _fullDescription.preferredHeight + 16f;
                    _acceptDescriptionBackground.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size);
                    _descBlock.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size + 64f);
                    _acceptTitle.SetText(card.IsResponse() ? _acceptTitleText[1] : _acceptTitleText[0]);
                    _cancelLabel.SetText(card.IsResponse() ? cancelBtnText[1] : cancelBtnText[0]);
                    _timerLabel.colorGradientPreset = card.IsResponse() ? _gradients[1] : _gradients[0];
                    _eventCreatorTitle.SetText(card.IsResponse() ? _creatorTitleText[0] : _creatorTitleText[1]);
                    _eventCreatorDesc.SetText(card.IsResponse() ? _contactsDescText[0] : _contactsDescText[1]);
                    _eventDescTitle.SetText(card.IsResponse() ? _descTitleText[0] : _descTitleText[1]);
                    var preset = card.IsResponse() ? _gradients[1] : _gradients[0];
                    _locationBtnGradient.m_color1 = preset.bottomLeft;
                    _locationBtnGradient.m_color2 = preset.bottomRight;
                    _timerLabel.colorGradient = new VertexGradient(preset.bottomLeft, preset.bottomRight, preset.topLeft, preset.topRight);
                    SetTime(request.time_to);
                    break;
                default:
                    break;
            }

            var profile = card.author;
            _usernameLabel.SetText(profile.firstname);
            _phoneNumber.SetText(profile.phone);
        }

        public void SetTime(int time)
        {
            int correctTime = Mathf.Clamp(time, 0, int.MaxValue);
            int minutes = Mathf.FloorToInt((float)correctTime / 60f);
            int seconds = correctTime - minutes * 60;
            _timerLabel.text = string.Format("00:{0:00}:{1:00}", minutes, seconds);
            _timerView.ApplyGradient();
        }
    }
}
