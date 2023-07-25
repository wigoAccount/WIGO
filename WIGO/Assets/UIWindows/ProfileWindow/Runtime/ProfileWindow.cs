using UnityEngine;
using UnityEngine.UI;
using WIGO.Core;
using System;
using System.Collections;
using System.Threading.Tasks;

namespace WIGO.Userinterface
{
    public class ProfileWindow : UIWindow
    {
        [SerializeField] GameObject _editButton;
        [SerializeField] GameObject _doneButton;
        [SerializeField] ProfileInfoArea _infoArea;
        [SerializeField] ProfileEditArea _editArea;
        [SerializeField] RectTransform _content;
        [Space]
        [SerializeField] UserProfileElement _avatar;
        [Space]
        [SerializeField] Image _profileTemplate;
        [SerializeField] Sprite[] _templates;

        ProfileData _currentProfile;

        public override void OnClose(WindowId next, Action callback = null)
        {
            ClearWindow();
            base.OnClose(next, callback);
        }

        public void Setup()
        {
            var myProfile = ServiceLocator.Get<GameModel>().GetMyProfile();
            Setup(myProfile);
        }

        public void Setup(ProfileData profile)
        {
            _currentProfile = profile;
            _profileTemplate.sprite = profile.GetGender() == Gender.male ? _templates[0] : _templates[1];
            _avatar.Setup(profile);
            _infoArea.Setup(profile, () => StartCoroutine(UpdateHeight(true)));

            bool myProfile = ServiceLocator.Get<GameModel>().IsMyProfile(profile.uid);
            _editButton.SetActive(myProfile);
        }

        public void OnBackButtonClick()
        {
            if (_editArea.gameObject.activeSelf)
            {
                _editArea.gameObject.SetActive(false);
                _infoArea.gameObject.SetActive(true);
                _content.anchoredPosition = Vector2.zero;
                _editButton.SetActive(true);
                _doneButton.SetActive(false);
                StartCoroutine(UpdateHeight(true));
                return;
            }

            ServiceLocator.Get<UIManager>().CloseCurrent();
        }

        public void OnMainFeedClick()
        {
            ServiceLocator.Get<UIManager>().SwitchTo(WindowId.FEED_SCREEN);
        }

        public void OnSettingsClick()
        {
            ServiceLocator.Get<UIManager>().Open<SettingsWindow>(WindowId.SETTINGS_SCREEN);
        }

        public void OnEventsClick()
        {
            ServiceLocator.Get<UIManager>().Open<ChatsListWindow>(WindowId.CHATS_LIST_SCREEN);
        }

        public void OnEditClick()
        {
            _infoArea.gameObject.SetActive(false);
            _editArea.gameObject.SetActive(true);
            _content.anchoredPosition = Vector2.zero;
            _editButton.SetActive(false);
            _doneButton.SetActive(true);
            _editArea.Setup(_currentProfile);
            StartCoroutine(UpdateHeight(false));
        }

        public async void OnDoneClick()
        {
            _editArea.UpdateInfo();
            _editArea.gameObject.SetActive(false);
            _infoArea.gameObject.SetActive(true);
            _content.anchoredPosition = Vector2.zero;
            _editButton.SetActive(true);
            _doneButton.SetActive(false);
            _infoArea.UpdateInfo(() => StartCoroutine(UpdateHeight(true)));

            await Task.Delay(400);
        }

        protected override void Awake()
        {
            _editArea.Init();
        }

        void ClearWindow()
        {
            _infoArea.gameObject.SetActive(true);
            _editArea.gameObject.SetActive(false);
            _doneButton.SetActive(false);
            _content.anchoredPosition = Vector2.zero;
            _currentProfile = null;
        }

        IEnumerator UpdateHeight(bool info)
        {
            yield return null;
            float height = info ? _infoArea.GetHeight() : _editArea.GetHeight();
            _content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }
    }
}
