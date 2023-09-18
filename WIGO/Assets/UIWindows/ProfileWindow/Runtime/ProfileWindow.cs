using UnityEngine;
using UnityEngine.UI;
using WIGO.Core;
using System;
using System.Collections;
using System.Threading;

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
        CancellationTokenSource _cts;

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
            _infoArea.Setup(profile);
            StartCoroutine(UpdateHeight(true));

            bool myProfile = ServiceLocator.Get<GameModel>().IsMyProfile(profile.uid);
            _editButton.SetActive(myProfile);
        }

        public void OnBackButtonClick()
        {
            if (_editArea.gameObject.activeSelf)
            {
                _editArea.Close();
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
            var updatedProfile = await _editArea.UpdateInfo();
            _editArea.Close();
            _infoArea.gameObject.SetActive(true);
            _content.anchoredPosition = Vector2.zero;
            _editButton.SetActive(true);
            _doneButton.SetActive(false);
            _infoArea.Setup(updatedProfile);
            StartCoroutine(UpdateHeight(true));

            var oldTagList = _currentProfile.tags;
            var newTagList = updatedProfile.tags;
            string addTags = string.Empty;
            string removeTags = string.Empty;
            foreach (var tag in oldTagList)
            {
                bool exists = Array.Exists(newTagList, x => x.uid == tag.uid);
                if (!exists)
                    removeTags += $"{tag.uid},";
            }

            if (!string.IsNullOrEmpty(removeTags))
                removeTags = removeTags[0..^1];

            foreach (var tag in newTagList)
            {
                bool exists = Array.Exists(oldTagList, x => x.uid == tag.uid);
                if (!exists)
                    addTags += $"{tag.uid},";
            }

            if (!string.IsNullOrEmpty(addTags))
                addTags = addTags[0..^1];

            var genderUID = 2 - (int)updatedProfile.GetGender();
            string userUpdJson = "{\"phone\":" + $"\"{updatedProfile.phone}\"," +
                    "\"firstname\":" + $"\"{updatedProfile.firstname}\"," +
                    "\"nickname\":" + $"\"{updatedProfile.nickname}\"," +
                    "\"about\":" + $"\"{updatedProfile.about.Replace("\n", "\\n")}\"," +
                    "\"gender\":" + $"\"{genderUID}\"," +
                    "\"avatar\":" + $"\"{updatedProfile.avatar}\"," +
                    "\"tags_add\":" + $"[{addTags}]," +// [1,2], // ?????? ????? (?????????)
                    "\"tags_remove\":" + $"[{removeTags}]}}";// [3]

            var model = ServiceLocator.Get<GameModel>();
            _cts = new CancellationTokenSource();
            Debug.Log(model.ShortToken);
            var profile = await NetService.TryUpdateUser(userUpdJson, model.ShortToken, _cts.Token);
            if (_cts.IsCancellationRequested)
            {
                Debug.Log("User update cancelled");
                _cts.Dispose();
                _cts = null;
                return;
            }

            _cts.Dispose();
            _cts = null;
            if (profile == null)
            {
                Debug.LogError("Wrong update profile");
                return;
            }

            _currentProfile = profile;
            model.SaveProfile(profile);
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
