using System;
using UnityEngine;
using WIGO.Core;

namespace WIGO
{
    [Serializable]
    public class ProfileData
    {
        public string uid;
        public string lang;
        public string firstname;
        public string lastname;
        public string birthday;
        public string nickname;
        public string email;
        public string phone;
        public string about;
        public ContainerData gender;
        public ContainerData[] tags;
        public string avatar;
        public string[] photos;
        public RatingData rating;

        Color _userColor;

        public ProfileData()
        {
            _userColor = GameConsts.GetRandomColor();
        }

        public Gender GetGender()
        {
            return gender.uid == 1 ? Gender.female : Gender.male;
        }

        public Color GetColor() => _userColor;

        public static ProfileData CopyProfile(ProfileData origin)
        {
            ProfileData copy = new ProfileData()
            {
                uid = origin.uid,
                lang = origin.lang,
                firstname = origin.firstname,
                lastname = origin.lastname,
                birthday = origin.birthday,
                nickname = origin.nickname,
                email = origin.email,
                phone = origin.phone,
                about = origin.about,
                gender = origin.gender,
                tags = origin.tags,
                avatar = origin.avatar,
                photos = origin.photos,
                rating = origin.rating
            };

            return copy;
        }
    }

    [Serializable]
    public struct ContainerData
    {
        public int uid;
        public string name;
    }

    [Serializable]
    public struct RatingData
    {
        public float author;
        public float member;
    }

    public enum Gender
    {
        male,
        female
    }
}
