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
        //[SerializeField] TMP_InputField _phoneIF;
        //[SerializeField] TMP_InputField _emailIF;
        [Space]
        [SerializeField] TMP_Text _genderLabel;
        [Space]
        //[SerializeField] ProfileSelectableTag[] _tags;
        [SerializeField] ProfileGalleryItem _avatar;
        //[SerializeField] ProfileGalleryItem[] _photos;

        ProfileData _currentProfile;
        //List<ContainerData> _tagsList = new List<ContainerData>();
        RectTransform _area;

        public float GetHeight() => _area.rect.height;
        public ProfileData GetUpdatedProfile() => _currentProfile;

        public void Init()
        {
            _area = transform as RectTransform;
            //foreach (var tag in _tags)
            //{
            //    tag.Setup(OnTagSelect);
            //}
        }

        public void Setup(ProfileData profile)
        {
            _currentProfile = ProfileData.CopyProfile(profile);
            _displayNameIF.SetTextWithoutNotify(profile.firstname);
            _usernameIF.SetTextWithoutNotify(profile.nickname);
            _aboutIF.SetTextWithoutNotify(profile.about);
            //_phoneIF.SetTextWithoutNotify(profile.phone);
            //_emailIF.SetTextWithoutNotify(profile.email);
            _genderLabel.SetText(profile.gender.uid == 2 ? "???????" : "???????");   // [TODO]: replace with configs
            _avatar.gameObject.SetActive(true);

            //_tagsList.Clear();
            //foreach (var tag in _tags)
            //{
            //    tag.SetSelected(false, false);
            //}

            //for (int i = 0; i < profile.tags.Length; i++)
            //{
            //    var tag = Array.Find(_tags, x => x.GetUID() == profile.tags[i].uid);
            //    tag?.SetSelected(true, false);
            //    _tagsList.Add(profile.tags[i]);
            //}

            //foreach (var photo in _photos)
            //{
            //    photo.SetEmpty();
            //}

            //if (profile.photos != null)
            //{
            //    for (int i = 0; i < profile.photos.Length; i++)
            //    {
            //        if (UnityEngine.Random.Range(0f, 1f) > 0.5f)// !string.IsNullOrEmpty(profile.photos[i]))
            //        {
            //            _photos[i].SetPhoto(profile.photos[i]);
            //        }
            //    }
            //}

            //for (int i = 0; i < 4; i++)
            //{
            //    if (UnityEngine.Random.Range(0f, 1f) > 0.5f)// !string.IsNullOrEmpty(profile.photos[i]))
            //    {
            //        _photos[i].SetPhoto("");
            //    }
            //}
        }

        public void Close()
        {
            _avatar.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }

        public async Task<ProfileData> UpdateInfo()
        {
            var path = await _avatar.UploadPhoto();

            _currentProfile.firstname = _displayNameIF.text;
            _currentProfile.nickname = _usernameIF.text;
            _currentProfile.about = _aboutIF.text;
            //_currentProfile.phone = _phoneIF.text;
            //_currentProfile.email = _emailIF.text;
            //_currentProfile.tags = _tagsList.ToArray();
            _currentProfile.avatar = path;

            //List<string> photos = new List<string>();
            //foreach (var photo in _photos)
            //{
            //    var url = await photo.UploadPhoto();
            //    if (!string.IsNullOrEmpty(url))
            //        photos.Add(url);
            //}

            //_currentProfile.photos = photos.ToArray();

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
            _genderLabel.SetText(newGender.uid == 2 ? "???????" : "???????");   // [TODO]: replace with configs
            _currentProfile.gender = newGender;
        }

        //void OnTagSelect(ContainerData data, ProfileSelectableTag tag)
        //{
        //    if (_tagsList.Exists(x => x.uid == data.uid))
        //    {
        //        var existing = _tagsList.Find(x => x.uid == data.uid);
        //        _tagsList.Remove(existing);
        //        tag.SetSelected(false);
        //    }
        //    else
        //    {
        //        _tagsList.Add(data);
        //        tag.SetSelected(true);
        //    }
        //}
    }
}
