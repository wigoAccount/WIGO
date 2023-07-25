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
            if (Enum.TryParse(gender.name, out Gender enumType))
            {
                return enumType;
            }

            return Gender.male;
        }

        public Color GetColor() => _userColor;
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
