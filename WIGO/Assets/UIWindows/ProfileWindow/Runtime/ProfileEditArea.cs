using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Threading.Tasks;

namespace WIGO.Userinterface
{
    public class ProfileEditArea : MonoBehaviour
    {
        [SerializeField] TMP_InputField _displayNameIF;
        [SerializeField] TMP_InputField _usernameIF;
        [SerializeField] TMP_InputField _aboutIF;
        [Space]
        [SerializeField] TMP_Text _genderLabel;
        [Space]
        [SerializeField] ProfileGalleryItem _avatar;

        ProfileData _currentProfile;
        RectTransform _area;

        public float GetHeight() => _area.rect.height;
        public ProfileData GetUpdatedProfile() => _currentProfile;

        public void Init()
        {
            _area = transform as RectTransform;
        }

        public void Setup(ProfileData profile)
        {
            _currentProfile = ProfileData.CopyProfile(profile);
            _displayNameIF.SetTextWithoutNotify(profile.firstname);
            _usernameIF.SetTextWithoutNotify(profile.nickname);
            _aboutIF.SetTextWithoutNotify(profile.about);
            _genderLabel.SetText(profile.gender.uid == 2 ? "Male" : "Female");   // [TODO]: replace with configs
            _avatar.gameObject.SetActive(true);
        }

        public void Close()
        {
            _avatar.OnClear();
            _avatar.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }

        public async Task<ProfileData> UpdateInfo()
        {
            var path = await _avatar.UploadPhoto();

            _currentProfile.firstname = _displayNameIF.text;
            _currentProfile.nickname = _usernameIF.text;
            _currentProfile.about = _aboutIF.text;
            _currentProfile.avatar = path;

            return _currentProfile;

        }

        public void OnGenderChange()
        {
            var gender = _currentProfile.GetGender();
            ContainerData newGender = new ContainerData()
            {
                uid = gender == Gender.male ? 1 : 2,
                name = gender == Gender.male ? "female" : "male"
            };
            _genderLabel.SetText(newGender.uid == 2 ? "Male" : "Female");   // [TODO]: replace with configs
            _currentProfile.gender = newGender;
        }
    }
}
